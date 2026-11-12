using System;
using System.Collections.Generic;
using IslandAdventure.Subworlds;
using IslandAdventure.Systems;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace IslandAdventure.Players
{
    // Character-owned records keyed by the MAIN world's UUID, never its name.
    public class ResumePlayer : ModPlayer
    {
        private readonly Dictionary<Guid, TagCompound> places = new();
        private Guid session;
        private TagCompound pending;
        private bool entering;
        private int loadAttempts;

        public override void SaveData(TagCompound tag)
        {
            Capture();
            tag["places"] = new List<TagCompound>(places.Values);
        }
        public override void LoadData(TagCompound tag)
        {
            places.Clear(); session = Guid.Empty; pending = null; entering = false;
            foreach (TagCompound place in tag.GetList<TagCompound>("places"))
                if (Guid.TryParse(place.GetString("world"), out Guid id) && id != Guid.Empty
                    && float.IsFinite(place.GetFloat("x")) && float.IsFinite(place.GetFloat("y")))
                    places[id] = place;
        }
        public override void OnEnterWorld()
        {
            if (Main.netMode != NetmodeID.SinglePlayer || Player.whoAmI != Main.myPlayer
                || SubworldSystem.AnyActive() || session != Guid.Empty) return;
            ResumeSystem.Quitting = false;
            session = Main.ActiveWorldFileData.UniqueId;
            places.TryGetValue(session, out pending);
            entering = false; loadAttempts = 0;
        }
        public void Capture()
        {
            if (Main.netMode != NetmodeID.SinglePlayer || Player.whoAmI != Main.myPlayer
                || session == Guid.Empty || pending != null || ResumeSystem.Quitting
                || WorldGen.generatingWorld || BoatTravelSystem.IsCinematic || BoatTravelSystem.IsLoading) return;
            if (Player.dead) { places.Remove(session); return; }
            bool island = ModContent.GetInstance<AnglerCompanionSystem>().WorldIsIsland;
            if (SubworldSystem.AnyActive() && !SubworldSystem.IsActive<IslandSubworld>()) return;
            if (!float.IsFinite(Player.position.X) || !float.IsFinite(Player.position.Y)) return;
            if (!places.TryGetValue(session, out TagCompound point))
                places[session] = point = new TagCompound { ["world"] = session.ToString() };
            point["island"] = island;
            point["x"] = Player.position.X; point["y"] = Player.position.Y;
            point["direction"] = Player.direction;
        }
        public void EndSession()
        {
            Capture();
            pending = null; entering = false; session = Guid.Empty;
        }
        public override void PostUpdate()
        {
            if (Main.netMode != NetmodeID.SinglePlayer || Player.whoAmI != Main.myPlayer
                || Main.gameMenu || WorldGen.generatingWorld || session == Guid.Empty) return;
            if (pending == null) { Capture(); return; }
            // Let the shared clock/NPC bridge finish its first world tick before
            // starting another load; otherwise we could copy the root file clock.
            if (WorldTravelStateSystem.HasPendingRestore) return;
            bool island = SubworldSystem.IsActive<IslandSubworld>();
            if (pending.GetBool("island") && !island)
            {
                if (SubworldSystem.AnyActive()) { pending = null; return; }
                if (!entering)
                {
                    entering = SubworldSystem.Enter<IslandSubworld>();
                    if (!entering && ++loadAttempts >= 3)
                    {
                        pending = null;
                        Main.NewText(Language.GetTextValue("Mods.IslandAdventure.Resume.LoadFailed"), Color.Wheat);
                    }
                }
                return;
            }
            if (pending.GetBool("island") != island) { pending = null; return; }
            Vector2 wanted = new Vector2(pending.GetFloat("x"), pending.GetFloat("y"));
            if (ResumeSystem.TrySafePosition(Player, wanted, out Vector2 safe))
            {
                Player.position = Player.oldPosition = safe;
                Player.gfxOffY = 0;
                Player.velocity = Vector2.Zero; Player.fallStart = (int)(safe.Y / 16);
                Player.direction = pending.GetInt("direction") < 0 ? -1 : 1;
                Player.immune = true; Player.immuneTime = Math.Max(Player.immuneTime, 120);
            }
            // No safe nearby space: keep the normal spawn, never excavate blocks.
            pending = null; entering = false;
            Capture();
        }
    }
}
