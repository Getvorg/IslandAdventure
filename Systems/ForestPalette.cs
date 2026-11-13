using System;
using Microsoft.Xna.Framework;

namespace IslandAdventure.Systems
{
    public enum ForestMaterial { Grass, Leaves, Bark, Soil, Stone, Plants }
    public static class ForestPalette
    {
        private static readonly Color[] Green = { new(5,17,9), new(12,34,18), new(24,57,30), new(43,83,45), new(73,112,68) };
        private static readonly Color[] Brown = { new(12,8,5), new(30,18,10), new(53,32,18), new(80,51,31), new(115,80,50) };
        private static readonly Color[] Earth = { new(18,13,9), new(43,29,19), new(69,47,31), new(99,72,49), new(134,103,73) };
        private static readonly Color[] Rock = { new(20,23,23), new(40,46,44), new(64,72,67), new(94,103,94), new(133,141,128) };

        // Terraria textures are premultiplied. Recolor straight RGB, then return
        // premultiplied pixels with the original alpha, including soft edges.
        public static Color Recolor(Color source, ForestMaterial material)
        {
            if (source.A == 0) return source;
            float inverseAlpha = 255f / source.A;
            float r = Math.Min(255, source.R * inverseAlpha);
            float g = Math.Min(255, source.G * inverseAlpha);
            float b = Math.Min(255, source.B * inverseAlpha);
            bool green = g > r * 1.03f && g >= b * .95f;
            bool warm = r > g * 1.08f && r > b * 1.08f;
            if (material == ForestMaterial.Plants && !green) return source; // flowers keep their colors
            Color[] ramp = material switch {
                ForestMaterial.Grass => green ? Green : Earth,
                ForestMaterial.Leaves => warm ? Brown : Green, // brown twigs stay brown
                ForestMaterial.Bark => Brown,
                ForestMaterial.Soil => Earth,
                ForestMaterial.Stone => Rock,
                _ => Green
            };
            float value = MathHelper.Clamp((.2126f * r + .7152f * g + .0722f * b) / 255f, 0, 1) * (ramp.Length - 1);
            int index = Math.Min((int)value, ramp.Length - 2);
            Color color = Color.Lerp(ramp[index], ramp[index + 1], value - index);
            return Color.FromNonPremultiplied(color.R, color.G, color.B, source.A);
        }
    }
}
