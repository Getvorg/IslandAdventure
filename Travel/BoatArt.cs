using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace IslandAdventure.Travel
{
    public static class BoatArt
    {
        private static Texture2D texture;

        private static Texture2D Texture()
        {
            if (texture != null && !texture.IsDisposed) return texture;
            const int width = 96, height = 48;
            Color[] pixels = new Color[width * height];
            void Box(int x, int y, int w, int h, Color c)
            {
                for (int j = Math.Max(0, y); j < Math.Min(height, y + h); j++)
                    for (int i = Math.Max(0, x); i < Math.Min(width, x + w); i++) pixels[j * width + i] = c;
            }
            Color dark = new Color(54, 33, 30), wood = new Color(134, 77, 43);
            Color trim = new Color(216, 163, 85), lightWood = new Color(170, 104, 55);
            // Original pixel-art hull, narrowed in successive rows toward the keel.
            for (int y = 20; y <= 38; y++)
            {
                int inset = (y - 20) / 2;
                Box(4 + inset, y, 88 - inset * 2, 1, dark);
                if (y > 21 && y < 37) Box(7 + inset, y, 82 - inset * 2, 1, y % 5 == 0 ? dark : wood);
            }
            Box(1, 18, 94, 3, dark); Box(3, 18, 90, 1, trim);
            Box(6, 21, 84, 2, lightWood); Box(21, 36, 54, 2, dark);
            foreach (int x in new[] { 22, 43, 64, 78 }) Box(x, 25, 1, 8, lightWood);
            Box(11, 10, 15, 8, dark); Box(12, 11, 13, 6, lightWood);
            Box(17, 11, 2, 6, trim); Box(12, 13, 13, 1, trim);
            Box(31, 15, 13, 3, trim); Box(55, 15, 13, 3, trim);
            Box(81, 2, 3, 16, dark); Box(80, 2, 12, 2, trim);
            Box(88, 4, 1, 3, dark); Box(85, 7, 7, 8, dark);
            Box(86, 8, 5, 5, new Color(255, 211, 106)); Box(88, 8, 1, 5, trim);
            Box(85, 14, 7, 1, trim);
            // A small blue pennant and mooring rope distinguish the fishing boat.
            Box(78, 1, 1, 9, dark); Box(68, 1, 10, 4, new Color(69, 132, 157));
            Box(72, 5, 6, 2, new Color(50, 94, 119));
            for (int x = 5; x < 20; x++) Box(x, 14 + (x % 3), 1, 1, trim);
            texture = new Texture2D(Main.instance.GraphicsDevice, width, height);
            texture.SetData(pixels);
            return texture;
        }

        public static void Draw(SpriteBatch batch, Vector2 waterline, float scale, bool left, float time)
        {
            Vector2 center = waterline + new Vector2(0, (float)Math.Sin(time * 2.2f) * 1.4f * scale);
            batch.Draw(Texture(), center, null, Color.White, (float)Math.Sin(time * 1.7f) * 0.012f,
                new Vector2(48, 34), scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            Vector2 oar = center + new Vector2((left ? 14 : -14) * scale, -8 * scale);
            Vector2 tip = oar + new Vector2((left ? -1 : 1) * (25 + (float)Math.Sin(time * 4) * 9), 18) * scale;
            Line(batch, oar, tip, new Color(197, 151, 89), 2 * scale);
            Line(batch, tip, tip + new Vector2(left ? -6 : 6, 5) * scale, new Color(141, 89, 51), 4 * scale);
        }

        public static void Line(SpriteBatch batch, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 difference = end - start;
            batch.Draw(TextureAssets.MagicPixel.Value, start, null, color,
                (float)Math.Atan2(difference.Y, difference.X), Vector2.Zero,
                new Vector2(difference.Length(), width), SpriteEffects.None, 0);
        }

        public static void Unload()
        {
            Texture2D old = texture;
            texture = null;
            if (old != null) Main.QueueMainThreadAction(() => { if (!old.IsDisposed) old.Dispose(); });
        }
    }
}
