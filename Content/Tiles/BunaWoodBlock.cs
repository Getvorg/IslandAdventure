using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Items;

namespace IslandAdventure.Content.Tiles
{
    public class BunaWoodBlock : ModTile
    {
        public override string Texture=>"IslandAdventure/Assets/Tiles/BunaWoodBlock";
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type]=Main.tileBlockLight[Type]=true;
            Main.tileMergeDirt[Type]=true;
            Main.tileMerge[Type][TileID.WoodBlock]=Main.tileMerge[TileID.WoodBlock][Type]=true;
            DustType=DustID.WoodFurniture;HitSound=SoundID.Dig;
            RegisterItemDrop(ModContent.ItemType<BunaWood>());
            VanillaFallbackOnModDeletion=TileID.WoodBlock;
            AddMapEntry(new Color(178,149,110),CreateMapEntryName());
        }
    }
}
