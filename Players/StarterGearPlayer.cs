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
            sword.SetDefaults(ItemID.Meowmere);
            yield return sword;

            Item wings = new Item();
            wings.SetDefaults(ItemID.CelestialStarboard);
            yield return wings;
        }
    }
}
