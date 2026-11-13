using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;

namespace IslandAdventure.Content.Tiles
{
    public class GroveGrass : ModTile
    {
        // Actual dirt pixels remain the installed vanilla dirt; only the rim is new.
        public override string Texture => $"Terraria/Images/Tiles_{TileID.Dirt}";
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileID.Dirt] = Main.tileMerge[TileID.Dirt][Type] = true;
            DustType = DustID.Grass; HitSound = SoundID.Dig;
            RegisterItemDrop(ItemID.DirtBlock); VanillaFallbackOnModDeletion = TileID.Grass;
            AddMapEntry(new Color(62, 78, 35), CreateMapEntryName());
        }
        public override void PostDraw(int i, int j, SpriteBatch batch)
        {
            if (WorldGen.SolidTile(i,j-1)) return;
            var tile = Main.tile[i,j];
            if (tile.Slope is SlopeType.SlopeUpLeft or SlopeType.SlopeUpRight) return;
            var texture = ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/GrassEdge", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 point = new Vector2(i*16,j*16) - Main.screenPosition;
            if (!Main.drawToScreen) point += new Vector2(Main.offScreenRange);
            int frame = (i % 3) * 18;
            Color light = Lighting.GetColor(i,j);
            // Pixel-column clipping follows vanilla hammered surfaces exactly.
            for (int x=0; x<16; x++)
            {
                int top = tile.IsHalfBlock ? 8 : tile.Slope == SlopeType.SlopeDownRight ? 15-x
                    : tile.Slope == SlopeType.SlopeDownLeft ? x : 0;
                int height = System.Math.Min(8,16-top);
                batch.Draw(texture,point+new Vector2(x,top),new Rectangle(frame+x,0,1,height),light);
            }
            // A root fan follows the living tree; it occupies no extra collision tiles.
            // Draw once at the leftmost base tile, including native flared tree bases.
            if (Main.tile[i,j-1].HasTile && Main.tile[i,j-1].TileType == TileID.Trees
                && !(Main.tile[i-1,j-1].HasTile && Main.tile[i-1,j-1].TileType == TileID.Trees))
            {
                var roots = ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree_Roots", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                batch.Draw(roots,point + new Vector2(8-roots.Width/2,-roots.Height+6),light);
            }
        }
    }
    public abstract class GrovePlant : ModTile
    {
        protected virtual int PlantWidth => 2;
        protected virtual int PlantHeight => 3;
        protected virtual bool Variants => false;
        protected virtual bool Cuttable => true;
        protected virtual int MaterialDrop => 0;
        protected virtual Color MapColor => new Color(56,110,66);
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = Main.tileNoFail[Type] = Main.tileLavaDeath[Type] = true;
            Main.tileCut[Type] = Cuttable;
            Main.tileAxe[Type] = MaterialDrop == ItemID.Wood;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = PlantWidth;
            TileObjectData.newTile.Height = PlantHeight;
            TileObjectData.newTile.Origin = new Point16(PlantWidth / 2, PlantHeight - 1);
            TileObjectData.newTile.CoordinateHeights = new int[PlantHeight];
            for (int row=0;row<PlantHeight;row++) TileObjectData.newTile.CoordinateHeights[row]=16;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, PlantWidth, 0);
            TileObjectData.newTile.AnchorValidTiles = new[] { TileID.Dirt, TileID.Grass, ModContent.TileType<GroveGrass>() };
            if (Variants) { TileObjectData.newTile.StyleHorizontal = true; TileObjectData.newTile.RandomStyleRange = 3; }
            TileObjectData.addTile(Type);
            DustType = MaterialDrop == ItemID.StoneBlock ? DustID.Stone : MaterialDrop == ItemID.Wood ? DustID.WoodFurniture : DustID.Grass;
            HitSound = MaterialDrop == ItemID.StoneBlock ? SoundID.Tink : MaterialDrop == ItemID.Wood ? SoundID.Dig : SoundID.Grass;
            if (MaterialDrop != 0) RegisterItemDrop(MaterialDrop);
            AddMapEntry(MapColor,CreateMapEntryName());
            VanillaFallbackOnModDeletion = TileID.Plants;
        }
    }
    public class GroveBush : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/bush";
        protected override int PlantWidth => 7;
        protected override int PlantHeight => 5;
    }
    public class GroveFern : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/fern";
        protected override int PlantWidth => 8;
        protected override int PlantHeight => 6;
    }
    public class GroveTuft : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Tuft";
        protected override int PlantWidth => 2;
        protected override int PlantHeight => 1;
        protected override bool Variants => true;
    }
    public class GroveTallGrass : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/TallGrass";
        protected override int PlantWidth => 3;
        protected override int PlantHeight => 4;
    }
    public class GroveSmallBush : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/SmallBush";
        protected override int PlantWidth => 4;
        protected override int PlantHeight => 3;
    }
    public class GroveSmallFern : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/SmallFern";
        protected override int PlantWidth => 5;
        protected override int PlantHeight => 4;
    }
    public class GroveMoss : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Moss";
        protected override int PlantWidth => 4;
        protected override int PlantHeight => 1;
    }
    public abstract class GroveWoodDecoration : GrovePlant
    {
        protected override bool Cuttable => false;
        protected override int MaterialDrop => ItemID.Wood;
        protected override Color MapColor => new Color(87,61,42);
    }
    public class GroveRoots : GroveWoodDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Roots";
        protected override int PlantWidth => 6;
        protected override int PlantHeight => 2;
    }
    public class GroveFallenLog : GroveWoodDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/FallenLog";
        protected override int PlantWidth => 7;
        protected override int PlantHeight => 3;
    }
    public class GroveHollowLog : GroveWoodDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/HollowLog";
        protected override int PlantWidth => 10;
        protected override int PlantHeight => 4;
    }
    public abstract class GroveStoneDecoration : GrovePlant
    {
        protected override bool Cuttable => false;
        protected override int MaterialDrop => ItemID.StoneBlock;
        protected override Color MapColor => new Color(103,126,130);
    }
    public class GrovePebble : GroveStoneDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Pebble";
        protected override int PlantWidth => 2;
        protected override int PlantHeight => 1;
    }
    public class GroveRock : GroveStoneDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Rock";
        protected override int PlantWidth => 4;
        protected override int PlantHeight => 3;
    }
    public class GroveLargeRock : GroveStoneDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/LargeRock";
        protected override int PlantWidth => 6;
        protected override int PlantHeight => 4;
    }
    public class GroveBoulder : GroveStoneDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Boulder";
        protected override int PlantWidth => 10;
        protected override int PlantHeight => 8;
    }
}
