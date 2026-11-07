using System;
using System.Collections.Generic;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;

namespace IslandAdventure.Subworlds
{
    public class IslandSubworld : Subworld
    {
        public override int Width => 2400;
        public override int Height => 1000;
        public override bool ShouldSave => true;
        public override bool NormalUpdates => true;

        public override List<GenPass> Tasks => new List<GenPass>
        {
            new PassLegacy("Forest", GenerateForest)
        };

        private void GenerateForest(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Growing a forest";
            int center = Width / 2;
            int[] surface = new int[Width];
            double phase = WorldGen.genRand.NextDouble() * Math.PI * 2;
            double secondPhase = WorldGen.genRand.NextDouble() * Math.PI * 2;

            Main.worldSurface = 360;
            Main.rockLayer = 480;
            Main.spawnTileX = center;
            Main.spawnTileY = 299;
            Main.dungeonX = 0;
            Main.dungeonY = 0;

            for (int x = 0; x < Width; x++)
            {
                // Blend hills into a flat, clear arrival area in the center.
                double blend = Math.Clamp((Math.Abs(x - center) - 14) / 65.0, 0, 1);
                blend = blend * blend * (3 - 2 * blend);
                double hills = 22 * Math.Sin(x / 95.0 + phase)
                    + 9 * Math.Sin(x / 33.0 + secondPhase);
                int ground = 300 + (int)(hills * blend);
                surface[x] = ground;

                for (int y = ground; y < Height; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;
                    tile.TileType = y < ground + 65 + (int)(8 * Math.Sin(x / 47.0))
                        ? TileID.Dirt : TileID.Stone;
                    tile.LiquidAmount = 0;
                    if (y > ground + 2 && y < Main.rockLayer)
                        tile.WallType = WallID.DirtUnsafe;
                }
                progress.Set(0.4 * x / Width);
            }

            // Carve wandering tunnels below a thick intact surface layer.
            for (int cave = 0; cave < 55; cave++)
            {
                double caveX = WorldGen.genRand.Next(60, Width - 60);
                double caveY = WorldGen.genRand.Next(420, Height - 100);
                double direction = WorldGen.genRand.NextDouble() * Math.PI * 2;
                int steps = WorldGen.genRand.Next(30, 80);
                for (int step = 0; step < steps; step++)
                {
                    int radius = WorldGen.genRand.Next(4, 9);
                    CarveCave((int)caveX, (int)caveY, radius, surface);
                    direction += (WorldGen.genRand.NextDouble() - 0.5) * 0.5;
                    caveX = Math.Clamp(caveX + Math.Cos(direction) * 4, 30, Width - 31);
                    caveY = Math.Clamp(caveY + Math.Sin(direction) * 3, 410, Height - 70);
                }
                progress.Set(0.4 + 0.3 * (cave + 1) / 55);
            }

            // Grass on exposed dirt, including the sides of slopes.
            for (int x = 1; x < Width - 1; x++)
            {
                int exposedBottom = Math.Max(surface[x], Math.Max(surface[x - 1], surface[x + 1]));
                for (int y = surface[x]; y <= exposedBottom; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && tile.TileType == TileID.Dirt)
                        tile.TileType = TileID.Grass;
                }
            }
            WorldGen.RangeFrame(1, 1, Width - 1, Height - 1);
            progress.Set(0.8);

            // Vanilla tree growth supplies real trunks, branches and leaf crowns.
            for (int x = 30; x < Width - 30; x += WorldGen.genRand.Next(7, 13))
            {
                if (Math.Abs(x - center) > 18)
                    WorldGen.GrowTree(x, surface[x]);
            }
            for (int x = 20; x < Width - 20; x++)
            {
                if (Math.Abs(x - center) > 8 && WorldGen.genRand.Next(3) == 0)
                    WorldGen.PlaceTile(x, surface[x] - 1, TileID.Plants, mute: true,
                        style: WorldGen.genRand.Next(6));
            }

            Main.dayTime = true;
            Main.time = 27000;
            progress.Set(1);
        }

        private void CarveCave(int centerX, int centerY, int radius, int[] surface)
        {
            for (int x = Math.Max(1, centerX - radius); x <= Math.Min(Width - 2, centerX + radius); x++)
            {
                for (int y = Math.Max(surface[x] + 70, centerY - radius);
                    y <= Math.Min(Height - 40, centerY + radius); y++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        Main.tile[x, y].HasTile = false;
                        Main.tile[x, y].LiquidAmount = 0;
                    }
                }
            }
        }
    }
}
