using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using IslandAdventure.Content.Items;
using IslandAdventure.Content.Trees;

namespace IslandAdventure.Content.Tiles
{
    public class BunaSapling : ModTile
    {
        public override string Texture=>"IslandAdventure/Assets/Tiles/BunaSapling";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type]=Main.tileCut[Type]=Main.tileNoFail[Type]=true;
            Main.tileLavaDeath[Type]=true;
            TileID.Sets.SwaysInWindBasic[Type]=true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.Origin=new Point16(0,1);
            TileObjectData.newTile.AnchorBottom=new AnchorData(AnchorType.SolidTile,1,0);
            TileObjectData.newTile.AnchorValidTiles=new[]{(int)TileID.Grass};
            TileObjectData.addTile(Type);
            DustType=DustID.Grass;HitSound=SoundID.Grass;
            RegisterItemDrop(ModContent.ItemType<BunaNut>());
            VanillaFallbackOnModDeletion=TileID.Saplings;
            AddMapEntry(new Color(94,145,54),CreateMapEntryName());
        }
        public override void RandomUpdate(int i,int j)
        {
            if(Main.netMode!=NetmodeID.MultiplayerClient&&Main.tile[i,j].TileFrameY%36==18&&WorldGen.genRand.NextBool(5))TryGrow(i,j);
        }
        public static bool TryGrow(int x,int y)
        {
            if(Main.netMode==NetmodeID.MultiplayerClient||!WorldGen.InWorld(x,y,45))return false;
            int type=ModContent.TileType<BunaSapling>();
            var tile=Main.tile[x,y];if(!tile.HasTile||tile.TileType!=type)return false;
            int top=y-(tile.TileFrameY%36)/18,floor=top+2;
            var upper=Main.tile[x,top];var lower=Main.tile[x,top+1];
            if(!upper.HasTile||!lower.HasTile||upper.TileType!=type||lower.TileType!=type)return false;
            // Suspend only HasTile: failed growth retains frame, paint and wires.
            upper.HasTile=lower.HasTile=false;
            bool grown=false;
            try{grown=BunaClassicTree.TryGrow(x,floor,tall:false);}
            finally{if(!grown)upper.HasTile=lower.HasTile=true;}
            if(grown)
            {
                BunaTreeEffects.EmitGrowth(x,floor);
                if(Main.netMode==NetmodeID.Server)NetMessage.SendTileSquare(-1,x-5,floor-40,11,41);
            }
            return grown;
        }
    }
}
