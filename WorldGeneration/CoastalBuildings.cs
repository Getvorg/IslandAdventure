using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using IslandAdventure.Systems;

namespace IslandAdventure.WorldGeneration
{
    // Buildings are authored in tiles; no external template library is needed.
    public static class CoastalBuildings
    {
        public static Rectangle PlaceFisherHouse()
        {
            FindShore(left: true, out int shoreX, out int seaY, out int shoreY);
            const int width = 23;
            int left = shoreX + 5;
            int floor = Math.Min(seaY - 3, shoreY - 1);
            for (int x = left; x < left + width; x++)
                floor = Math.Min(floor, FindGround(x) - 1);

            Rectangle area = new Rectangle(left - 4, floor - 22, width + 13, 34);
            ClearArea(area.Left, area.Top, area.Right, floor);
            for (int x = left; x < left + width; x++)
            {
                SetBlock(x, floor, TileID.RichMahogany);
                SetBlock(x, floor + 1, TileID.StoneSlab);
            }
            for (int x = left; x < left + width; x += 6)
                PlacePile(x, floor + 2);

            // Low sleeping annex joins a taller living room through an open arch.
            BuildRoom(left, floor, 8, 6);
            BuildRoom(left + 7, floor, 16, 10);
            for (int y = floor - 5; y < floor; y++)
            {
                Tile tile = Main.tile[left + 7, y];
                tile.ClearTile();
            }
            for (int offset = -2; offset <= 6; offset++)
            {
                int roofY = floor - 8 - (offset + 2) / 3;
                for (int y = roofY; y <= roofY + 1; y++)
                    SetBlock(left + offset, y, TileID.BorealWood);
                if (offset > 0)
                    FillWall(left + offset, roofY + 2, left + offset + 1, floor - 6, WallID.PalmWood);
            }
            for (int offset = 6; offset <= 24; offset++)
            {
                int roofY = floor - 16 + Math.Abs(offset - 14) / 2;
                SetBlock(left + offset, roofY, TileID.BorealWood);
                SetBlock(left + offset, roofY + 1, TileID.RichMahogany);
                if (offset > 7 && offset < 22)
                    FillWall(left + offset, roofY + 2, left + offset + 1, floor - 10, WallID.PalmWood);
            }
            // A capped brick chimney breaks up the silhouette.
            for (int x = left + 18; x <= left + 19; x++)
                for (int y = floor - 19; y <= floor - 12; y++)
                    SetBlock(x, y, TileID.RedBrick);
            for (int x = left + 17; x <= left + 20; x++)
                SetBlock(x, floor - 20, TileID.StoneSlab);

            FillWall(left + 2, floor - 5, left + 6, floor - 3, WallID.Glass);
            FillWall(left + 19, floor - 8, left + 22, floor - 6, WallID.Glass);
            Item door = new Item();
            door.SetDefaults(ItemID.PalmWoodDoor);
            foreach (int x in new[] { left, left + width - 1 })
            {
                for (int y = floor - 3; y < floor; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.ClearTile();
                }
                if (!WorldGen.PlaceDoor(x, floor - 2, TileID.ClosedDoor, door.placeStyle))
                    throw new InvalidOperationException("Could not place the fisher house door.");
            }

            PlaceOnSurface(ItemID.PalmWoodBed, left + 2, floor);
            PlaceOnSurface(ItemID.PalmWoodChair, left + 9, floor);
            PlaceOnSurface(ItemID.PalmWoodTable, left + 11, floor);
            PlaceOnSurface(ItemID.WorkBench, left + 14, floor);
            PlaceOnSurface(ItemID.Barrel, left + 20, floor);
            PlaceItem(ItemID.PalmWoodChandelier, left + 13, floor - 9);
            PlaceItem(ItemID.TreasureMap, left + 16, floor - 6);
            PlaceItem(ItemID.CompassRose, left + 13, floor - 14);
            for (int x = left + 9; x <= left + 11; x++)
                SetBlock(x, floor - 5, TileID.BorealWood);
            PlaceOnSurface(ItemID.ShipInABottle, left + 9, floor - 5);
            PlaceFishingChest(left + 17, floor);

            Rectangle pier = BuildPier(left - 1, shoreX - 27, floor, seaY);
            // Sheltered two-tile veranda, with a hanging palm-wood lantern.
            for (int x = left - 3; x < left; x++)
                SetBlock(x, floor - 6, TileID.BorealWood);
            for (int y = floor - 5; y < floor; y++)
                SetBlock(left - 3, y, TileID.WoodenBeam);
            PlaceItem(ItemID.PalmWoodLantern, left - 2, floor - 5);
            for (int step = 0; step < 7; step++)
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

        private static void BuildRoom(int left, int floor, int width, int height)
        {
            for (int x = left; x < left + width; x++)
            {
                SetBlock(x, floor - height, TileID.RichMahogany);
                for (int y = floor - height + 1; y < floor; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.WallType = y >= floor - 3 ? WallID.Planked : WallID.PalmWood;
                    if (x == left || x == left + width - 1)
                        SetBlock(x, y, y >= floor - 2 ? TileID.StoneSlab : TileID.PalmWood);
                }
            }
        }

        private static void FillWall(int left, int top, int right, int bottom, ushort type)
        {
            for (int x = left; x < right; x++)
                for (int y = top; y < bottom; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.WallType = type;
                }
        }

        private static Item Furniture(int itemId, out TileObjectData data)
        {
            Item item = new Item();
            item.SetDefaults(itemId);
            data = TileObjectData.GetTileData(item.createTile, item.placeStyle, 0);
            if (data == null)
                throw new InvalidOperationException("Missing furniture data for item " + itemId);
            return item;
        }

        private static void PlaceOnSurface(int itemId, int left, int floor)
        {
            Furniture(itemId, out TileObjectData data);
            PlaceItem(itemId, left, floor - data.Height);
        }

        private static void PlaceItem(int itemId, int left, int top)
        {
            Item item = Furniture(itemId, out TileObjectData data);
            if (TileID.Sets.BasicChest[item.createTile])
            {
                if (WorldGen.PlaceChest(left + data.Origin.X, top + data.Origin.Y,
                    (ushort)item.createTile, style: item.placeStyle) < 0)
                    throw new InvalidOperationException("Could not place storage item " + itemId);
            }
            else
                WorldGen.PlaceObject(left + data.Origin.X, top + data.Origin.Y,
                    item.createTile, mute: true, style: item.placeStyle, direction: 1);
            Tile tile = Main.tile[left, top];
            if (!tile.HasTile || tile.TileType != item.createTile)
                throw new InvalidOperationException($"Could not place decoration {itemId} at {left}, {top}.");
        }

        private static void PlaceFishingChest(int left, int floor)
        {
            Item item = Furniture(ItemID.CoralChest, out TileObjectData data);
            int chestIndex = WorldGen.PlaceChest(left + data.Origin.X,
                floor - data.Height + data.Origin.Y, (ushort)item.createTile, style: item.placeStyle);
            if (chestIndex < 0)
                throw new InvalidOperationException("Could not place the fishing supplies chest.");
            Chest chest = Main.chest[chestIndex];
            chest.name = "Снасти Рыбака";
            int[] types = { ItemID.ReinforcedFishingPole, ItemID.ApprenticeBait,
                ItemID.FishingPotion, ItemID.SonarPotion, ItemID.CratePotion,
                ItemID.Rope, ItemID.Glowstick, ItemID.Bass };
            int[] stacks = { 1, 25, 3, 3, 3, 75, 20, 5 };
            for (int i = 0; i < types.Length; i++)
            {
                chest.item[i].SetDefaults(types[i]);
                chest.item[i].stack = stacks[i];
            }
        }

        public static Rectangle PlaceIslandPier()
        {
            FindShore(left: false, out int shoreX, out int seaY, out int shoreY);
            int start = shoreX - 6;
            int floor = Math.Min(seaY - 3, shoreY - 1);
            for (int x = start; x <= shoreX; x++)
                floor = Math.Min(floor, FindGround(x) - 1);

            Rectangle footprint = BuildPier(start, shoreX + 30, floor, seaY);
            WorldGen.RangeFrame(footprint.Left - 2, footprint.Top - 2,
                footprint.Right + 2, Math.Min(Main.maxTilesY - 2, footprint.Bottom + 2));
            GenVars.structures.AddProtectedStructure(footprint);
            return footprint;
        }

        private static Rectangle BuildPier(int start, int end, int floor, int seaY)
        {
            int left = Math.Min(start, end);
            int right = Math.Max(start, end);
            bool facesLeft = end < start;
            ClearArea(left, floor - 7, right + 1, floor);
            for (int x = left; x <= right; x++)
            {
                SetBlock(x, floor, (x - left) % 8 == 0 ? TileID.BorealWood : TileID.PalmWood);
                if ((x - left) % 10 < 7)
                    FillWall(x, floor - 2, x + 1, floor, WallID.WoodenFence);
            }
            int deepest = floor + 1;
            for (int x = left + 2; x <= right; x += 9)
                deepest = Math.Max(deepest, PlacePile(x, floor + 1));

            int post = facesLeft ? left + 2 : right - 2;
            int lamp = facesLeft ? post + 2 : post - 3;
            for (int y = floor - 5; y < floor; y++)
                SetBlock(post, y, TileID.BorealWood);
            for (int x = Math.Min(post, lamp); x <= Math.Max(post, lamp) + 1; x++)
                SetBlock(x, floor - 5, TileID.RichMahogany);
            PlaceItem(ItemID.PalmWoodLantern, lamp, floor - 4);

            int middle = (left + right) / 2;
            PlaceOnSurface(ItemID.OceanCrate, middle, floor);
            PlaceOnSurface(ItemID.Barrel, middle + 4, floor);
            for (int y = floor + 1; y <= floor + 6; y++)
            {
                Tile tile = Main.tile[post + (facesLeft ? -1 : 1), y];
                if (tile.HasTile)
                    break;
                tile.HasTile = true;
                tile.TileType = TileID.Rope;
            }
            ModContent.GetInstance<HarborSystem>().SetHarbor(
                new Point(facesLeft ? right - 5 : left + 3, floor),
                new Point(end + (facesLeft ? -5 : 5), seaY), facesLeft);
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
