using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.Trees
{
    // One original whole-tree image. Each native stem cell owns its horizontal
    // strip, so forks are never joined from unrelated branch/crown textures.
    public static class BunaTree
    {
        public const int Width=288, Height=448, AnchorX=136, TileHeight=28;
        public static bool IsSampleRoot(int x,int floor)
        {
            var bounds=ModContent.GetInstance<GrovePreviewSystem>().Bounds;
            int dx=x-bounds.Left;
            return bounds.Width==GrovePreviewSystem.GalleryWidth&&floor==bounds.Bottom-4
                &&(dx==17||dx==43||dx==164);
        }
        public static bool Mirrored(int x)
            =>x-ModContent.GetInstance<GrovePreviewSystem>().Bounds.Left==43;
        public static int StripY(int floor,int y)=>(TileHeight-(floor-y))*16;
        public static bool Intact(int x,int floor)
        {
            if(!WorldGen.InWorld(x,floor-TileHeight,1))return false;
            var top=Main.tile[x,floor-TileHeight];
            return top.HasTile&&top.TileType==TileID.Trees&&top.TileFrameX==22&&top.TileFrameY==198;
        }
        public static bool TryGrow(int x,int floor)
        {
            if(!WorldGen.InWorld(x-9,floor-TileHeight,2)||!WorldGen.InWorld(x+10,floor,2))return false;
            for(int dx=-2;dx<=2;dx++)
            {
                var soil=Main.tile[x+dx,floor];
                if(!soil.HasTile||soil.TileType!=TileID.Grass||soil.Slope!=SlopeType.Solid||soil.IsHalfBlock)return false;
            }
            // Do not place the artwork over a player's existing build/liquid.
            int anchor=Mirrored(x)?Width-AnchorX:AnchorX;
            int left=(x*16+8-anchor-4)/16,right=(x*16+8-anchor+Width+3)/16;
            for(int tx=left;tx<=right;tx++)for(int y=floor-TileHeight;y<floor;y++)
                if(Main.tile[tx,y].HasTile||Main.tile[tx,y].LiquidAmount!=0)return false;
            for(int y=floor-TileHeight;y<floor;y++)
            {
                var tile=Main.tile[x,y];tile.ClearTile();tile.HasTile=true;tile.TileType=TileID.Trees;
                tile.TileFrameX=(short)(y==floor-TileHeight?22:0);
                tile.TileFrameY=(short)(y==floor-TileHeight?198:((floor-y)%3)*22);
                tile.TileColor=PaintID.None;tile.IsTileInvisible=tile.IsTileFullbright=false;
            }
            return true;
        }
    }
}
