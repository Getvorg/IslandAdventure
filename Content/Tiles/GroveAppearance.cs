using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
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

        public override void PostDraw(int i,int j,int type,SpriteBatch batch)
        {
            if(type==TileID.Grass&&InGrove(i,j))GroveGrass.DrawTurf(i,j,batch);
        }

        public override bool PreDraw(int i,int j,int type,SpriteBatch batch)
        {
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
    }
}
