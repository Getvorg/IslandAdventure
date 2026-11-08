using IslandAdventure.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace IslandAdventure.Players
{
    public class BoatTravelPlayer : ModPlayer
    {
        private bool Locked => Player.whoAmI == Main.myPlayer && BoatTravelSystem.IsCinematic;

        public override void SetControls()
        {
            if (!Locked) return;
            Player.controlLeft = Player.controlRight = Player.controlUp = Player.controlDown = false;
            Player.controlJump = Player.controlHook = Player.controlMount = false;
            Player.controlUseItem = Player.controlUseTile = Player.controlThrow = false;
            Player.controlInv = false;
        }

        public override void PreUpdateMovement()
        {
            if (Locked) BoatTravelSystem.HoldPlayer(Player);
        }

        public override bool CanUseItem(Item item) => !Locked;
        public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => Locked;
    }
}
