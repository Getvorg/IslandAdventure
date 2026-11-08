using System.Collections.Generic;
using IslandAdventure.Systems;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;

namespace IslandAdventure.Subworlds
{
    public class IslandSubworld : Subworld
    {
        // Vanilla generation assumes at least the dimensions of a small world.
        public override int Width => 4200;
        public override int Height => 1200;
        public override bool ShouldSave => true;
        public override bool NormalUpdates => true;
        public override void DrawMenu(GameTime gameTime) => BoatTravelSystem.DrawLoading();

        public override List<GenPass> Tasks => new List<GenPass>
        {
            new PassLegacy("Island vanilla generation", GenerateIsland)
        };

        private void GenerateIsland(GenerationProgress progress, GameConfiguration configuration)
        {
            // Let Terraria initialize GenVars, ore choices and the generation config.
            // The system below filters passes only during this nested invocation.
            GenerationProgress outerProgress = WorldGenerator.CurrentGenerationProgress;
            IslandGenerationSystem.GeneratingIsland = true;
            try
            {
                WorldGen.GenerateWorld(Main.ActiveWorldFileData.Seed, new GenerationProgress());
            }
            finally
            {
                IslandGenerationSystem.GeneratingIsland = false;
                // Subworld Library still needs to finish its enclosing generation pass.
                WorldGenerator.CurrentGenerationProgress = outerProgress;
                WorldGen.generatingWorld = true;
            }
            progress.Set(1);
        }
    }
}
