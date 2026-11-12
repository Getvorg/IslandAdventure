using System;
using IslandAdventure.Players;
using IslandAdventure.Subworlds;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace IslandAdventure.Systems
{
    public class ResumeSystem : ModSystem
    {
        public static bool Quitting { get; internal set; }
        public static bool QuitFromIsland => Main.netMode == NetmodeID.SinglePlayer
            && (Quitting || Main.ingameOptionsWindow) && !BoatTravelSystem.IsLoading;

        public override void Load() { if (!Main.dedServ) IL_IngameOptions.Draw += QuitLabel; }
        public override void Unload() { if (!Main.dedServ) IL_IngameOptions.Draw -= QuitLabel; Quitting = false; }
        private void QuitLabel(ILContext il)
        {
            // SL changes this one menu label to Return. Keep all its save logic,
            // but describe our island's direct-to-menu return destination accurately.
            var cursor = new ILCursor(il);
            if (cursor.TryGotoNext(i => i.MatchLdstr("Mods.SubworldLibrary.Return"))
                && cursor.TryGotoNext(MoveType.After, i => i.MatchCall(typeof(Language), "GetTextValue")))
                cursor.EmitDelegate<Func<string, string>>(text =>
                    SubworldSystem.IsActive<IslandSubworld>() && Main.netMode == NetmodeID.SinglePlayer
                        ? Lang.inter[35].Value : text);
            else Mod.Logger.Warn("Could not relabel the island quit button; direct save-and-quit still works.");
        }
        public static int ReturnDestination()
        {
            if (!QuitFromIsland) return -1;
            Main.LocalPlayer.GetModPlayer<ResumePlayer>().EndSession();
            Quitting = true;
            // Supported SL destination: save this subworld and its root cache,
            // then return directly to the title screen instead of loading the root.
            return int.MinValue;
        }
        public override void PreSaveAndQuit()
        {
            Main.LocalPlayer.GetModPlayer<ResumePlayer>().EndSession();
            // Terraria closes the options window BEFORE SL queries ReturnDestination.
            Quitting = Main.netMode == NetmodeID.SinglePlayer && SubworldSystem.IsActive<IslandSubworld>();
        }
        public static bool TrySafePosition(Player player, Vector2 wanted, out Vector2 result)
        {
            result = wanted;
            if (!float.IsFinite(wanted.X) || !float.IsFinite(wanted.Y)) return false;
            bool Safe(Vector2 p) => p.X >= 32 && p.Y >= 32
                && p.X + player.width < Main.maxTilesX * 16 - 32
                && p.Y + player.height < Main.maxTilesY * 16 - 32
                && !Collision.SolidCollision(p, player.width, player.height)
                && !Collision.LavaCollision(p, player.width, player.height);
            if (Safe(wanted)) return true;
            for (int radius = 1; radius <= 20; radius++)
                for (int y = -radius; y <= radius; y++)
                    for (int x = -radius; x <= radius; x++)
                    {
                        if (Math.Abs(x) != radius && Math.Abs(y) != radius) continue;
                        Vector2 candidate = wanted + new Vector2(x * 16, y * 16);
                        if (Safe(candidate)) { result = candidate; return true; }
                    }
            return false;
        }
    }
}
