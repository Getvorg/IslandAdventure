using System;
using Terraria;
using Terraria.ModLoader;
namespace IslandAdventure.Systems
{
    public class GroveWindSystem : ModSystem
    {
        public static double Time { get; private set; }
        public override void OnWorldLoad()=>Time=0;
        public override void PostUpdateWorld(){if(!Main.gamePaused)Time+=1d/60;}
        public static int Offset(int row,int height,float amplitude,float drive)
        {
            float weight=Math.Clamp((height-4-row)/(float)Math.Max(1,height-4),0,1);
            return (int)Math.Round(amplitude*weight*weight*drive);
        }
    }
}
