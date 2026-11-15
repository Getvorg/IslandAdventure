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
    public class BunaClassicAppearance : GlobalTile
    {
        public static bool Applies(int i,int j,int type)
        {
            if(type!=TileID.Trees||!GroveAppearance.InGrove(i,j))return false;
            WorldGen.GetTreeBottom(i,j,out int rootX,out int floor);
            return BunaClassicTree.IsSampleRoot(rootX,floor);
        }
        public override bool PreDraw(int i,int j,int type,SpriteBatch batch)
        {
            if(!Applies(i,j,type))return true;
            var tile=Main.tile[i,j];if(!TileDrawing.IsVisible(tile))return false;
            var point=new Vector2(i*16,j*16)-Main.screenPosition;
            if(!Main.drawToScreen)point+=new Vector2(Main.offScreenRange);
            Color light=tile.IsTileFullbright?Color.White:Lighting.GetColor(i,j);
            if(tile.TileColor!=PaintID.None)
            {
                Color paint=WorldGen.paintColor(tile.TileColor);
                light=new Color(light.R*paint.R/255,light.G*paint.G/255,light.B*paint.B/255,light.A);
            }
            int fx=tile.TileFrameX,fy=tile.TileFrameY;
            var stem=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/BunaClassic",AssetRequestMode.ImmediateLoad).Value;
            if(fx>=0&&fy>=0&&fx+20<=stem.Width&&fy+20<=stem.Height)
                batch.Draw(stem,point+new Vector2(-2,0),new Rectangle(fx,fy,20,20),light);
            if(fy<198)return false;
            float sway=0;
            if(Main.SettingsEnabled_TilesSwayInWind&&Main.instance.TilesRenderer.InAPlaceWithWind(i,j,1,1))
                sway=Main.instance.TilesRenderer.GetWindCycle(i,j,GroveWindSystem.Time*.1)*.018f;
            if(fx==22)
            {
                var crown=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/BunaClassic_Tops",AssetRequestMode.ImmediateLoad).Value;
                batch.Draw(crown,point+new Vector2(8,16),new Rectangle(0,0,136,112),light,
                    sway,new Vector2(68,112),1,SpriteEffects.None,0);
                // Keep the collar locked to the stem while the upper crown sways.
                batch.Draw(crown,point+new Vector2(0,8),new Rectangle(60,104,16,8),light);
            }
            else if(fx==44||fx==66)
            {
                bool left=fx==44;int style=System.Math.Clamp((fy-198)/22,0,2);
                var branch=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/BunaClassic_Branches",AssetRequestMode.ImmediateLoad).Value;
                batch.Draw(branch,point+new Vector2(left?16:0,left?12:18),new Rectangle(left?0:42,style*42,40,40),light,
                    sway,new Vector2(left?40:0,left?24:30),1,SpriteEffects.None,0);
                // The shared native joint stays fixed while its leafy tip sways.
                batch.Draw(stem,point+new Vector2(-2,0),new Rectangle(fx,fy,20,20),light);
            }
            return false;
        }
    }
}
