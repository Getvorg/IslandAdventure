using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using IslandAdventure.Content.Trees;

namespace IslandAdventure.Systems
{
    // Root ownership survives saves. Cell snapshots retain identity while native
    // chopping clears the base before recursively breaking upper cells.
    public class BunaTreeRegistry : ModSystem
    {
        private readonly HashSet<Point> roots=new();
        private readonly Dictionary<Point,Point> cells=new();
        private readonly Dictionary<Point,List<Point>> rootCells=new();
        private readonly HashSet<Point> dirty=new();
        public bool Owns(int x,int floor)=>roots.Contains(new Point(x,floor));
        public override void OnWorldLoad(){roots.Clear();cells.Clear();rootCells.Clear();dirty.Clear();BunaTreeEffects.Reset();}
        public override void PreWorldGen()=>OnWorldLoad();
        public void Register(int x,int floor,bool broadcast=true)
        {
            if(!WorldGen.InWorld(x,floor,1))return;
            var root=new Point(x,floor);bool added=roots.Add(root);if(added||!rootCells.ContainsKey(root))Snapshot(root);
            if(added&&broadcast&&Main.netMode==NetmodeID.Server)Send(root,true);
        }
        private void Snapshot(Point root)
        {
            if(rootCells.TryGetValue(root,out var previous))foreach(var p in previous)cells.Remove(p);
            var list=new List<Point>();rootCells[root]=list;
            for(int x=root.X-2;x<=root.X+2;x++)for(int y=System.Math.Max(1,root.Y-40);y<root.Y;y++)
            {
                if(!WorldGen.InWorld(x,y,1))continue;
                var tile=Main.tile[x,y];if(!tile.HasTile||tile.TileType!=TileID.Trees)continue;
                WorldGen.GetTreeBottom(x,y,out int rx,out int ry);
                if(rx!=root.X||ry!=root.Y)continue;
                var p=new Point(x,y);list.Add(p);cells[p]=root;
            }
        }
        public bool TryRoot(int x,int y,out Point root,bool allowSnapshot=false)
        {
            root=default;if(!WorldGen.InWorld(x,y,1))return false;
            var tile=Main.tile[x,y];if(!tile.HasTile||tile.TileType!=TileID.Trees)return false;
            if(allowSnapshot&&cells.TryGetValue(new Point(x,y),out root)&&roots.Contains(root))return true;
            WorldGen.GetTreeBottom(x,y,out int rx,out int ry);
            if(!Owns(rx,ry)&&!BunaClassicTree.IsPreviewRoot(rx,ry))return false;
            Register(rx,ry);root=new Point(rx,ry);return true;
        }
        public void Changed(Point root)=>dirty.Add(root);
        public void Forget(int x,int floor)=>Remove(new Point(x,floor));
        public int Top(Point root)
        {
            int y=root.Y-1;
            while(y>1&&Main.tile[root.X,y-1].HasTile&&Main.tile[root.X,y-1].TileType==TileID.Trees)y--;
            return y;
        }
        private void Remove(Point root,bool broadcast=true)
        {
            roots.Remove(root);BunaTreeEffects.Forget(root);
            if(rootCells.Remove(root,out var list))foreach(var p in list)cells.Remove(p);
            if(broadcast&&Main.netMode==NetmodeID.Server)Send(root,false);
        }
        public override void PostUpdateWorld()
        {
            if(Main.netMode==NetmodeID.MultiplayerClient){foreach(var root in dirty)Snapshot(root);dirty.Clear();return;}
            foreach(var root in dirty)
            {
                var baseTile=Main.tile[root.X,root.Y-1];
                if(!baseTile.HasTile||baseTile.TileType!=TileID.Trees)Remove(root);
                else Snapshot(root);
            }
            dirty.Clear();
        }
        public override void SaveWorldData(TagCompound tag)
        {
            var list=new List<TagCompound>();
            foreach(var p in roots)
                if(Main.tile[p.X,p.Y-1].HasTile&&Main.tile[p.X,p.Y-1].TileType==TileID.Trees)
                    list.Add(new TagCompound{{"x",p.X},{"floor",p.Y}});
            tag["roots"]=list;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            foreach(var entry in tag.GetList<TagCompound>("roots"))Register(entry.GetInt("x"),entry.GetInt("floor"),false);
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(roots.Count);foreach(var root in roots){writer.Write(root.X);writer.Write(root.Y);}
        }
        public override void NetReceive(BinaryReader reader)
        {
            OnWorldLoad();int count=reader.ReadInt32();
            for(int i=0;i<count;i++)Register(reader.ReadInt32(),reader.ReadInt32(),false);
        }
        private void Send(Point root,bool add)
        {
            var packet=Mod.GetPacket();packet.Write((byte)1);packet.Write(root.X);packet.Write(root.Y);packet.Write(add);packet.Send();
        }
        public void ApplyRemote(int x,int floor,bool add)
        {
            if(!WorldGen.InWorld(x,floor,1))return;
            if(add)Register(x,floor,false);else Remove(new Point(x,floor),false);
        }
    }
}
