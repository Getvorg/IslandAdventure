using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Trees;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.Tiles
{
    public class CamphorAppearance : GlobalTile
    {
        public static bool Applies(int i,int j,int type)
        {
            if(type!=TileID.Trees||!GroveAppearance.InGrove(i,j))return false;
            WorldGen.GetTreeBottom(i,j,out int x,out int y);
            return GroveAppearance.InGrove(x,y)&&Main.tile[x,y].HasTile
                &&Main.tile[x,y].TileType==TileID.Grass&&Main.tile[i,j].TileColor==PaintID.None;
        }
        public override bool PreDraw(int i,int j,int type,SpriteBatch batch)
        {
            if(!Applies(i,j,type))return true;
            var tile=Main.tile[i,j];
            if(!TileDrawing.IsVisible(tile))return false;
            Vector2 point=new Vector2(i*16,j*16)-Main.screenPosition;
            if(!Main.drawToScreen)point+=new Vector2(Main.offScreenRange);
            Color light=tile.IsTileFullbright?Color.White:Lighting.GetColor(i,j);
            var bark=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Camphor",AssetRequestMode.ImmediateLoad).Value;
            int fx=tile.TileFrameX,fy=tile.TileFrameY;
            if(fx>=0&&fy>=0&&fx+20<=bark.Width&&fy+20<=bark.Height)
                batch.Draw(bark,point+new Vector2(-2,0),new Rectangle(fx,fy,20,20),light);
            if(fy<198)return false;
            float sway=0;
            var renderer=Main.instance.TilesRenderer;
            if(Main.SettingsEnabled_TilesSwayInWind&&renderer.InAPlaceWithWind(i,j,1,1))
                sway=renderer.GetWindCycle(i,j,GroveWindSystem.Time*.1)*.018f;
            if(fx==22)
            {
                var crown=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Camphor_Tops",AssetRequestMode.ImmediateLoad).Value;
                batch.Draw(crown,point+new Vector2(8,16),new Rectangle(0,0,AncientCamphor.CrownWidth,AncientCamphor.CrownHeight),
                    light,sway,new Vector2(AncientCamphor.CrownWidth/2,AncientCamphor.CrownHeight),1,SpriteEffects.None,0);
            }
            else if(fx==44||fx==66)
            {
                var branch=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Camphor_Boughs",AssetRequestMode.ImmediateLoad).Value;
                int style=System.Math.Clamp((fy-198)/22,0,2);
                bool left=fx==44;
                batch.Draw(branch,point+new Vector2(left?16:0,left?12:18),
                    new Rectangle(left?0:66,style*50,AncientCamphor.BoughWidth,AncientCamphor.BoughHeight),light,sway,new Vector2(left?AncientCamphor.BoughWidth:0,left?36:42),1,SpriteEffects.None,0);
            }
            return false;
        }
    }
}
