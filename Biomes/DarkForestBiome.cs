using IslandAdventure.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace IslandAdventure.Biomes
{
    public class DarkForestBiome : ModBiome
    {
        public override string BestiaryIcon => "Terraria/Images/Item_9";
        public override string BackgroundPath => "Terraria/Images/MapBG1";
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicID.JungleNight;
        public override bool IsBiomeActive(Player player) => SubworldSystem.IsActive<IslandSubworld>()
            && !player.ZoneBeach && !player.ZoneUnderworldHeight
            && (player.ZoneOverworldHeight || player.ZoneSkyHeight);
    }
}
