using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.Tiles
{
    // Appearance is regional; native tile IDs keep every vanilla gameplay path.
    // The gallery remains the art testing area until grove worldgen is introduced.
    public class GroveAppearance : GlobalTile
    {
        public static bool InGrove(int x,int y)
            => ModContent.GetInstance<GrovePreviewSystem>().Bounds.Contains(x,y);

        public static bool GroveFoliage(int i,int j,int type)
        {
            if(!WorldGen.InWorld(i,j,1))return false;
            if(type==TileID.Plants||type==TileID.Plants2)
                return InGrove(i,j+1)&&Main.tile[i,j+1].HasTile&&Main.tile[i,j+1].TileType==TileID.Grass;
            if(type!=TileID.Vines)return false;
            // Follow the actual root, including segments below the gallery boundary.
            int y=j;
            while(y>1&&Main.tile[i,y].HasTile&&Main.tile[i,y].TileType==TileID.Vines)y--;
            return InGrove(i,y)&&Main.tile[i,y].HasTile&&Main.tile[i,y].TileType==TileID.Grass;
        }

        public static bool UsesGrassAtlas(int i,int j,int type)
            =>type==TileID.Grass&&InGrove(i,j)&&Main.tile[i,j].TileColor==PaintID.None;

        public override void DrawEffects(int i,int j,int type,SpriteBatch batch,ref TileDrawInfo drawData)
        {
            if(UsesGrassAtlas(i,j,type))
                drawData.drawTexture=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/GrassAtlas",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        }

        private static uint PlantHash(int i,int j)
        {
            uint value=unchecked((uint)i*73856093u^(uint)j*19349663u);
            value^=value>>16;value*=0x7feb352du;value^=value>>15;
            return value;
        }
        public static int PlantVariant(int i,int j)
        {
            uint hash=PlantHash(i,j);
            return (int)((hash>>8)%8)+(hash%4==0?8:0);
        }
        public static int VineVariant(int i,int rootY)=>(int)(PlantHash(i,rootY)%4);
        public static Rectangle VineSource(int variant,int segment,bool tip)
            =>new Rectangle(variant*16,(tip?7:segment%7)*16,16,16);

        public override void PostDraw(int i,int j,int type,SpriteBatch batch)
        {
            if(type==TileID.Grass&&InGrove(i,j))GroveGrass.DrawRoots(i,j,batch);
        }

        public override bool PreDraw(int i,int j,int type,SpriteBatch batch)
        {
            var surface=Main.tile[i,j];
            if(surface.TileColor==PaintID.None&&GroveFoliage(i,j,type))
            {
                Main.instance.TilesRenderer.AddSpecialPoint(i,j,TileDrawing.TileCounterType.CustomNonSolid);
                return false; // Do not enqueue vanilla windy grass/vines as well.
            }
            if(type!=TileID.Trees||!InGrove(i,j))return true;
            WorldGen.GetTreeBottom(i,j,out int rootX,out int rootY);
            if(!InGrove(rootX,rootY)||Main.tile[rootX,rootY].TileType!=TileID.Grass)return true;
            var tile=Main.tile[i,j];
            // Let vanilla painting handle painted trees. Coatings still work below.
            if(tile.TileColor!=PaintID.None)return true;
            if(!TileDrawing.IsVisible(tile))return false;
            Vector2 point=new Vector2(i*16,j*16)-Main.screenPosition;
            if(!Main.drawToScreen)point+=new Vector2(Main.offScreenRange);
            Color light=tile.IsTileFullbright?Color.White:Lighting.GetColor(i,j);
            var bark=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree",ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            int fx=tile.TileFrameX,fy=tile.TileFrameY;
            if(fx+20<=bark.Width&&fy+20<=bark.Height)
                batch.Draw(bark,point+new Vector2(-2,0),new Rectangle(fx,fy,20,20),light);
            if(fy>=198)
            {
                float sway=0;
                var renderer=Main.instance.TilesRenderer;
                if(Main.SettingsEnabled_TilesSwayInWind&&renderer.InAPlaceWithWind(i,j,1,1))
                    sway=renderer.GetWindCycle(i,j,GroveWindSystem.Time*.1)*.025f;
                if(fx==22)
                {
                    var top=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree_Tops",ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    batch.Draw(top,point+new Vector2(8,16),new Rectangle(0,0,256,192),light,
                        sway,new Vector2(128,192),1,SpriteEffects.None,0);
                }
                else if(fx==44||fx==66)
                {
                    var branch=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree_Branches",ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    int style=System.Math.Clamp((fy-198)/22,0,2);
                    Vector2 origin=new Vector2(fx==44?40:0,24);
                    batch.Draw(branch,point+new Vector2(fx==44?16:0,12),
                        new Rectangle(fx==44?0:42,style*42,40,40),light,sway,origin,1,SpriteEffects.None,0);
                }
            }
            return false;
        }

        public override void SpecialDraw(int i,int j,int type,SpriteBatch batch)
        {
            var tile=Main.tile[i,j];
            if(!tile.HasTile||tile.TileType!=type||!GroveFoliage(i,j,type)||!TileDrawing.IsVisible(tile))return;
            Vector2 point=new Vector2(i*16,j*16)-Main.Camera.UnscaledPosition;
            Color light=tile.IsTileFullbright?Color.White:Lighting.GetColor(i,j);
            float drive=0;
            var renderer=Main.instance.TilesRenderer;
            if(Main.SettingsEnabled_TilesSwayInWind&&renderer.InAPlaceWithWind(i,j,1,1))
                drive=renderer.GetWindCycle(i,j,GroveWindSystem.Time*.1);
            if(type==TileID.Vines)
            {
                var vine=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Vine",ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                int root=j;
                while(root>1&&Main.tile[i,root-1].HasTile&&Main.tile[i,root-1].TileType==TileID.Vines)root--;
                bool tip=!Main.tile[i,j+1].HasTile||Main.tile[i,j+1].TileType!=TileID.Vines;
                Rectangle source=VineSource(VineVariant(i,root),j-root,tip);
                for(int py=0;py<16;py+=2)
                {
                    float depth=(j-root)*16+py;
                    int shift=(int)System.Math.Round(drive*System.Math.Min(4,depth/16));
                    batch.Draw(vine,point+new Vector2(shift,py),new Rectangle(source.X,source.Y+py,16,2),light);
                }
            }
            else
            {
                var plants=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/GroundPlants",ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                int style=PlantVariant(i,j);
                for(int py=0;py<32;py+=2)
                {
                    int shift=GroveWindSystem.Offset(py,32,2,drive);
                    batch.Draw(plants,point+new Vector2(-8+shift,py-16),new Rectangle(style*32,py,32,2),light);
                }
            }
        }
    }
}
