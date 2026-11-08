using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using IslandAdventure.Content.NPCs.Bosses;
using IslandAdventure.Subworlds;
using SubworldLibrary;

namespace IslandAdventure.Systems
{
    public static class QueenEncounter
    {
        public static bool TryFindSea(Player player, out Vector2 surface)
        {
            surface = Vector2.Zero;
            if (!player.ZoneBeach) return false;
            Point origin = player.Center.ToTileCoordinates();
            float best = float.MaxValue;
            int top = Math.Max(20, origin.Y - 30), bottom = Math.Min(Main.maxTilesY - 240, origin.Y + 85);
            for (int x = Math.Max(20, origin.X - 60); x <= Math.Min(Main.maxTilesX - 21, origin.X + 60); x += 2)
            {
                if (x >= 500 && x <= Main.maxTilesX - 500) continue;
                for (int y = top; y < bottom; y++)
                {
                    if (!Water(x, y) || Water(x, y - 1)) continue;
                    if (!Water(x, y + 14) || !Water(x - 4, y + 8) || !Water(x + 4, y + 8)) continue;
                    float score = Math.Abs(x - origin.X) + Math.Abs(y - origin.Y) * 0.5f;
                    if (score >= best) continue;
                    best = score; surface = new Vector2(x * 16 + 8, y * 16);
                    break;
                }
            }
            return best < float.MaxValue;
        }

        private static bool Water(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.LiquidType == LiquidID.Water && tile.LiquidAmount > 127 && !WorldGen.SolidTile(x, y);
        }

        public static bool TrySummon(Player player, bool requireIsland, out string message)
        {
            string key = "Mods.IslandAdventure.QueenEncounter.";
            if (Main.netMode != NetmodeID.SinglePlayer)
            { message = Language.GetTextValue(key + "SinglePlayer"); return false; }
            if (NPC.AnyNPCs(ModContent.NPCType<MermaidQueen>()))
            { message = Language.GetTextValue(key + "AlreadyActive"); return false; }
            if (requireIsland && !SubworldSystem.IsActive<IslandSubworld>())
            { message = Language.GetTextValue(key + "IslandOnly"); return false; }
            if (BoatTravelSystem.IsCinematic || BoatTravelSystem.IsLoading || !TryFindSea(player, out Vector2 water))
            { message = Language.GetTextValue(key + "DeepOcean"); return false; }
            Vector2 center = new Vector2(water.X, Math.Max(140, water.Y - 180));
            int index = NPC.NewNPC(new EntitySource_Misc("IslandAdventure:QueenSummon"),
                (int)center.X, (int)center.Y + MermaidQueen.BodyHeight / 2,
                ModContent.NPCType<MermaidQueen>(), ai3: water.Y, Target: player.whoAmI);
            if (index >= Main.maxNPCs)
            { message = Language.GetTextValue(key + "NoSlot"); return false; }
            Main.npc[index].Center = center;
            Main.npc[index].netUpdate = true;
            message = Language.GetTextValue(key + "Summoned");
            return true;
        }
    }
}
