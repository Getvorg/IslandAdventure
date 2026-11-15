using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.Items
{
    // Keep native axe bonuses, stack counts and nut chances. Transform only
    // tree-break drops owned by Buna II; ordinary trees/items are unaffected.
    public class BunaTreeDrops : GlobalItem
    {
        public override void OnSpawn(Item item,IEntitySource source)
        {
            if(source is not EntitySource_TileBreak broken)return;
            if(!ModContent.GetInstance<BunaTreeRegistry>().TryRoot(broken.TileCoords.X,broken.TileCoords.Y,out _,true))return;
            int replacement=item.type==ItemID.Wood?ModContent.ItemType<BunaWood>():item.type==ItemID.Acorn?ModContent.ItemType<BunaNut>():0;
            if(replacement==0)return;
            int stack=item.stack;var center=item.Center;var velocity=item.velocity;
            item.SetDefaults(replacement);item.stack=stack;item.Center=center;item.velocity=velocity;
        }
    }
    public class BunaWoodRecipes : ModSystem
    {
        public override void AddRecipeGroups()=>RecipeGroup.recipeGroups[RecipeGroupID.Wood].ValidItems.Add(ModContent.ItemType<BunaWood>());
    }
}
