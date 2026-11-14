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
    public class BunaAppearance : GlobalTile
    {
        public static bool TryRow(int i,int j,int type,out int floor,out int sourceY)
        {
            floor=sourceY=0;
            if(type!=TileID.Trees||!GroveAppearance.InGrove(i,j))return false;
            WorldGen.GetTreeBottom(i,j,out int rootX,out floor);
            sourceY=BunaTree.StripY(floor,j);
            return rootX==i&&BunaTree.IsSampleRoot(rootX,floor)&&sourceY>=0&&sourceY<BunaTree.Height;
        }
        public override bool PreDraw(int i,int j,int type,SpriteBatch batch)
        {
            if(!TryRow(i,j,type,out int floor,out int sourceY))return true;
            var tile=Main.tile[i,j];if(!TileDrawing.IsVisible(tile))return false;
            bool whole=BunaTree.Intact(i,floor);
            bool mirrored=BunaTree.Mirrored(i);
            Vector2 point=new Vector2(i*16+8,j*16)-Main.screenPosition;
            if(!Main.drawToScreen)point+=new Vector2(Main.offScreenRange);
            Color light=tile.IsTileFullbright?Color.White:Lighting.GetColor(i,j);
            if(tile.TileColor!=PaintID.None)
            {
                Color paint=WorldGen.paintColor(tile.TileColor);
                light=new Color(light.R*paint.R/255,light.G*paint.G/255,light.B*paint.B/255,light.A);
            }
            if(whole||j==floor-1)
            {
                var tree=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Buna",AssetRequestMode.ImmediateLoad).Value;
                float drive=0;
                if(whole&&Main.SettingsEnabled_TilesSwayInWind&&Main.instance.TilesRenderer.InAPlaceWithWind(i,floor-8,1,1))
                    drive=Main.instance.TilesRenderer.GetWindCycle(i,floor-8,GroveWindSystem.Time*.1);
                int sway=GroveWindSystem.Offset(sourceY,BunaTree.Height,2,drive)*2;
                int anchor=mirrored?BunaTree.Width-BunaTree.AnchorX:BunaTree.AnchorX;
                batch.Draw(tree,point+new Vector2(-anchor+sway,0),new Rectangle(0,sourceY,BunaTree.Width,16),light,
                    0,Vector2.Zero,1,mirrored?SpriteEffects.FlipHorizontally:SpriteEffects.None,0);
            }
            else
            {
                // Chopping removes the upper native cells. Remaining cells show
                // only original bark, never a floating sliced canopy.
                var bark=ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/BunaStump",AssetRequestMode.ImmediateLoad).Value;
                batch.Draw(bark,point+new Vector2(-8,0),new Rectangle(0,(j%4)*16,16,16),light);
            }
            return false;
        }
    }
}
