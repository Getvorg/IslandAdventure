using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
namespace IslandAdventure.Content.Tiles
{
    public static class GroveTurfShape
    {
        public static bool Contains(SlopeType slope,bool half,int x,int y)
        {
            if(x<0||x>=16||y<0||y>=16)return false;
            if(half)return y>=8;
            return slope switch {
                SlopeType.SlopeDownRight=>y>=14-(x/2)*2,
                SlopeType.SlopeDownLeft=>y>=(x/2)*2,
                SlopeType.SlopeUpRight=>y<=1+(x/2)*2,
                SlopeType.SlopeUpLeft=>y<=15-(x/2)*2,
                _=>true
            };
        }
        private static bool Solid(int x,int y)
        {
            int i=x>>4,j=y>>4;
            if(i<0||j<0||i>=Main.maxTilesX||j>=Main.maxTilesY)return false;
            var t=Main.tile[i,j];
            return t.HasTile&&!t.IsActuated&&Main.tileSolid[t.TileType]&&!Main.tileSolidTop[t.TileType]
                &&Contains(t.Slope,t.IsHalfBlock,x&15,y&15);
        }
        public static void Draw(int i,int j,SpriteBatch batch,Texture2D texture,Vector2 point,Color light)
        {
            var tile=Main.tile[i,j];
            int frame=(i%(texture.Width/18))*18;
            for(int y=0;y<16;y+=2)for(int x=0;x<16;x+=2)
            {
                if(TrySample(i,j,x,y,out int along,out int sourceY))
                    batch.Draw(texture,point+new Vector2(x,y),new Rectangle(frame+along,sourceY,2,2),light);
            }
            // Grass tips remain above an exposed flat top; soil/body never spills outside slopes.
            if(tile.Slope==SlopeType.Solid)
            {
                int top=tile.IsHalfBlock?8:0;
                for(int x=0;x<16;x+=2)if(!Solid(i*16+x,j*16+top-2))
                    batch.Draw(texture,point+new Vector2(x,top-2),new Rectangle(frame+x,2,2,2),light);
            }
        }
        // Exposed geometry, not tile-frame indices, controls all connections.
        // The top is 4px of sod + 2px transition, sides at most 4px;
        // undersides sample only sparse root tips, never an opaque green band.
        public static bool TrySample(int i,int j,int x,int y,out int along,out int sourceY)
        {
            along=x;sourceY=0;
            var tile=Main.tile[i,j];
            if(!tile.HasTile||!Contains(tile.Slope,tile.IsHalfBlock,x,y))return false;
            int wx=i*16+x,wy=j*16+y;
            for(int step=1;step<=3;step++)
            {
                int d=step*2;
                if(!Solid(wx,wy-d)){sourceY=4+(step-1)*2;return true;}
                if(step<=2&&(!Solid(wx-d,wy)||!Solid(wx+d,wy)))
                {along=y;sourceY=4+(step-1)*2;return true;}
                if(step==1&&!Solid(wx,wy+d)){sourceY=10;return true;}
                if(step<=2&&(!Solid(wx-d,wy-d)||!Solid(wx+d,wy-d)))
                {sourceY=6+(step-1)*2;return true;}
                if(step==1&&(!Solid(wx-d,wy+d)||!Solid(wx+d,wy+d)))
                {sourceY=10;return true;}
            }
            return false;
        }
    }
}
