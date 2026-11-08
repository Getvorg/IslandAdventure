using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Items;

namespace IslandAdventure.Players
{
    public class StarterGearPlayer : ModPlayer
    {
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            // Give test equipment only when creating a character, not on respawn.
            if (mediumCoreDeath)
                yield break;

            Item sword = new Item();
            sword.SetDefaults(ItemID.TerraBlade);
            sword.Prefix(PrefixID.Legendary);
            yield return sword;

            Item wings = new Item();
            wings.SetDefaults(ItemID.LongRainbowTrailWings);
            yield return wings;

            Item boots = new Item();
            boots.SetDefaults(ItemID.TerrasparkBoots);
            yield return boots;

            Item pickaxe = new Item();
            pickaxe.SetDefaults(ItemID.ShroomiteDiggingClaw);
            yield return pickaxe;
            foreach (Item sample in TideliteSamples()) yield return sample;
        }

        public static IEnumerable<Item> TideliteSamples()
        {
            int[] types = { ModContent.ItemType<TideliteOre>(), ModContent.ItemType<TideliteBar>(),
                ModContent.ItemType<TideliteBrick>(), ModContent.ItemType<TideliteBrickWall>(),
                ModContent.ItemType<TideliteSword>(), ModContent.ItemType<TidelitePickaxe>(),
                ModContent.ItemType<TideliteAxe>(), ModContent.ItemType<TideliteHammer>(),
                ItemID.Furnace, ItemID.IronAnvil };
            int[] stacks = { 100, 50, 100, 100, 1, 1, 1, 1, 1, 1 };
            for (int i = 0; i < types.Length; i++)
            {
                Item item = new Item();
                item.SetDefaults(types[i]); item.stack = stacks[i];
                yield return item;
            }
        }
    }
}
