using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Dusts;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.Trees
{
    public class BunaTreeEffects : GlobalTile
    {
        private static readonly Dictionary<Point,ulong> nextBurst=new();
        public static void Reset()=>nextBurst.Clear();
        public static void Forget(Point root)=>nextBurst.Remove(root);
        public override void Unload()=>Reset();
        public override bool CreateDust(int i,int j,int type,ref int dustType)
        {
            if(type==TileID.Trees&&ModContent.GetInstance<BunaTreeRegistry>().TryRoot(i,j,out _,true))dustType=ModContent.DustType<BunaBarkDust>();
            return true;
        }
        public override void KillTile(int i,int j,int type,ref bool fail,ref bool effectOnly,ref bool noItem)
        {
            if(type!=TileID.Trees||!ModContent.GetInstance<BunaTreeRegistry>().TryRoot(i,j,out var root,true))return;
            ModContent.GetInstance<BunaTreeRegistry>().Changed(root);
            if(Main.dedServ||Main.gameMenu||WorldGen.gen||WorldGen.generatingWorld)return;
            if(nextBurst.TryGetValue(root,out var until)&&Main.GameUpdateCount<until)return;
            nextBurst[root]=Main.GameUpdateCount+12;
            int top=ModContent.GetInstance<BunaTreeRegistry>().Top(root);
            Emit(root.X,top,fail?6:18);
        }
        public override void NearbyEffects(int i,int j,int type,bool closer)
        {
            if(!closer||Main.dedServ||Main.gamePaused||Main.gameMenu||WorldGen.gen||type!=TileID.Trees)return;
            var tile=Main.tile[i,j];if(tile.TileFrameX!=22||tile.TileFrameY<198||tile.WallType!=WallID.None)return;
            if(!ModContent.GetInstance<BunaTreeRegistry>().TryRoot(i,j,out _))return;
            int chance=System.Math.Abs(Main.windSpeedCurrent)>.35f?24:90;
            if(Main.rand.NextBool(chance))Emit(i,j,1);
        }
        private static void Emit(int x,int top,int count)
        {
            if(Main.dedServ)return;
            for(int n=0;n<count;n++)
            {
                var point=new Vector2(x*16+8+Main.rand.Next(-40,41),top*16-42+Main.rand.Next(-24,25));
                int index=Dust.NewDust(point,1,1,ModContent.DustType<BunaLeafDust>());
                Main.dust[index].velocity.X=Main.windSpeedCurrent*1.5f+Main.rand.NextFloat(-.6f,.6f);
            }
        }
        public static void EmitGrowth(int x,int floor)
        {
            if(!Main.dedServ)Emit(x,ModContent.GetInstance<BunaTreeRegistry>().Top(new Point(x,floor)),16);
        }
    }
}
