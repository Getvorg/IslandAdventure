using System;
using Microsoft.Xna.Framework;

namespace IslandAdventure.Travel
{
    public static class BoatPixelArt
    {
        public const int LogicalWidth = 64, LogicalHeight = 28;
        public const int Width = LogicalWidth * 2, Height = LogicalHeight * 2;

        public static Color[] CreatePixels()
        {
            Color[] logical = new Color[LogicalWidth * LogicalHeight];
            void Box(int x, int y, int w, int h, Color c)
            {
                for (int j = Math.Max(0, y); j < Math.Min(LogicalHeight, y + h); j++)
                    for (int i = Math.Max(0, x); i < Math.Min(LogicalWidth, x + w); i++) logical[j * LogicalWidth + i] = c;
            }
            Color outline = new Color(46, 30, 25), shade = new Color(80, 48, 32);
            Color wood = new Color(136, 83, 45), light = new Color(175, 119, 62);
            Color edge = new Color(209, 161, 87), iron = new Color(83, 89, 98);
            for (int y = 13; y <= 23; y++)
            {
                int d = y - 13;
                int l = 3 + d / 2 + d * d / 45;
                int r = 61 - d / 2 - d * d / 32;
                Box(l, y, r - l, 1, outline);
                if (y < 23) Box(l + 1, y, r - l - 2, 1, d % 3 == 0 ? shade : d % 3 == 1 ? light : wood);
            }
            Box(2, 12, 58, 2, outline); Box(3, 12, 56, 1, edge);
            Box(4, 14, 54, 1, light); Box(59, 10, 2, 3, outline);
            Box(59, 10, 1, 1, edge); Box(3, 10, 1, 3, shade);
            foreach (int x in new[] { 14, 26, 38, 50 })
            {
                Box(x, 14, 1, 8, shade);
                Box(x, 15, 1, 1, iron); Box(x, 20, 1, 1, iron);
            }
            Box(16, 10, 8, 2, outline); Box(16, 10, 8, 1, edge);
            Box(34, 10, 8, 2, outline); Box(34, 10, 8, 1, light);
            Box(54, 2, 2, 10, outline); Box(55, 2, 1, 9, light);
            Box(53, 2, 9, 1, edge);
            Box(52, 0, 1, 6, shade);
            Box(46, 0, 6, 2, new Color(44, 111, 139));
            Box(48, 2, 4, 1, new Color(31, 75, 100));
            Box(6, 10, 7, 2, shade); Box(7, 10, 5, 1, edge);
            Box(8, 9, 3, 1, edge); Box(8, 11, 3, 1, light);

            // Author at half resolution, then enlarge with exact nearest-neighbor
            // duplication: every logical pixel is one solid 2x2 square.
            Color[] pixels = new Color[Width * Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    pixels[y * Width + x] = logical[(y / 2) * LogicalWidth + x / 2];
            return pixels;
        }
    }
}
