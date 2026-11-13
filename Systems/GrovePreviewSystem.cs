using System;
using Microsoft.Xna.Framework;
using IslandAdventure.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace IslandAdventure.Systems
{
    public class GrovePreviewSystem : ModSystem
    {
        public Rectangle Bounds { get; private set; }
        private Vector2 returnPosition;
        private bool canReturn;
        private string returnOwner;
        private int plantLayout;
        private const int CurrentPlantLayout = 4;
        public override void OnWorldLoad() { Bounds = Rectangle.Empty; canReturn=false; returnOwner=null; plantLayout=0; }
        public override void PreWorldGen() => OnWorldLoad();
        public override void SaveWorldData(TagCompound tag)
        {
            if(Bounds.Width>0){tag["x"]=Bounds.X;tag["y"]=Bounds.Y;tag["w"]=Bounds.Width;tag["h"]=Bounds.Height;}
            if(Bounds.Width>0)tag["plantLayout"]=plantLayout;
            if(canReturn){tag["returnX"]=returnPosition.X;tag["returnY"]=returnPosition.Y;tag["owner"]=returnOwner;}
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if(tag.GetInt("w")==56 && tag.GetInt("h")==46)
                Bounds=new Rectangle(tag.GetInt("x"),tag.GetInt("y"),56,46);
            plantLayout=tag.GetInt("plantLayout");
            if(tag.ContainsKey("owner")&&float.IsFinite(tag.GetFloat("returnX"))&&float.IsFinite(tag.GetFloat("returnY")))
            {returnPosition=new Vector2(tag.GetFloat("returnX"),tag.GetFloat("returnY"));returnOwner=tag.GetString("owner");canReturn=true;}
        }
        public override void PreUpdateWorld()
        {
            if(Main.gameMenu||WorldGen.generatingWorld||Main.netMode==NetmodeID.MultiplayerClient)return;
            if(Bounds.Width>0&&plantLayout<CurrentPlantLayout)RefreshPlants();
        }
        public bool Visit(Player player, out string message)
        {
            if (Main.netMode!=NetmodeID.SinglePlayer || BoatTravelSystem.IsCinematic || BoatTravelSystem.IsLoading)
            {message="Образец доступен в одиночной игре вне переправы.";return false;}
            foreach(var npc in Main.npc)if(npc.active&&npc.boss){message="Сначала заверши бой с боссом.";return false;}
            if(Bounds.Width==0)
            {
                int center=(int)player.Center.X/16;
                bool found=false;
                foreach(int dx in new[]{0,70,-70,140,-140})
                {
                    int left=Math.Clamp(center+dx-28,100,Main.maxTilesX-156);
                    for(int floor=Math.Clamp((int)(player.position.Y/16)-12,100,(int)Main.worldSurface-20);floor>=70;floor-=12)
                    {
                        var area=new Rectangle(left,floor-42,56,46);
                        if(area.Top<10||!Empty(area))continue;
                        Build(area);found=true;break;
                    }
                    if(found)break;
                }
                if(!found){message="Не найден свободный участок неба. Поднимись на поверхность и попробуй снова.";return false;}
            }
            if(!Valid(Bounds)){message="Запись тестового участка повреждена.";return false;}
            if(plantLayout<CurrentPlantLayout)RefreshPlants();
            Vector2 landing=new Vector2((Bounds.Left+5)*16,(Bounds.Bottom-4)*16-player.height);
            if(!ResumeSystem.TrySafePosition(player,landing,out var safe)){message="Место входа занято блоками.";return false;}
            if(!Bounds.Contains(player.Center.ToTileCoordinates())){returnPosition=player.position;returnOwner=player.name;canReturn=true;}
            Move(player,safe);
            message="Образец рощи готов. Осмотри траву, дерево, куст и папоротник. /grovepreview back — вернуться.";
            return true;
        }
        public bool Back(Player player)
        {
            if(BoatTravelSystem.IsCinematic||BoatTravelSystem.IsLoading||!canReturn||returnOwner!=player.name||!ResumeSystem.TrySafePosition(player,returnPosition,out var safe))return false;
            Move(player,safe);canReturn=false;return true;
        }
        private static void Move(Player p, Vector2 point)
        {
            p.position=p.oldPosition=point;p.velocity=Vector2.Zero;p.fallStart=(int)(point.Y/16);
            p.immune=true;p.immuneTime=Math.Max(p.immuneTime,120);
        }
        private static bool Valid(Rectangle area) => area.Left>=10&&area.Top>=10&&area.Right<Main.maxTilesX-10&&area.Bottom<Main.maxTilesY-10;
        private static bool Empty(Rectangle area)
        {
            if(!Valid(area))return false;
            for(int x=area.Left;x<area.Right;x++)for(int y=area.Top;y<area.Bottom;y++)
            {var t=Main.tile[x,y];if(t.HasTile||t.WallType!=WallID.None||t.LiquidAmount!=0)return false;}
            return true;
        }
        public bool RefreshPlants()
        {
            if(!Valid(Bounds))return false;
            int bush=ModContent.TileType<GroveBush>(),fern=ModContent.TileType<GroveFern>(),tuft=ModContent.TileType<GroveTuft>();
            for(int x=Bounds.Left;x<Bounds.Right;x++)for(int y=Bounds.Top;y<Bounds.Bottom;y++)
            {
                var t=Main.tile[x,y];
                if(t.HasTile&&(t.TileType==bush||t.TileType==fern||t.TileType==tuft))t.ClearTile();
            }
            int floor=Bounds.Bottom-4;
            PlacePlant(Bounds.Left+32,floor,bush,7,5);
            PlacePlant(Bounds.Left+43,floor,fern,8,6);
            foreach(int dx in new[]{8,25,28,37,50})PlacePlant(Bounds.Left+dx,floor,tuft,1,2);
            plantLayout=CurrentPlantLayout;
            return true;
        }
        private static void PlacePlant(int origin,int floor,int type,int width,int height)
        {
            int left=origin-width/2;
            for(int x=left;x<left+width;x++)for(int y=floor-height;y<floor;y++)
                if(Main.tile[x,y].HasTile||Main.tile[x,y].LiquidAmount>0)return;
            WorldGen.PlaceObject(origin,floor-1,type,true);
        }
        private void Build(Rectangle area)
        {
            int floor=area.Bottom-4;
            // Only originally empty space is occupied; existing terrain is untouched.
            for(int x=area.Left+2;x<area.Right-2;x++)for(int y=floor;y<=floor+2;y++)
            {var t=Main.tile[x,y];t.HasTile=true;t.TileType=(ushort)(y==floor?TileID.Grass:TileID.Dirt);}
            if(!WorldGen.GrowEpicTree(area.Left+17,floor))WorldGen.GrowTree(area.Left+17,floor);
            for(int x=area.Left+2;x<area.Right-2;x++){var t=Main.tile[x,floor];t.TileType=(ushort)ModContent.TileType<GroveGrass>();}
            Bounds=area;
            RefreshPlants();
            WorldGen.RangeFrame(area.Left,area.Top,area.Right,area.Bottom);
        }
    }
}
