using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.Trees
{
    public class BunaClassicTree : ModTree
    {
        public const int CrownWidth=136,CrownHeight=112;
        public override TreePaintingSettings TreeShaderSettings=>new TreePaintingSettings();
        public override void SetStaticDefaults()=>GrowsOnTileId=new[]{ModContent.TileType<GroveGrass>()};
        public override Asset<Texture2D> GetTexture()=>ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/BunaClassic");
        public override Asset<Texture2D> GetTopTextures()=>ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/BunaClassic_Tops");
        public override Asset<Texture2D> GetBranchTextures()=>ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/BunaClassic_Branches");
        public override void SetTreeFoliageSettings(int i,int j,Tile tile,int xoffset,ref int treeFrame,int floorY,ref int width,ref int height)
        {treeFrame=xoffset==0?0:System.Math.Clamp(treeFrame,0,2);width=CrownWidth;height=CrownHeight;}
        public override int DropWood()=>ModContent.ItemType<Items.BunaWood>();
        public static bool IsBunaRoot(int x,int floor)=>IsPreviewRoot(x,floor)||ModContent.GetInstance<BunaTreeRegistry>().Owns(x,floor);
        public static bool IsPreviewRoot(int x,int floor)
        {
            var b=ModContent.GetInstance<GrovePreviewSystem>().Bounds;
            return b.Width==GrovePreviewSystem.GalleryWidth&&floor==b.Bottom-4
                &&(x==b.Left+43||x==b.Left+164);
        }
        public static bool TryGrow(int x,int floor,bool tall)
        {
            if(!WorldGen.InWorld(x-5,floor-40,2)||!WorldGen.InWorld(x+5,floor,2))return false;
            for(int dx=-2;dx<=2;dx++)
            {
                var t=Main.tile[x+dx,floor];
                if(!t.HasTile||t.TileType!=TileID.Grass||t.IsHalfBlock||t.Slope!=SlopeType.Solid)return false;
            }
            for(int tx=x-5;tx<=x+5;tx++)for(int y=floor-40;y<floor;y++)
                if(Main.tile[tx,y].HasTile||Main.tile[tx,y].LiquidAmount!=0)return false;
            bool grown=tall?WorldGen.GrowEpicTree(x,floor):WorldGen.GrowTree(x,floor);
            if(!grown)return false;
            // Vanilla may deliberately choose a broken/bare top. This beech
            // sample always starts with its separate living crown.
            int top=floor-1;
            while(Main.tile[x,top-1].HasTile&&Main.tile[x,top-1].TileType==TileID.Trees)top--;
            var cap=Main.tile[x,top];cap.TileFrameX=22;
            cap.TileFrameY=(short)(198+System.Math.Clamp((cap.TileFrameY-198)/22,0,2)*22);
            ModContent.GetInstance<BunaTreeRegistry>().Register(x,floor);
            return true;
        }
    }
}
