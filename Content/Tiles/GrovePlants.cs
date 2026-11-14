using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;

namespace IslandAdventure.Content.Tiles
{
    public class GroveGrass : ModTile
    {
        public const int RootSink = 14;
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
            => DrawTurf(i,j,batch);
        public static void DrawTurf(int i,int j,SpriteBatch batch)
        {
            var tile = Main.tile[i,j];
            if(!TileDrawing.IsVisible(tile)||tile.TileColor!=PaintID.None)return;
            var texture = ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/GrassEdge", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 point = new Vector2(i*16,j*16) - Main.screenPosition;
            if (!Main.drawToScreen) point += new Vector2(Main.offScreenRange);
            Color light = tile.IsTileFullbright?Color.White:Lighting.GetColor(i,j);
            if(tile.IsActuated)light*=.4f;
            GroveTurfShape.Draw(i,j,batch,texture,point,light);
            // A root fan follows the living tree; it occupies no extra collision tiles.
            // Draw once at the trunk center, rather than offsetting it to a flared side root.
            if (Main.tile[i,j-1].HasTile && Main.tile[i,j-1].TileType == TileID.Trees)
            {
                WorldGen.GetTreeBottom(i,j-1,out int rootX,out int rootY);
                if(rootX!=i||rootY!=j)return;
                var roots = ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree_Roots", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                batch.Draw(roots,point + new Vector2(8-roots.Width/2,-roots.Height+RootSink),light);
            }
        }
    }
    public abstract class GrovePlant : ModTile
    {
        public virtual float SwayAmplitude => 0f;
        protected virtual int PlantWidth => 2;
        protected virtual int PlantHeight => 3;
        protected virtual bool Variants => false;
        protected virtual bool Cuttable => true;
        protected virtual int SurfaceOffset => 4;
        protected virtual int MaterialDrop => 0;
        protected virtual Color MapColor => new Color(56,110,66);
        public override bool PreDraw(int i,int j,SpriteBatch batch)
        {
            if(SwayAmplitude<=0)return true;
            if(TileObjectData.IsTopLeft(i,j))Main.instance.TilesRenderer.AddSpecialPoint(i,j,TileDrawing.TileCounterType.CustomNonSolid);
            return false;
        }
        public override void SpecialDraw(int i,int j,SpriteBatch batch)
        {
            var first=Main.tile[i,j];if(!first.HasTile||first.TileType!=Type||SwayAmplitude<=0)return;
            var data=TileObjectData.GetTileData(first);
            var renderer=Main.instance.TilesRenderer;
            float drive=0;
            if(Main.SettingsEnabled_TilesSwayInWind&&renderer.InAPlaceWithWind(i,j,data.Width,data.Height))
            {
                double time=global::IslandAdventure.Systems.GroveWindSystem.Time;
                float wind=renderer.GetWindCycle(i,j,time*0.1);
                drive=MathHelper.Clamp(wind+(float)System.Math.Sin(time*1.5+i*.37+j*.13)*.2f,-1,1);
            }
            Vector2 screen=Main.Camera.UnscaledPosition;
            for(int row=0;row<data.Height;row++)for(int col=0;col<data.Width;col++)
            {
                var tile=Main.tile[i+col,j+row];
                if(!tile.HasTile||tile.TileType!=Type||!TileDrawing.IsVisible(tile))continue;
                var texture=renderer.GetTileDrawTexture(tile,i+col,j+row);
                Color color=tile.IsTileFullbright?Color.White:Lighting.GetColor(i+col,j+row);
                for(int py=0;py<16;py+=2)
                {
                    int y=row*16+py;
                    int shift=global::IslandAdventure.Systems.GroveWindSystem.Offset(y,data.Height*16,SwayAmplitude,drive);
                    batch.Draw(texture,new Vector2((i+col)*16+shift,(j+row)*16+data.DrawYOffset+py)-screen,
                        new Rectangle(tile.TileFrameX,tile.TileFrameY+py,16,2),color);
                }
            }
        }
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = Main.tileNoFail[Type] = Main.tileLavaDeath[Type] = true;
            Main.tileCut[Type] = Cuttable;
            Main.tileAxe[Type] = MaterialDrop == ItemID.Wood;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = PlantWidth;
            TileObjectData.newTile.Height = PlantHeight;
            TileObjectData.newTile.DrawYOffset = SurfaceOffset;
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
        public override float SwayAmplitude => 4f;
        public override string Texture => "IslandAdventure/Assets/Grove/bush";
        protected override int PlantWidth => 7;
        protected override int PlantHeight => 5;
    }
    public class GroveFern : GrovePlant
    {
        public override float SwayAmplitude => 8f;
        public override string Texture => "IslandAdventure/Assets/Grove/fern";
        protected override int PlantWidth => 8;
        protected override int PlantHeight => 6;
    }
    public class GroveTuft : GrovePlant
    {
        public override float SwayAmplitude => 2f;
        public override string Texture => "IslandAdventure/Assets/Grove/Tuft";
        protected override int PlantWidth => 2;
        protected override int PlantHeight => 1;
        protected override bool Variants => true;
    }
    public class GroveSmallBush : GrovePlant
    {
        public override float SwayAmplitude => 4f;
        public override string Texture => "IslandAdventure/Assets/Grove/SmallBush";
        protected override int PlantWidth => 4;
        protected override int PlantHeight => 3;
    }
    public class GroveSmallFern : GrovePlant
    {
        public override float SwayAmplitude => 6f;
        public override string Texture => "IslandAdventure/Assets/Grove/SmallFern";
        protected override int PlantWidth => 5;
        protected override int PlantHeight => 4;
    }
    public class GroveMoss : GrovePlant
    {
        public override float SwayAmplitude => 2f;
        public override string Texture => "IslandAdventure/Assets/Grove/Moss";
        protected override int PlantWidth => 4;
        protected override int PlantHeight => 1;
    }
    public abstract class GroveWoodDecoration : GrovePlant
    {
        protected override bool Cuttable => false;
        protected override int MaterialDrop => ItemID.Wood;
        protected override int SurfaceOffset => 6;
        protected override Color MapColor => new Color(87,61,42);
    }
    public class GroveRoots : GroveWoodDecoration
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Roots";
        protected override int SurfaceOffset => 12;
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
        protected override int SurfaceOffset => 6;
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
