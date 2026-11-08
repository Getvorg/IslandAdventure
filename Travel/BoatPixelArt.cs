using System;
using Microsoft.Xna.Framework;

namespace IslandAdventure.Travel
{
    public static class BoatPixelArt
    {
        public const int LogicalWidth = 96, LogicalHeight = 40;
        public const int Width = LogicalWidth * 2, Height = LogicalHeight * 2;
        public const int Waterline = 64;

        public static Color[] CreatePixels()
        {
            Color[] logical = new Color[LogicalWidth * LogicalHeight];
            void Box(int x, int y, int w, int h, Color c)
            {
                for (int j = Math.Max(0, y); j < Math.Min(LogicalHeight, y + h); j++)
                    for (int i = Math.Max(0, x); i < Math.Min(LogicalWidth, x + w); i++) logical[j * LogicalWidth + i] = c;
            }
            Color ink = new Color(43, 31, 32), dark = new Color(74, 46, 36);
            Color wood = new Color(131, 79, 46), warm = new Color(162, 102, 53);
            Color honey = new Color(192, 137, 70), gold = new Color(226, 179, 103);
            Color teal = new Color(44, 91, 99), blue = new Color(32, 59, 68);
            Color iron = new Color(88, 95, 101), steel = new Color(156, 163, 153);

            // Raised bow, curved keel and separate planks, rather than a scaled-up icon.
            for (int y = 21; y <= 37; y++)
            {
                int d = y - 21;
                int l = 4 + d / 3 + d * d / 35;
                int r = 94 - d / 2 - d * d / 28;
                Box(l, y, r - l, 1, ink);
                if (y < 37) Box(l + 1, y, r - l - 2, 1,
                    y >= 34 ? dark : d % 4 == 0 ? dark : d % 4 == 1 ? honey : d % 4 == 2 ? warm : wood);
            }
            Box(7, 26, 82, 1, blue); Box(8, 27, 79, 2, teal);
            Box(6, 22, 85, 1, honey);
            // Staggered plank joins, grain, iron knees and paired rivets.
            foreach (int x in new[] { 19, 38, 58, 77 })
            {
                Box(x, 23, 1, 12, dark);
                Box(x - 1, 23, 3, 2, iron); Box(x, 23, 1, 1, steel);
                Box(x, 30, 1, 1, gold); Box(x, 33, 1, 1, gold);
            }
            foreach (int x in new[] { 13, 33, 53, 73 })
            {
                Box(x, 25, 5, 1, wood); Box(x + 4, 29, 7, 1, dark);
                Box(x + 1, 32, 5, 1, honey);
            }
            // Gunwale with a dark inner lip and upward-curving ends.
            Box(3, 19, 89, 3, ink); Box(4, 19, 87, 1, gold);
            Box(6, 20, 83, 1, honey); Box(8, 21, 79, 1, dark);
            Box(90, 16, 3, 5, ink); Box(90, 16, 2, 1, gold); Box(89, 18, 3, 1, honey);
            Box(3, 17, 2, 3, ink); Box(3, 17, 1, 1, gold);
            // Benches behind the near gunwale; passengers remain in native scale.
            Box(29, 17, 11, 2, ink); Box(29, 17, 11, 1, honey);
            Box(52, 17, 12, 2, ink); Box(52, 17, 12, 1, warm);
            // Stern mooring cleat and tightly wound rope coil.
            Box(8, 17, 6, 1, iron); Box(10, 16, 2, 3, steel);
            Box(15, 15, 9, 4, dark); Box(16, 15, 7, 1, gold);
            Box(15, 16, 1, 2, honey); Box(23, 16, 1, 2, honey);
            Box(16, 18, 7, 1, honey); Box(18, 16, 3, 2, honey); Box(19, 16, 1, 1, dark);
            // Hand-repaired planks and a small metal anchor emblem on the bow.
            Box(60, 30, 10, 3, dark); Box(60, 30, 10, 1, warm);
            Box(61, 31, 1, 1, steel); Box(68, 31, 1, 1, steel);
            Box(82, 24, 1, 7, steel); Box(80, 25, 5, 1, steel);
            Box(79, 29, 1, 2, steel); Box(85, 29, 1, 2, steel);
            Box(80, 31, 5, 1, steel); Box(81, 32, 3, 1, iron);
            // Lantern gallows and a stitched, two-tone blue pennant.
            Box(76, 6, 2, 13, ink); Box(77, 6, 1, 13, honey);
            Box(76, 6, 14, 1, gold); Box(78, 7, 11, 1, dark);
            for (int i = 0; i < 5; i++) Box(78 + i, 12 - i, 1, 1, warm);
            Box(87, 7, 1, 2, iron);
            Box(72, 1, 1, 18, dark); Box(72, 1, 1, 1, gold);
            Box(60, 2, 12, 5, ink); Box(61, 3, 10, 2, teal);
            Box(63, 5, 8, 2, blue); Box(64, 7, 7, 1, blue);
            Box(66, 3, 1, 3, gold); Box(64, 4, 5, 1, gold);

            Color[] pixels = new Color[Width * Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    pixels[y * Width + x] = logical[(y / 2) * LogicalWidth + x / 2];
            return pixels;
        }
    }
}
