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
            // Keep the day/night cycle and personal lights; never force permanent night.
            tileColor = new Color((int)(tileColor.R * .72f), (int)(tileColor.G * .82f), (int)(tileColor.B * .78f));
            backgroundColor = new Color((int)(backgroundColor.R * .55f), (int)(backgroundColor.G * .68f), (int)(backgroundColor.B * .68f));
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
            int added = 0, painted = 0;
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
            int soil = ModContent.TileType<DarkSoilTile>(), grass = ModContent.TileType<DarkGrassTile>();
            int stone = ModContent.TileType<DarkStoneTile>(), plant = ModContent.TileType<DarkPlantTile>();
            int vine = ModContent.TileType<DarkVineTile>();
            // Actual custom materials, not a paint overlay. Preserve ocean sand,
            // ore veins, liquids, containers and the Underworld.
            for (int x = 500; x < Main.maxTilesX - 500; x++)
                for (int y = 30; y < Main.maxTilesY - 200; y++)
                {
                    var tile = Main.tile[x, y];
                    if (tile.HasTile)
                    {
                        int replacement = tile.TileType switch {
                            TileID.Dirt => soil, TileID.Grass => grass, TileID.Stone => stone,
                            TileID.Plants or TileID.Plants2 => plant, TileID.Vines => vine, _ => -1
                        };
                        if (replacement >= 0)
                        {
                            tile.TileType = (ushort)replacement; tile.TileColor = PaintID.None;
                            if (replacement == plant) { tile.TileFrameX = (short)((x % 3) * 18); tile.TileFrameY = 0; }
                            if (replacement == vine) { tile.TileFrameX = 0; tile.TileFrameY = (short)((x+y)%3*18); }
                            painted++;
                        }
                        if (tile.TileType == TileID.Trees) tile.TileColor = PaintID.None;
                    }
                    if (tile.WallType is WallID.DirtUnsafe or WallID.DirtUnsafe1 or WallID.DirtUnsafe2
                        or WallID.DirtUnsafe3 or WallID.DirtUnsafe4 or WallID.GrassUnsafe or WallID.FlowerUnsafe)
                        tile.WallType = (ushort)ModContent.WallType<DarkSoilWall>();
                    else if (tile.WallType is WallID.Stone or WallID.RocksUnsafe1 or WallID.RocksUnsafe2
                        or WallID.RocksUnsafe3 or WallID.RocksUnsafe4)
                        tile.WallType = (ushort)ModContent.WallType<DarkStoneWall>();
                }
            WorldGen.RangeFrame(498, 28, Main.maxTilesX - 498, Main.maxTilesY - 198);
            ModContent.GetInstance<DarkForestSystem>().Mod.Logger.Info($"Dark forest canopy: {added} extra trees, {painted} custom material tiles.");
            progress.Set(1);
        }
    }
}
