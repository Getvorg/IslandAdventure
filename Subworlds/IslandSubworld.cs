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
        public override int Width => 1200;
        public override int Height => 600;
        public override bool ShouldSave => true;
        public override bool NormalUpdates => true;

        public override List<GenPass> Tasks => new List<GenPass>
        {
            new PassLegacy("Island", GenerateIsland)
        };

        private void GenerateIsland(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating island";
            const int seaLevel = 240;
            const int seaFloor = 440;
            int center = Width / 2;
            const int radius = 220;

            Main.worldSurface = 280;
            Main.rockLayer = 380;
            Main.spawnTileX = center;
            Main.spawnTileY = 209;

            for (int x = 0; x < Width; x++)
            {
                progress.Set((double)x / Width);
                double distance = Math.Abs(x - center) / (double)radius;
                int ground = seaFloor;
                if (distance < 1)
                {
                    // A broad, flat center gives the player a safe arrival area.
                    ground = 210 + (int)(230 * Math.Pow(distance, 2.4));
                }

                for (int y = seaLevel; y < ground; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.LiquidType = LiquidID.Water;
                    tile.LiquidAmount = byte.MaxValue;
                }

                for (int y = ground; y < Height; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;
                    tile.TileType = y < ground + 8
                        ? TileID.Sand
                        : y < ground + 45 ? TileID.Dirt : TileID.Stone;
                    tile.LiquidAmount = 0;
                }
            }

            Main.dayTime = true;
            Main.time = 27000;
            progress.Set(1);
        }
    }
}
