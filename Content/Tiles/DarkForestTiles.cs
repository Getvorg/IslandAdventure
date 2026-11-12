using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using IslandAdventure.Content.Items;

namespace IslandAdventure.Content.Tiles
{
    public abstract class ForestGround : ModTile
    {
        protected abstract Color MapColor { get; }
        protected virtual bool Rock => false;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true; Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            DustType = Rock ? DustID.Stone : DustID.Dirt;
            HitSound = Rock ? SoundID.Tink : SoundID.Dig;
            AddMapEntry(MapColor, CreateMapEntryName());
            VanillaFallbackOnModDeletion = Rock ? TileID.Stone : TileID.Dirt;
        }
        public override void PostSetDefaults()
        {
            foreach (int other in new[] { ModContent.TileType<DarkSoilTile>(), ModContent.TileType<DarkGrassTile>(), ModContent.TileType<DarkStoneTile>() })
                Main.tileMerge[Type][other] = Main.tileMerge[other][Type] = true;
        }
    }
    public class DarkSoilTile : ForestGround
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/DarkForest/Soil";
        protected override Color MapColor => new Color(52, 35, 27);
        public override void SetStaticDefaults() { base.SetStaticDefaults(); RegisterItemDrop(ItemID.DirtBlock); }
    }
    public class DarkGrassTile : ForestGround
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/DarkForest/Grass";
        protected override Color MapColor => new Color(39, 45, 25);
        public override void SetStaticDefaults() { base.SetStaticDefaults(); RegisterItemDrop(ItemID.DirtBlock); }
        public override void RandomUpdate(int i, int j)
        {
            if (WorldGen.genRand.NextBool(8)) WorldGen.SpreadGrass(i, j, ModContent.TileType<DarkSoilTile>(), Type);
        }
    }
    public class DarkStoneTile : ForestGround
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/DarkForest/Stone";
        protected override bool Rock => true;
        protected override Color MapColor => new Color(43, 43, 49);
        public override void SetStaticDefaults() { base.SetStaticDefaults(); RegisterItemDrop(ItemID.StoneBlock); }
    }
    public class DarkWoodTile : ModTile
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/DarkForest/Wood";
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = Main.tileBlockLight[Type] = true;
            DustType = DustID.WoodFurniture; HitSound = SoundID.Dig;
            AddMapEntry(new Color(56, 33, 23), CreateMapEntryName());
            RegisterItemDrop(ItemID.Wood);
            VanillaFallbackOnModDeletion = TileID.WoodBlock;
        }
    }
    public class DarkPlantTile : ModTile
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/DarkForest/Plant";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true; Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true; Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<DarkGrassTile>(), ModContent.TileType<DarkSoilTile>() };
            TileObjectData.addTile(Type);
            DustType = DustID.Grass; HitSound = SoundID.Grass;
            AddMapEntry(new Color(39, 45, 25), CreateMapEntryName());
            VanillaFallbackOnModDeletion = TileID.Plants;
        }
    }
    public class DarkVineTile : ModTile
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/DarkForest/Vine";
        public override void SetStaticDefaults()
        {
            Main.tileCut[Type] = true; Main.tileNoFail[Type] = true; Main.tileLavaDeath[Type] = true;
            DustType = DustID.Grass; HitSound = SoundID.Grass;
            AddMapEntry(new Color(33, 40, 24), CreateMapEntryName());
            VanillaFallbackOnModDeletion = TileID.Vines;
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            var tile = Main.tile[i, j]; tile.TileFrameX = 0; tile.TileFrameY = (short)((i + j) % 3 * 18); return false;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!Main.tile[i,j-1].HasTile) WorldGen.KillTile(i,j);
        }
    }
}
