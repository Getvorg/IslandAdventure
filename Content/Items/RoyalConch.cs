using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.NPCs.Bosses;
using IslandAdventure.Subworlds;
using IslandAdventure.Systems;
using SubworldLibrary;

namespace IslandAdventure.Content.Items
{
    public class RoyalConch : ModItem
    {
        public override string Texture => "IslandAdventure/Assets/Items/RoyalConch";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 40;
            Item.maxStack = 1; Item.consumable = false;
            Item.useTime = Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Roar;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(silver: 20);
        }
        public override bool CanUseItem(Player player) => Main.netMode == NetmodeID.SinglePlayer
            && SubworldSystem.IsActive<IslandSubworld>() && !NPC.AnyNPCs(ModContent.NPCType<MermaidQueen>())
            && !BoatTravelSystem.IsCinematic && QueenEncounter.TryFindSea(player, out _);
        public override bool? UseItem(Player player)
        {
            bool success = QueenEncounter.TrySummon(player, true, out string message);
            if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(message);
            return success;
        }
        public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Coral, 5)
            .AddIngredient(ItemID.Seashell, 5).AddIngredient(ItemID.FallenStar)
            .AddTile(TileID.WorkBenches).Register();
    }
}
