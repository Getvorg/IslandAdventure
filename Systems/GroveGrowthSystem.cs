using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;

namespace IslandAdventure.Systems
{
    // Keep native turf spreading and native vine/grass updates. Suppress only
    // the other spontaneous plants and their growth inside the art prototype.
    public class GroveGrowthSystem : ModSystem
    {
        private readonly List<Hook> hooks=new();
        private int grassUpdate,herbSeeding;
        private bool Restricted(int x,int y)=>!WorldGen.generatingWorld
            &&WorldGen.InWorld(x,y,1)&&(GroveAppearance.InGrove(x,y)||GroveAppearance.InGrove(x,y+1));
        private void Add(string name,Delegate hook)
            =>hooks.Add(new Hook(typeof(WorldGen).GetMethod(name,BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic),hook));
        public override void Load()
        {
            Add("UpdateWorld_GrassGrowth",(Action<Action<int,int,int,int,int,int,bool>,int,int,int,int,int,int,bool>)GrassGrowth);
            Add("PlaceTile",(Func<Func<int,int,int,bool,bool,int,int,bool>,int,int,int,bool,bool,int,int,bool>)PlaceTile);
            Add("PlacePumpkin",(Action<Action<int,int>,int,int>)PlacePumpkin);
            Add("plantDye",(Action<Action<int,int,bool>,int,int,bool>)Dye);
            Add("PlantAlch",(Action<Action>)SeedHerbs);
            Add("PlaceAlch",(Func<Func<int,int,int,bool>,int,int,int,bool>)PlaceHerb);
            Add("GrowAlch",(Action<Action<int,int>,int,int>)GrowHerb);
            Add("GrowPumpkin",(Action<Action<int,int,int>,int,int,int>)GrowPumpkin);
            Add("AttemptToGrowTreeFromSapling",(Func<Func<int,int,bool,bool>,int,int,bool,bool>)GrowSapling);
        }
        public override void Unload()
        {
            foreach(var hook in hooks)hook.Dispose();
            hooks.Clear();grassUpdate=herbSeeding=0;
        }
        private void GrassGrowth(Action<int,int,int,int,int,int,bool> orig,int i,int j,int minI,int maxI,int minJ,int maxJ,bool underground)
        {
            bool restrict=Restricted(i,j)&&Main.tile[i,j].HasTile&&Main.tile[i,j].TileType==TileID.Grass;
            if(restrict)grassUpdate++;
            try{orig(i,j,minI,maxI,minJ,maxJ,underground);}
            finally{if(restrict)grassUpdate--;}
        }
        private bool PlaceTile(Func<int,int,int,bool,bool,int,int,bool> orig,int i,int j,int type,bool mute,bool forced,int player,int style)
        {
            bool placed=orig(i,j,type,mute,forced,player,style);
            if(placed&&grassUpdate>0&&Restricted(i,j)&&type==TileID.Plants)
            {
                var tile=Main.tile[i,j];
                // Native frame 144 is a harvestable mushroom, not decorative grass.
                // Pick an ordinary grass frame BEFORE vanilla sends its tile update.
                if(tile.HasTile&&tile.TileType==TileID.Plants&&tile.TileFrameX==144)
                    tile.TileFrameX=0;
            }
            return placed;
        }
        private void PlacePumpkin(Action<int,int> orig,int x,int y)
        {if(grassUpdate==0||!Restricted(x,y))orig(x,y);}
        private void Dye(Action<int,int,bool> orig,int x,int y,bool exotic)
        {if(!Restricted(x,y))orig(x,y,exotic);}
        private void SeedHerbs(Action orig)
        {
            herbSeeding++;
            try{orig();}finally{herbSeeding--;}
        }
        private bool PlaceHerb(Func<int,int,int,bool> orig,int x,int y,int style)
            =>(herbSeeding==0||!Restricted(x,y))&&orig(x,y,style);
        private void GrowHerb(Action<int,int> orig,int x,int y)
        {if(!Restricted(x,y))orig(x,y);}
        private void GrowPumpkin(Action<int,int,int> orig,int x,int y,int type)
        {if(!Restricted(x,y))orig(x,y,type);}
        private bool GrowSapling(Func<int,int,bool,bool> orig,int x,int y,bool underground)
            =>!Restricted(x,y)&&orig(x,y,underground);
    }
}
