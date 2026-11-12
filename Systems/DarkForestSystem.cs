using System;
using IslandAdventure.Biomes;
using IslandAdventure.Content.Tiles;
using IslandAdventure.Content.Walls;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace IslandAdventure.Systems
{
    public class DarkForestSystem : ModSystem
    {
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (Main.gameMenu || !Main.LocalPlayer.InModBiome<DarkForestBiome>()) return;
            // The block atlases supply their own dark colors. Keep foreground
            // sunlight neutral; only dim the distant backdrop without a green tint.
            backgroundColor = new Color((int)(backgroundColor.R * .72f), (int)(backgroundColor.G * .72f), (int)(backgroundColor.B * .72f));
        }

        public static void ShapeTerrain(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Формирование холмов и низин тёмного леса";
            int left = 650, right = Main.maxTilesX - 650;
            double phase = WorldGen.genRand.NextDouble() * Math.PI * 2;
            int[] surface = new int[Main.maxTilesX];
            for (int x = left; x < right; x++)
                for (int y = 80; y < Main.worldSurface; y++)
                    if (Main.tile[x, y].HasTile) { surface[x] = y; break; }
            int changed = 0;
            for (int x = left; x < right; x++)
            {
                int old = surface[x]; if (old == 0) continue;
                float blend = MathHelper.Clamp(Math.Min(x - left, right - x) / 160f, 0, 1);
                // Broad wooded hills overlaid with shallower hollows. Vanilla caves,
                // ores and liquids are placed AFTER this pass, never shifted/copied.
                double shape = 27 * Math.Sin(x * .011 + phase) + 12 * Math.Sin(x * .027 - phase);
                int top = Math.Clamp(old + (int)(shape * blend), 95, Math.Max(96, (int)Main.worldSurface - 12));
                if (top > old)
                    for (int y = old; y < top; y++) { var t = Main.tile[x, y]; t.ClearEverything(); }
                else
                    for (int y = top; y < old; y++)
                    { var t = Main.tile[x, y]; t.HasTile = true; t.TileType = TileID.Dirt; }
                if (top != old) changed++;
            }
            ModContent.GetInstance<DarkForestSystem>().Mod.Logger.Info($"Dark forest terrain: {changed} columns shaped; coastal margins preserved.");
            progress.Set(1);
        }

        public static void PlantForest(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Посадка тёмного леса";
            int added = 0;
            double phase = WorldGen.genRand.NextDouble() * 6;
            // Break the dense canopy with occasional natural clearings.
            for (int x = 550; x < Main.maxTilesX - 550; x += WorldGen.genRand.Next(5, 9))
            {
                if (Math.Sin(x * .019 + phase) > .9) continue;
                for (int y = 80; y < Main.worldSurface; y++)
                {
                    var ground = Main.tile[x, y];
                    if (!ground.HasTile || !Main.tileSolid[ground.TileType]) continue;
                    if (ground.TileType == TileID.Grass)
                    {
                        // Remove only tiny vegetation in the tree footprint, not objects.
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            var above = Main.tile[x + dx, y - 1];
                            if (above.HasTile && (above.TileType == TileID.Plants || above.TileType == TileID.Plants2))
                                WorldGen.KillTile(x + dx, y - 1, noItem: true);
                        }
                        bool grown = WorldGen.genRand.NextBool(5) && WorldGen.GrowEpicTree(x, y);
                        if (!grown) grown = WorldGen.GrowTree(x, y);
                        if (grown) added++;
                    }
                    break;
                }
            }
            ModContent.GetInstance<DarkForestSystem>().Mod.Logger.Info($"Dark forest canopy: {added} extra vanilla trees; no custom blocks or paint.");
            progress.Set(1);
        }
    }
}
