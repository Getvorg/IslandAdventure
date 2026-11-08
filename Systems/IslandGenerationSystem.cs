using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace IslandAdventure.Systems
{
    public class IslandGenerationSystem : ModSystem
    {
        internal static bool GeneratingIsland;

        // Keep vanilla order. An explicit selection also prevents mixed passes such
        // as Micro Biomes from adding jungle structures or underground cabins.
        private static readonly HashSet<string> NaturalPasses = new HashSet<string>(StringComparer.Ordinal)
        {
            "Reset", "Terrain", "Ocean Sand", "Tunnels", "Mount Caves",
            "Dirt Wall Backgrounds", "Rocks In Dirt", "Dirt In Rocks", "Clay",
            "Small Holes", "Dirt Layer Caves", "Rock Layer Caves", "Surface Caves",
            "Wavy Caves", "Grass", "Mushroom Patches", "Marble", "Granite", "Silt",
            "Shinies", "Webs", "Underworld", "Lakes", "Mountain Caves", "Beaches",
            "Gems", "Gravitating Sand", "Create Ocean Caves", "Shimmer", "Clean Up Dirt",
            "Dirt Rock Wall Runner", "Settle Liquids", "Remove Water From Sand",
            "Shell Piles", "Smooth World", "Waterfalls", "Wall Variety",
            "Life Crystals", "Buried Chests", "Surface Chests", "Water Chests",
            "Spider Caves", "Gem Caves", "Moss", "Cave Walls", "Quick Cleanup",
            "Pots", "Hellforge", "Spreading Grass", "Surface Ore and Stone",
            "Piles", "Spawn Point", "Grass Wall", "Sunflowers", "Planting Trees",
            "Herbs", "Dye Plants", "Weeds", "Vines", "Flowers", "Mushrooms",
            "Random Gems", "Moss Grass", "Settle Liquids Again",
            "Cactus, Palm Trees, & Coral", "Tile Cleanup", "Water Plants", "Stalac",
            "Remove Broken Traps", "Final Cleanup"
        };

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            if (!GeneratingIsland)
                return;

            // Never filter the primary world, or run unrelated mod generation passes
            // in a subworld whose excluded biomes those mods may expect to exist.
            tasks.RemoveAll(pass => !NaturalPasses.Contains(pass.Name)
                || !WorldGen.VanillaGenPasses.TryGetValue(pass.Name, out GenPass vanilla)
                || !ReferenceEquals(pass, vanilla));

            foreach (string required in new[] { "Reset", "Terrain", "Shinies", "Underworld", "Shimmer", "Spawn Point", "Final Cleanup" })
            {
                if (!tasks.Any(pass => pass.Name == required))
                    throw new InvalidOperationException("Island generation requires vanilla pass: " + required);
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                GenPass original = tasks[i];
                if (original.Name == "Settle Liquids" || original.Name == "Settle Liquids Again")
                    original = new PassLegacy(original.Name, SettleIslandLiquids, original.Weight);
                else if (original.Name == "Buried Chests")
                    original = new PassLegacy(original.Name, PlaceIslandChests, original.Weight);

                GenPass inner = original;
                tasks[i] = new PassLegacy(inner.Name, (progress, configuration) =>
                {
                    Mod.Logger.Info("Island generation started: " + inner.Name);
                    Stopwatch timer = Stopwatch.StartNew();
                    inner.Apply(progress, configuration);
                    if (inner.Name == "Reset")
                    {
                        // Dunes normally allocates these. Ocean Sand still records
                        // beach candidates even when dunes/pyramids are excluded.
                        GenVars.PyrX = new int[8];
                        GenVars.PyrY = new int[8];
                        GenVars.numPyr = 0;
                    }
                    Mod.Logger.Info($"Island generation finished: {inner.Name} ({timer.ElapsedMilliseconds} ms)");
                }, inner.Weight);
            }

            totalWeight = tasks.Sum(pass => pass.Weight);
            Mod.Logger.Info("Island vanilla generation passes: " + string.Join(", ", tasks.Select(pass => pass.Name)));
        }

        private void PlaceIslandChests(GenerationProgress progress, GameConfiguration configuration)
        {
            // The vanilla pass also searches the underground desert, which this
            // island deliberately never creates. Use ordinary depth-based loot.
            progress.Message = Lang.gen[31].Value;
            int placed = 0;
            const int target = 100;
            for (int attempt = 0; attempt < 5000 && placed < target; attempt++)
            {
                int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                int y = WorldGen.genRand.Next((int)Main.worldSurface + 20, Main.maxTilesY - 60);
                if (!Main.tile[x, y].HasTile && WorldGen.AddBuriedChest(x, y, notNearOtherChests: true))
                    placed++;
                progress.Set(attempt / 5000.0);
            }
            Mod.Logger.Info($"Island underground chests placed: {placed}");
            progress.Set(1);
        }

        private void SettleIslandLiquids(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = Lang.gen[27].Value;
            bool previousQuickSettle = Liquid.quickSettle;
            try
            {
                Liquid.worldGenTilesIgnoreWater(true);
                Liquid.QuickWater(3);
                WorldGen.WaterCheck();
                Liquid.quickSettle = true;

                // Vanilla can request five updates per queued liquid tile, ten
                // times over. Large queues make that quadratic work impractical.
                // Keep all water/lava; let normal simulation finish any remainder.
                const int maxUpdates = 600;
                Stopwatch timer = Stopwatch.StartNew();
                int updates = 0;
                while ((Liquid.numLiquid > 0 || LiquidBuffer.numLiquidBuffer > 0)
                    && updates < maxUpdates && timer.Elapsed.TotalSeconds < 8)
                {
                    Liquid.UpdateLiquid();
                    updates++;
                    progress.Set(updates / (double)maxUpdates);
                }

                Mod.Logger.Info($"Island liquids: {updates} updates in {timer.ElapsedMilliseconds} ms; "
                    + $"remaining {Liquid.numLiquid} active, {LiquidBuffer.numLiquidBuffer} buffered.");
            }
            finally
            {
                Liquid.quickSettle = previousQuickSettle;
                Liquid.worldGenTilesIgnoreWater(false);
                Main.tileSolid[TileID.RollingCactus] = false;
            }
            progress.Set(1);
        }
    }
}
