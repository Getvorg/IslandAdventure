using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;
namespace IslandAdventure.Content.Items
{
    public class DarkSoil : ModItem
    {
        public override string Texture => "IslandAdventure/Assets/Items/DarkSoil";
        public override void SetDefaults() { Item.DefaultToPlaceableTile(ModContent.TileType<DarkSoilTile>()); Item.width=Item.height=16; Item.maxStack=9999; }
    }
    public class DarkStone : ModItem
    {
        public override string Texture => "IslandAdventure/Assets/Items/DarkStone";
        public override void SetDefaults() { Item.DefaultToPlaceableTile(ModContent.TileType<DarkStoneTile>()); Item.width=Item.height=16; Item.maxStack=9999; }
    }
    public class DarkWood : ModItem
    {
        public override string Texture => "IslandAdventure/Assets/Items/DarkWood";
        public override void SetDefaults() { Item.DefaultToPlaceableTile(ModContent.TileType<DarkWoodTile>()); Item.width=Item.height=16; Item.maxStack=9999; }

    }
}
