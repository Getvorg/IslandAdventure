using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;

namespace IslandAdventure.Systems
{
    public static class QueenSounds
    {
        private const string Root = "IslandAdventure/Assets/Sounds/Queen/";
        private static SoundStyle Effect(string name, float volume, float pitchVariance = 0) => new SoundStyle(Root + name)
        {
            Volume = volume, PitchVariance = pitchVariance,
            MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        };
        public static readonly SoundStyle Awaken = Effect("Awaken", 0.8f);
        public static readonly SoundStyle DashCharge = Effect("DashCharge", 0.65f);
        public static readonly SoundStyle Dash = Effect("Dash", 0.8f, 0.05f);
        public static readonly SoundStyle Dive = Effect("Dive", 0.8f);
        public static readonly SoundStyle Wave = Effect("Wave", 0.6f);
        public static readonly SoundStyle Emerge = Effect("Emerge", 0.65f);
        public static readonly SoundStyle Summon = Effect("Summon", 0.8f);
        public static readonly SoundStyle Song = Effect("Song", 0.85f);
        public static readonly SoundStyle SongRelease = Effect("SongRelease", 0.65f);
        public static readonly SoundStyle Hurt = Effect("Hurt", 0.45f, 0.08f);
        public static readonly SoundStyle Death = Effect("Death", 0.85f);

        public static void Play(SoundStyle sound, Vector2 source)
        {
            if (Main.dedServ || Main.gameMenu || BoatTravelSystem.IsCinematic) return;
            // Boss warnings remain audible when she is just beyond the camera.
            // Still respects Terraria's sound-effect volume, including mute.
            float distance = Vector2.Distance(Main.LocalPlayer.Center, source);
            float gain = MathHelper.Clamp(1 - distance / 3000f, 0.25f, 1);
            if (distance <= 2600) SoundEngine.PlaySound(sound with { Volume = sound.Volume * gain });
        }
    }
}
