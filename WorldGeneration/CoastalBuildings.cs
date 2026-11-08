using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace IslandAdventure.WorldGeneration
{
    // Buildings are authored in tiles; no external template library is needed.
    public static class CoastalBuildings
    {
        public static Rectangle PlaceFisherHouse()
        {
            FindShore(left: true, out int shoreX, out int seaY, out int shoreY);
            const int width = 29;
            int left = shoreX + 8;
            int floor = Math.Min(seaY - 3, shoreY - 1);
            for (int x = left; x < left + width; x++)
                floor = Math.Min(floor, FindGround(x) - 1);

            Rectangle area = new Rectangle(left - 3, floor - 21, width + 16, 33);
            ClearArea(area.Left, area.Top, area.Right, floor);

            // Raised timber floor and masonry plinth; piles reach the beach.
            for (int x = left; x < left + width; x++)
            {
                SetBlock(x, floor, TileID.PalmWood);
                SetBlock(x, floor + 1, TileID.GrayBrick);
                SetBlock(x, floor + 2, TileID.GrayBrick);
            }
            for (int x = left; x < left + width; x += 7)
                PlacePile(x, floor + 3);

            for (int x = left; x < left + width; x++)
            {
                SetBlock(x, floor - 10, TileID.PalmWood);
                for (int y = floor - 9; y < floor; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.WallType = WallID.Wood;
                    if (x == left || x == left + width - 1)
                        SetBlock(x, y, TileID.PalmWood);
                }
            }

            // A stepped red gable with a timber-lined attic.
            for (int offset = -2; offset < width + 2; offset++)
            {
                int roofY = floor - 18 + Math.Abs(offset - width / 2) / 2;
                for (int y = roofY; y <= roofY + 1; y++)
                    SetBlock(left + offset, y, TileID.RedBrick);
                if (offset > 0 && offset < width - 1)
                {
                    for (int y = roofY + 2; y < floor - 10; y++)
                    {
                        Tile tile = Main.tile[left + offset, y];
                        tile.WallType = WallID.Wood;
                    }
                }
            }

            // Glass is background wall, so the room remains walkable.
            foreach (int offset in new[] { 5, 19 })
            {
                for (int x = left + offset; x < left + offset + 4; x++)
                    for (int y = floor - 7; y < floor - 4; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        tile.WallType = WallID.Glass;
                    }
            }

            // PlaceDoor takes the middle of the three-tile opening.
            foreach (int x in new[] { left, left + width - 1 })
            {
                for (int y = floor - 3; y < floor; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.ClearTile();
                }
                if (!WorldGen.PlaceDoor(x, floor - 2, TileID.ClosedDoor))
                    throw new InvalidOperationException("Could not place the fisher house door.");
            }

            WorldGen.PlaceTile(left + 12, floor - 1, TileID.Tables, mute: true);
            WorldGen.PlaceTile(left + 9, floor - 1, TileID.Chairs, mute: true);
            WorldGen.PlaceTile(left + 15, floor - 1, TileID.Chairs, mute: true);
            WorldGen.PlaceTile(left + 23, floor - 1, TileID.WorkBenches, mute: true);
            WorldGen.PlaceTile(left + 2, floor - 6, TileID.Torches, mute: true);
            WorldGen.PlaceTile(left + 26, floor - 6, TileID.Torches, mute: true);

            Rectangle pier = BuildPier(left - 1, shoreX - 62, floor);
            // Short steps connect the raised house to the inland beach.
            for (int step = 0; step < 10; step++)
            {
                int x = left + width + step;
                int y = Math.Min(floor + step / 2, FindGround(x));
                SetBlock(x, y, TileID.PalmWood);
            }
            Rectangle footprint = Rectangle.Union(area, pier);
            WorldGen.RangeFrame(footprint.Left - 2, footprint.Top - 2,
                footprint.Right + 2, Math.Min(Main.maxTilesY - 2, footprint.Bottom + 2));
            GenVars.structures.AddProtectedStructure(footprint);
            return footprint;
        }

        public static Rectangle PlaceIslandPier()
        {
            FindShore(left: false, out int shoreX, out int seaY, out int shoreY);
            int start = shoreX - 12;
            int floor = Math.Min(seaY - 3, shoreY - 1);
            for (int x = start; x <= shoreX; x++)
                floor = Math.Min(floor, FindGround(x) - 1);

            Rectangle footprint = BuildPier(start, shoreX + 62, floor);
            WorldGen.RangeFrame(footprint.Left - 2, footprint.Top - 2,
                footprint.Right + 2, Math.Min(Main.maxTilesY - 2, footprint.Bottom + 2));
            GenVars.structures.AddProtectedStructure(footprint);
            return footprint;
        }

        private static Rectangle BuildPier(int start, int end, int floor)
        {
            int left = Math.Min(start, end);
            int right = Math.Max(start, end);
            ClearArea(left, floor - 7, right + 1, floor);
            for (int x = left; x <= right; x++)
            {
                SetBlock(x, floor, TileID.PalmWood);
                for (int y = floor - 2; y < floor; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.WallType = WallID.WoodenFence;
                }
            }

            int deepest = floor + 1;
            for (int x = left + 2; x <= right; x += 8)
                deepest = Math.Max(deepest, PlacePile(x, floor + 1));
            foreach (int x in new[] { left + 3, right - 3 })
            {
                for (int y = floor - 4; y < floor; y++)
                    SetBlock(x, y, TileID.PalmWood);
                WorldGen.PlaceTile(x, floor - 5, TileID.Torches, mute: true);
            }
            WorldGen.PlaceObject((left + right) / 2, floor - 1, TileID.FishingCrate, mute: true);
            return new Rectangle(left, floor - 8, right - left + 1, deepest - floor + 9);
        }

        private static int PlacePile(int x, int top)
        {
            int bottom = top;
            while (bottom < Main.maxTilesY - 220 && !WorldGen.SolidTile(x, bottom))
            {
                // Beams are not solid; preserve water around submerged piles.
                Tile tile = Main.tile[x, bottom];
                byte amount = tile.LiquidAmount;
                int liquid = tile.LiquidType;
                tile.ClearTile();
                tile.HasTile = true;
                tile.TileType = TileID.WoodenBeam;
                tile.LiquidType = liquid;
                tile.LiquidAmount = amount;
                bottom++;
            }
            return bottom;
        }

        private static void FindShore(bool left, out int shoreX, out int seaY, out int shoreY)
        {
            bool foundWater = false;
            seaY = 0;
            for (int distance = 60; distance < 750; distance++)
            {
                int x = left ? distance : Main.maxTilesX - 1 - distance;
                int ground = FindGround(x);
                int waterTop = -1;
                for (int y = 60; y < ground; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.LiquidAmount > 127 && tile.LiquidType == LiquidID.Water)
                    {
                        waterTop = y;
                        break;
                    }
                }
                if (waterTop >= 0)
                {
                    seaY = waterTop;
                    foundWater = true;
                }
                else if (foundWater && Math.Abs(ground - seaY) < 20)
                {
                    shoreX = x;
                    shoreY = ground;
                    return;
                }
            }
            throw new InvalidOperationException("Could not find a safe " + (left ? "left" : "right") + " ocean shore.");
        }

        private static int FindGround(int x)
        {
            int limit = Math.Min(Main.maxTilesY - 220, (int)Main.worldSurface + 300);
            for (int y = 60; y < limit; y++)
                if (WorldGen.SolidTile(x, y))
                    return y;
            throw new InvalidOperationException("Could not find coastal ground at x=" + x);
        }

        private static void SetBlock(int x, int y, ushort type)
        {
            Tile tile = Main.tile[x, y];
            tile.ClearTile();
            tile.HasTile = true;
            tile.TileType = type;
            tile.LiquidAmount = 0;
        }

        private static void ClearArea(int left, int top, int right, int bottom)
        {
            for (int x = left; x < right; x++)
                for (int y = top; y < bottom; y++)
                {
                    // Kill whole natural multitiles (palms, plants) before clearing
                    // the footprint so cropped trunks/furniture are not left behind.
                    WorldGen.KillTile(x, y, noItem: true);
                    Tile tile = Main.tile[x, y];
                    tile.ClearEverything();
                }
        }
    }
}
