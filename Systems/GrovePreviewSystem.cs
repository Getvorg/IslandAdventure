using System;
using Microsoft.Xna.Framework;
using IslandAdventure.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace IslandAdventure.Systems
{
    public class GrovePreviewSystem : ModSystem
    {
        public const int GalleryWidth = 176;
        public const int GalleryHeight = 72;
        private const int CurrentPlantLayout = 5;
        public Rectangle Bounds { get; private set; }
        private Vector2 returnPosition;
        private bool canReturn, triedUpgrade;
        private string returnOwner;
        private int plantLayout;
        public override void OnWorldLoad()
        {
            Bounds=Rectangle.Empty; canReturn=triedUpgrade=false; returnOwner=null; plantLayout=0;
        }
        public override void PreWorldGen() => OnWorldLoad();
        public override void SaveWorldData(TagCompound tag)
        {
            if(Bounds.Width>0)
            {
                tag["x"]=Bounds.X;tag["y"]=Bounds.Y;tag["w"]=Bounds.Width;tag["h"]=Bounds.Height;
                tag["plantLayout"]=plantLayout;
            }
            if(canReturn){tag["returnX"]=returnPosition.X;tag["returnY"]=returnPosition.Y;tag["owner"]=returnOwner;}
        }
        public override void LoadWorldData(TagCompound tag)
        {
            int w=tag.GetInt("w"),h=tag.GetInt("h");
            if((w==56&&h==46)||(w==GalleryWidth&&h==GalleryHeight))
                Bounds=new Rectangle(tag.GetInt("x"),tag.GetInt("y"),w,h);
            plantLayout=tag.GetInt("plantLayout");
            if(tag.ContainsKey("owner")&&float.IsFinite(tag.GetFloat("returnX"))&&float.IsFinite(tag.GetFloat("returnY")))
            {returnPosition=new Vector2(tag.GetFloat("returnX"),tag.GetFloat("returnY"));returnOwner=tag.GetString("owner");canReturn=true;}
        }
        public override void PreUpdateWorld()
        {
            if(Main.gameMenu||WorldGen.generatingWorld||Main.netMode!=NetmodeID.SinglePlayer)return;
            if(Bounds.Width==0||plantLayout>=CurrentPlantLayout||triedUpgrade)return;
            triedUpgrade=true;
            if(Bounds.Width==GalleryWidth||TryExpand())RefreshPlants();
        }
        public bool Visit(Player player, out string message)
        {
            if(Main.netMode!=NetmodeID.SinglePlayer||BoatTravelSystem.IsCinematic||BoatTravelSystem.IsLoading)
            {message="Образец доступен в одиночной игре вне переправы.";return false;}
            foreach(var npc in Main.npc)if(npc.active&&npc.boss){message="Сначала заверши бой с боссом.";return false;}
            bool wasInPlot=Bounds.Width>0&&Bounds.Contains(player.Center.ToTileCoordinates());
            if(Bounds.Width==56&&!TryExpand())
            {
                // Existing builds block expansion: find fresh sky, never erase the old plot.
                if(!CreateNear(player)){message="Для расширения не хватает свободного неба. Поднимись выше и попробуй снова.";return false;}
            }
            else if(Bounds.Width==0&&!CreateNear(player))
            {message="Не найден свободный участок неба. Поднимись на поверхность и попробуй снова.";return false;}
            if(!Valid(Bounds)){message="Запись тестового участка повреждена.";return false;}
            if(plantLayout<CurrentPlantLayout)RefreshPlants();
            Vector2 landing=new Vector2((Bounds.Left+5)*16,(Bounds.Bottom-4)*16-player.height);
            if(!ResumeSystem.TrySafePosition(player,landing,out var safe)){message="Место входа занято блоками.";return false;}
            if(!wasInPlot){returnPosition=player.position;returnOwner=player.name;canReturn=true;}
            Move(player,safe);
            message="Образец рощи расширен: пройди вправо до большого валуна и второго дерева. /grovepreview back — вернуться.";
            return true;
        }
        public bool Back(Player player)
        {
            if(BoatTravelSystem.IsCinematic||BoatTravelSystem.IsLoading||!canReturn||returnOwner!=player.name||!ResumeSystem.TrySafePosition(player,returnPosition,out var safe))return false;
            Move(player,safe);canReturn=false;return true;
        }
        private static void Move(Player p,Vector2 point)
        {
            p.position=p.oldPosition=point;p.velocity=Vector2.Zero;p.fallStart=(int)(point.Y/16);
            p.immune=true;p.immuneTime=Math.Max(p.immuneTime,120);
        }
        private static bool Valid(Rectangle area) => area.Left>=10&&area.Top>=10&&area.Width>0&&area.Right<Main.maxTilesX-10&&area.Bottom<Main.maxTilesY-10;
        private static bool Empty(Rectangle area,Rectangle allowed=default)
        {
            if(!Valid(area))return false;
            for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++)
            {
                if(allowed.Contains(x,y))continue;
                var t=Main.tile[x,y];if(t.HasTile||t.WallType!=WallID.None||t.LiquidAmount!=0)return false;
            }
            return true;
        }
        private bool CreateNear(Player p)
        {
            int center=(int)p.Center.X/16;
            foreach(int dx in new[]{0,190,-190,380,-380})
            {
                int left=Math.Clamp(center+dx-28,100,Main.maxTilesX-GalleryWidth-20);
                for(int floor=Math.Clamp((int)(p.position.Y/16)-12,110,Math.Max(110,(int)Main.worldSurface-20));floor>=90;floor-=12)
                {
                    var area=new Rectangle(left,floor-GalleryHeight+4,GalleryWidth,GalleryHeight);
                    if(!Empty(area))continue;
                    Bounds=area;BuildBase();RefreshPlants();return true;
                }
            }
            return false;
        }
        private bool TryExpand()
        {
            if(Bounds.Width!=56||!Valid(Bounds))return false;
            var enlarged=new Rectangle(Bounds.Left,Bounds.Bottom-GalleryHeight,GalleryWidth,GalleryHeight);
            if(!Empty(enlarged,Bounds))return false;
            Bounds=enlarged;BuildBase();return true;
        }
        private void BuildBase()
        {
            int floor=Bounds.Bottom-4;
            for(int x=Bounds.Left+2;x<Bounds.Right-2;x++)for(int y=floor;y<=floor+2;y++)
            {
                var t=Main.tile[x,y];if(t.HasTile)continue;
                t.HasTile=true;t.TileType=(ushort)(y==floor?ModContent.TileType<GroveGrass>():TileID.Dirt);
            }
        }
        public bool RefreshPlants()
        {
            if(!Valid(Bounds)||Bounds.Width!=GalleryWidth)return false;
            // Only our own decorations are replaced; player blocks and living trees survive.
            for(int x=Bounds.Left;x<Bounds.Right;x++)for(int y=Bounds.Top;y<Bounds.Bottom;y++)
            {
                var t=Main.tile[x,y];
                if(t.HasTile&&TileLoader.GetTile(t.TileType) is GrovePlant)t.ClearTile();
            }
            int floor=Bounds.Bottom-4;
            EnsureTree(Bounds.Left+17,floor); // Preserve the original preview's tree position.
            EnsureTree(Bounds.Left+43,floor);
            EnsureTree(Bounds.Left+164,floor);
            foreach(int dx in new[]{8,12,39,72,158,172})Place<GroveTuft>(dx,floor);
            foreach(int dx in new[]{21,24,168})Place<GroveTallGrass>(dx,floor);
            Place<GroveSmallBush>(29,floor);Place<GroveSmallFern>(35,floor);
            Place<GroveBush>(55,floor);Place<GroveFern>(66,floor);
            Place<GroveMoss>(48,floor);Place<GroveMoss>(154,floor);
            Place<GroveRoots>(77,floor);Place<GroveFallenLog>(87,floor);Place<GroveHollowLog>(101,floor);
            Place<GrovePebble>(113,floor);Place<GroveRock>(121,floor);Place<GroveLargeRock>(131,floor);Place<GroveBoulder>(147,floor);
            plantLayout=CurrentPlantLayout;triedUpgrade=true;
            WorldGen.RangeFrame(Bounds.Left,Bounds.Top,Bounds.Right,Bounds.Bottom);
            return true;
        }
        private void Place<T>(int dx,int floor) where T:ModTile
        {
            int type=ModContent.TileType<T>(),origin=Bounds.Left+dx;
            var data=TileObjectData.GetTileData(type,0);
            int left=origin-data.Origin.X,top=floor-1-data.Origin.Y;
            for(int x=left;x<left+data.Width;x++)for(int y=top;y<floor;y++)
                if(Main.tile[x,y].HasTile||Main.tile[x,y].LiquidAmount>0)return;
            WorldGen.PlaceObject(origin,floor-1,type,true);
        }
        private void EnsureTree(int x,int floor)
        {
            // Grow on vanilla grass first, then use the grove's native ModTree renderer.
            for(int tx=x-2;tx<=x+2;tx++)
            {
                var ground=Main.tile[tx,floor];
                if(!ground.HasTile||ground.TileType!=ModContent.TileType<GroveGrass>())return;
                for(int y=floor-40;y<floor;y++)if(Main.tile[tx,y].HasTile)return;
            }
            for(int tx=x-2;tx<=x+2;tx++){var t=Main.tile[tx,floor];t.TileType=TileID.Grass;}
            try{if(!WorldGen.GrowEpicTree(x,floor))WorldGen.GrowTree(x,floor);}
            finally{for(int tx=x-2;tx<=x+2;tx++){var t=Main.tile[tx,floor];t.TileType=(ushort)ModContent.TileType<GroveGrass>();}}
        }
    }
}
