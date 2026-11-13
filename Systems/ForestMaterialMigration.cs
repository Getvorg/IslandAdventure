using System;
using IslandAdventure.Content.Items;
using IslandAdventure.Content.Tiles;
using IslandAdventure.Content.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SubworldLibrary;
using IslandAdventure.Subworlds;

namespace IslandAdventure.Systems
{
    // Legacy type registrations remain solely to read 0.3.2 saves safely.
    public class ForestMaterialMigration : ModSystem
    {
        private bool pending;
        public override void OnWorldLoad() => pending = true;
        public static int VanillaTile(int type) => type == ModContent.TileType<DarkSoilTile>() ? TileID.Dirt
            : type == ModContent.TileType<DarkGrassTile>() || type == ModContent.TileType<GroveGrass>() ? TileID.Grass
            : type == ModContent.TileType<DarkStoneTile>() ? TileID.Stone
            : type == ModContent.TileType<DarkWoodTile>() ? TileID.WoodBlock
            : type == ModContent.TileType<DarkPlantTile>() ? TileID.Plants
            : type == ModContent.TileType<DarkVineTile>() ? TileID.Vines : type;
        public static void ConvertItem(Item item)
        {
            if (item == null) return;
            int type = item.type == ModContent.ItemType<DarkSoil>() ? ItemID.DirtBlock
                : item.type == ModContent.ItemType<DarkStone>() ? ItemID.StoneBlock
                : item.type == ModContent.ItemType<DarkWood>() ? ItemID.Wood : -1;
            if (type < 0) return;
            int stack = item.stack; bool favorite = item.favorited;
            item.SetDefaults(type); item.stack = stack; item.favorited = favorite;
        }
        public override void PreUpdateWorld()
        {
            if (!pending || Main.gameMenu || WorldGen.generatingWorld) return;
            pending = false;
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            int soilWall = ModContent.WallType<DarkSoilWall>(), stoneWall = ModContent.WallType<DarkStoneWall>();
            var mapping = new System.Collections.Generic.Dictionary<int,int>();
            foreach (int id in new[] { ModContent.TileType<DarkSoilTile>(), ModContent.TileType<DarkGrassTile>(), ModContent.TileType<GroveGrass>(), ModContent.TileType<DarkStoneTile>(), ModContent.TileType<DarkWoodTile>(), ModContent.TileType<DarkPlantTile>(), ModContent.TileType<DarkVineTile>() })
                mapping[id] = VanillaTile(id);
            bool island = SubworldSystem.IsActive<IslandSubworld>();
            int minX = Main.maxTilesX, minY = Main.maxTilesY, maxX = 0, maxY = 0, changed = 0;
            for (int x = 1; x < Main.maxTilesX - 1; x++)
                for (int y = 1; y < Main.maxTilesY - 1; y++)
                {
                    var tile = Main.tile[x,y]; bool altered = false;
                    if (tile.HasTile)
                    {
                        if (mapping.TryGetValue(tile.TileType, out int type)) { tile.TileType = (ushort)type; altered = true; }
                    }
                    if (island && x >= 500 && x < Main.maxTilesX - 500
                        && tile.HasTile && tile.TileType is TileID.Grass or TileID.Plants or TileID.Plants2 or TileID.Vines or TileID.Trees
                        && tile.TileColor is PaintID.DeepGreenPaint or PaintID.DeepTealPaint)
                    { tile.TileColor = PaintID.None; altered = true; }
                    if (tile.WallType == soilWall) { tile.WallType = WallID.DirtUnsafe; altered = true; }
                    else if (tile.WallType == stoneWall) { tile.WallType = WallID.Stone; altered = true; }
                    if (!altered) continue;
                    minX = Math.Min(minX,x); minY = Math.Min(minY,y); maxX = Math.Max(maxX,x); maxY = Math.Max(maxY,y); changed++;
                }
            if (changed > 0) WorldGen.RangeFrame(Math.Max(1,minX-2), Math.Max(1,minY-2), Math.Min(Main.maxTilesX-2,maxX+3), Math.Min(Main.maxTilesY-2,maxY+3));
            foreach (var chest in Main.chest) if (chest != null) foreach (var item in chest.item) ConvertItem(item);
            foreach (var item in Main.item) ConvertItem(item);
            if (changed > 0) Mod.Logger.Info($"Restored {changed} legacy forest tiles/walls to vanilla materials.");
        }
    }
    public class ForestInventoryMigration : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer) return;
            foreach (var item in Player.inventory) ForestMaterialMigration.ConvertItem(item);
            foreach (var item in Player.bank.item) ForestMaterialMigration.ConvertItem(item);
            foreach (var item in Player.bank2.item) ForestMaterialMigration.ConvertItem(item);
            foreach (var item in Player.bank3.item) ForestMaterialMigration.ConvertItem(item);
            foreach (var item in Player.bank4.item) ForestMaterialMigration.ConvertItem(item);
            ForestMaterialMigration.ConvertItem(Player.trashItem);
        }
    }
}
