using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;

namespace IslandAdventure.Content.Items
{
    public abstract class TideliteItem : ModItem
    {
        public override string Texture => "IslandAdventure/Assets/Items/" + Name;
    }

    public class TideliteOre : TideliteItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 100;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<TideliteOreTile>());
            Item.width = Item.height = 32;
            Item.value = Item.sellPrice(copper: 12);
        }
    }

    public class TideliteBar : TideliteItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<TideliteBarTile>());
            Item.width = 32; Item.height = 24;
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes() => CreateRecipe().AddIngredient<TideliteOre>(4).AddTile(TileID.Furnaces).Register();
    }

    public class TideliteBrick : TideliteItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 100;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<TideliteBrickTile>());
            Item.width = Item.height = 32;
        }
        public override void AddRecipes()
        {
            CreateRecipe(2).AddIngredient<TideliteOre>().AddIngredient(ItemID.StoneBlock, 2).AddTile(TileID.Furnaces).Register();
            CreateRecipe().AddIngredient<TideliteBrickWall>(4).AddTile(TileID.WorkBenches).Register();
        }
    }

    public class TideliteBrickWall : TideliteItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 400;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall(ModContent.WallType<Walls.TideliteWall>());
            Item.width = Item.height = 28;
        }
        public override void AddRecipes() => CreateRecipe(4).AddIngredient<TideliteBrick>().AddTile(TileID.WorkBenches).Register();
    }

    public class TideliteSword : TideliteItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.GoldBroadsword);
            Item.width = Item.height = 40;
            Item.damage = 24; Item.knockBack = 5f;
            Item.useTime = Item.useAnimation = 25;
            Item.autoReuse = true;
            Item.value = Item.sellPrice(silver: 30); Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes() => CreateRecipe().AddIngredient<TideliteBar>(8).AddTile(TileID.Anvils).Register();
    }

    public class TidelitePickaxe : TideliteItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.GoldPickaxe);
            Item.width = Item.height = 32;
            Item.damage = 10; Item.pick = 60;
            Item.useTime = 15; Item.useAnimation = 20;
            Item.value = Item.sellPrice(silver: 40); Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes() => CreateRecipe().AddIngredient<TideliteBar>(12).AddRecipeGroup(RecipeGroupID.Wood, 4).AddTile(TileID.Anvils).Register();
    }

    public class TideliteAxe : TideliteItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.GoldAxe);
            Item.width = Item.height = 36;
            Item.damage = 13; Item.axe = 12;
            Item.useTime = 18; Item.useAnimation = 27;
            Item.value = Item.sellPrice(silver: 30); Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes() => CreateRecipe().AddIngredient<TideliteBar>(9).AddRecipeGroup(RecipeGroupID.Wood, 3).AddTile(TileID.Anvils).Register();
    }

    public class TideliteHammer : TideliteItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.GoldHammer);
            Item.width = Item.height = 36;
            Item.damage = 14; Item.hammer = 55;
            Item.useTime = 18; Item.useAnimation = 27;
            Item.value = Item.sellPrice(silver: 25); Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes() => CreateRecipe().AddIngredient<TideliteBar>(8).AddRecipeGroup(RecipeGroupID.Wood, 3).AddTile(TileID.Anvils).Register();
    }
}
