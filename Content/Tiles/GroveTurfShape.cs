using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace IslandAdventure.Content.Tiles
{
    public static class GroveTurfShape
    {
        public const int Top=1,Right=2,Bottom=4,Left=8;
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
            var tile=Main.tile[i,j];
            return tile.HasTile&&!tile.IsActuated&&Main.tileSolid[tile.TileType]
                &&!Main.tileSolidTop[tile.TileType]&&Contains(tile.Slope,tile.IsHalfBlock,x&15,y&15);
        }
        public static int ShapeFaces(SlopeType slope,bool half)
        {
            if(half)return Bottom;
            return slope switch {
                SlopeType.SlopeDownRight=>Bottom|Right,
                SlopeType.SlopeDownLeft=>Bottom|Left,
                SlopeType.SlopeUpRight=>Top|Right,
                SlopeType.SlopeUpLeft=>Top|Left,
                _=>Top|Right|Bottom|Left
            };
        }
        private static bool EdgeOpen(int i,int j,int face,int along)
            =>face switch {
                Top=>!Solid(i*16+along,j*16-1),
                Right=>!Solid(i*16+16,j*16+along),
                Bottom=>!Solid(i*16+along,j*16+16),
                _=>!Solid(i*16-1,j*16+along)
            };
        public static Rectangle FaceFrame(int i,int face)
        {
            int row=face==Top?0:face==Right?1:face==Bottom?2:3;
            return new Rectangle((i%6)*20,row*20,20,20);
        }
        public static bool FaceSample(int i,int j,int face,int x,int y,out Rectangle source)
        {
            source=Rectangle.Empty;
            var tile=Main.tile[i,j];
            if(!tile.HasTile||(ShapeFaces(tile.Slope,tile.IsHalfBlock)&face)==0)return false;
            int along=face==Top||face==Bottom?x:y;
            if(along<0||along>=16)return false;
            bool inside=x>=0&&x<16&&y>=0&&y<16;
            if(inside)
            {
                if(!Contains(tile.Slope,tile.IsHalfBlock,x,y))return false;
            }
            else
            {
                bool tip=face switch {
                    Top=>y==-2&&Contains(tile.Slope,tile.IsHalfBlock,x,0),
                    Right=>x==16&&Contains(tile.Slope,tile.IsHalfBlock,15,y),
                    Bottom=>y==16&&Contains(tile.Slope,tile.IsHalfBlock,x,15),
                    _=>x==-2&&Contains(tile.Slope,tile.IsHalfBlock,0,y)
                };
                if(!tip)return false;
            }
            int depth=face switch {Top=>y,Right=>14-x,Bottom=>14-y,_=>x};
            if(depth < -2||depth>6)return false;
            if(!EdgeOpen(i,j,face,along))
            {
                // A concave corner exists only when both adjacent faces are
                // connected but the diagonal is empty. Flat strips keep no seams.
                if(!inside||depth>2||(along>2&&along<12))return false;
                bool nearStart=along<=2;
                bool vertical=face==Top||face==Bottom;
                int dx=vertical?(nearStart?-1:16):(face==Left?-1:16);
                int dy=vertical?(face==Top?-1:16):(nearStart?-1:16);
                int adjacent=vertical?(nearStart?Left:Right):(nearStart?Top:Bottom);
                int adjacentAlong=face==Bottom||face==Right?14:0;
                if(EdgeOpen(i,j,adjacent,adjacentAlong)||Solid(i*16+dx,j*16+dy))return false;
            }
            var frame=FaceFrame(i,face);
            source=new Rectangle(frame.X+x+2,frame.Y+y+2,2,2);
            return true;
        }
        public static void Draw(int i,int j,SpriteBatch batch,Texture2D texture,Vector2 point,Color light)
        {
            var tile=Main.tile[i,j];
            int faces=ShapeFaces(tile.Slope,tile.IsHalfBlock);
            for(int face=Top;face<=Left;face<<=1)
            {
                if((faces&face)==0)continue;
                bool whole=tile.Slope==SlopeType.Solid&&!tile.IsHalfBlock;
                if(whole)for(int along=0;along<16;along+=2)
                    if(!EdgeOpen(i,j,face,along)){whole=false;break;}
                if(whole)
                    batch.Draw(texture,point-new Vector2(2),FaceFrame(i,face),light);
                else
                    for(int y=-2;y<=16;y+=2)for(int x=-2;x<=16;x+=2)
                        if(FaceSample(i,j,face,x,y,out var source))
                            batch.Draw(texture,point+new Vector2(x,y),source,light);
            }
        }
    }
}
