using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace IslandAdventure.Travel
{
    public static class BoatGeometry
    {
        public static (Vector2 Pivot, Vector2 Tip, Vector2 Blade) Oar(Vector2 center, float scale,
            bool left, float time, bool rowing)
        {
            float swing = rowing ? (float)Math.Sin(time * 3.5f) * 7 : 0;
            Vector2 pivot = center + new Vector2(left ? 12 : -12, -10) * scale;
            Vector2 tip = pivot + new Vector2((left ? -1 : 1) * (25 + swing), 17) * scale;
            return (pivot, tip, tip + new Vector2(left ? -7 : 7, 5) * scale);
        }

        // Destination rectangles keep the result independent of MagicPixel's
        // actual texture dimensions. Also reject invalid or runaway geometry.
        public static IEnumerable<Rectangle> PixelLine(Vector2 start, Vector2 end, float thickness)
        {
            if (!float.IsFinite(start.X) || !float.IsFinite(start.Y)
                || !float.IsFinite(end.X) || !float.IsFinite(end.Y)
                || !float.IsFinite(thickness) || (end - start).LengthSquared() > 512 * 512
                || Math.Abs(start.X) > 100000 || Math.Abs(start.Y) > 100000)
                yield break;
            int x = (int)Math.Round(start.X), y = (int)Math.Round(start.Y);
            int endX = (int)Math.Round(end.X), endY = (int)Math.Round(end.Y);
            int dx = Math.Abs(endX - x), dy = -Math.Abs(endY - y);
            int sx = x < endX ? 1 : -1, sy = y < endY ? 1 : -1;
            int error = dx + dy;
            int width = Math.Clamp((int)Math.Round(thickness), 1, 16);
            for (int step = 0; step < 1024; step++)
            {
                yield return new Rectangle(x - width / 2, y - width / 2, width, width);
                if (x == endX && y == endY) yield break;
                int doubled = error * 2;
                if (doubled >= dy) { error += dy; x += sx; }
                if (doubled <= dx) { error += dx; y += sy; }
            }
        }
    }
}
