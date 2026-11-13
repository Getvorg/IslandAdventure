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
            // Sample every exposed face against actual neighboring collision geometry.
            // The same textured rim is oriented onto sides, undersides and hammered edges.
            for(int y=0;y<16;y++)for(int x=0;x<16;x++)
            {
                if(!Contains(tile.Slope,tile.IsHalfBlock,x,y))continue;
                int depth=9,axis=0;
                for(int d=1;d<=8;d++)
                {
                    int wx=i*16+x,wy=j*16+y;
                    if(!Solid(wx,wy-d)){depth=d;axis=0;break;}
                    if(!Solid(wx-d,wy)){depth=d;axis=1;break;}
                    if(!Solid(wx+d,wy)){depth=d;axis=2;break;}
                    if(!Solid(wx,wy+d)){depth=d;axis=3;break;}
                    if(d<=4&&(!Solid(wx-d,wy-d)||!Solid(wx+d,wy-d)||!Solid(wx-d,wy+d)||!Solid(wx+d,wy+d)))
                    {depth=d+2;axis=0;break;}
                }
                if(depth>8)continue;
                int along=axis==1||axis==2?y:x;
                batch.Draw(texture,point+new Vector2(x,y),new Rectangle(frame+along,4+depth-1,1,1),light);
            }
            // Grass tips remain above an exposed flat top; soil/body never spills outside slopes.
            if(tile.Slope==SlopeType.Solid)
            {
                int top=tile.IsHalfBlock?8:0;
                for(int x=0;x<16;x++)if(!Solid(i*16+x,j*16+top-1))
                    for(int y=0;y<4;y++)if(!Solid(i*16+x,j*16+top-4+y))
                        batch.Draw(texture,point+new Vector2(x,top-4+y),new Rectangle(frame+x,y,1,1),light);
            }
        }
    }
}
