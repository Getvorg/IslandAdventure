using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;

namespace IslandAdventure.Content.Items
{
    public class BunaNut : ModItem
    {
        public override string Texture=>"IslandAdventure/Assets/Items/BunaNut";
        public override void SetStaticDefaults()=>Item.ResearchUnlockCount=25;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BunaSapling>());
            Item.width=32;Item.height=28;Item.maxStack=Item.CommonMaxStack;
            Item.value=Item.sellPrice(copper:2);
        }
    }
    public class BunaWood : ModItem
    {
        public override string Texture=>"IslandAdventure/Assets/Items/BunaWood";
        public override void SetStaticDefaults()=>Item.ResearchUnlockCount=100;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BunaWoodBlock>());
            Item.width=Item.height=32;Item.maxStack=Item.CommonMaxStack;
            Item.value=Item.sellPrice(copper:1);
        }
    }
}
