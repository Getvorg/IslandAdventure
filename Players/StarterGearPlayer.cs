using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
        }
    }
}
