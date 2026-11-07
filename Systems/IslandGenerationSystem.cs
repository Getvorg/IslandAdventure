using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
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
            "Gems", "Gravitating Sand", "Create Ocean Caves", "Clean Up Dirt",
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

            foreach (string required in new[] { "Reset", "Terrain", "Shinies", "Underworld", "Spawn Point", "Final Cleanup" })
            {
                if (!tasks.Any(pass => pass.Name == required))
                    throw new InvalidOperationException("Island generation requires vanilla pass: " + required);
            }

            totalWeight = tasks.Sum(pass => pass.Weight);
            Mod.Logger.Info("Island vanilla generation passes: " + string.Join(", ", tasks.Select(pass => pass.Name)));
        }
    }
}
