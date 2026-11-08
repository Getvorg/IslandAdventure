using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace IslandAdventure.Travel
{
    public static class BoatArt
    {
        public const int Width = 128;
        public const int Height = 56;
        public const int Waterline = 40;
        private static Texture2D texture;

        private static Texture2D Texture()
        {
            if (texture != null && !texture.IsDisposed) return texture;
            Color[] pixels = new Color[Width * Height];
            void Box(int x, int y, int w, int h, Color c)
            {
                for (int j = Math.Max(0, y); j < Math.Min(Height, y + h); j++)
                    for (int i = Math.Max(0, x); i < Math.Min(Width, x + w); i++) pixels[j * Width + i] = c;
            }
            Color outline = new Color(46, 30, 25), shade = new Color(80, 48, 32);
            Color wood = new Color(136, 83, 45), light = new Color(175, 119, 62);
            Color edge = new Color(209, 161, 87), iron = new Color(83, 89, 98);
            // Short, overlapping strakes with a raised stem, ribs and iron nails.
            // Integer pixel contours keep the silhouette sharp like Terraria tiles.
            for (int y = 25; y <= 46; y++)
            {
                int depth = y - 25;
                int l = 6 + depth / 2 + depth * depth / 90;
                int r = 122 - depth / 2 - depth * depth / 65;
                Box(l, y, r - l, 1, outline);
                if (y < 45)
                    Box(l + 2, y, r - l - 4, 1, depth % 5 == 0 ? shade : depth % 5 == 1 ? light : wood);
            }
            Box(4, 23, 116, 3, outline); Box(6, 23, 112, 1, edge);
            Box(8, 26, 108, 2, light); Box(117, 19, 5, 7, outline);
            Box(118, 19, 3, 2, edge); Box(5, 20, 4, 5, shade);
            foreach (int x in new[] { 28, 51, 76, 99 })
            {
                Box(x, 28, 2, 14, shade);
                Box(x + 1, 29, 1, 10, light);
                Box(x, 29, 2, 2, iron); Box(x, 38, 2, 2, iron);
            }
            Box(31, 19, 17, 3, outline); Box(32, 19, 15, 1, edge);
            Box(67, 19, 17, 3, outline); Box(68, 19, 15, 1, light);
            // Lantern bracket, pennant and the coil of mooring rope on the stern.
            Box(108, 3, 3, 20, outline); Box(109, 4, 1, 17, light);
            Box(107, 3, 16, 2, edge);
            Box(105, 1, 2, 12, shade); Box(93, 1, 12, 4, new Color(44, 111, 139));
            Box(96, 5, 9, 2, new Color(31, 75, 100));
            for (int coil = 0; coil < 3; coil++)
            {
                Box(12 + coil * 2, 19 - coil, 11 - coil * 3, 1, edge);
                Box(12 + coil * 2, 20 + coil, 11 - coil * 3, 1, shade);
            }
            texture = new Texture2D(Main.instance.GraphicsDevice, Width, Height);
            texture.SetData(pixels);
            return texture;
        }

        public static void Draw(SpriteBatch batch, Vector2 waterline, float scale, bool left, float time,
            bool rowing = false, Color? lighting = null)
        {
            Color tint = lighting ?? Color.White;
            Vector2 center = new Vector2((float)Math.Round(waterline.X),
                (float)Math.Round(waterline.Y + Math.Sin(time * 2.2f) * scale));
            // Vanilla prop sprites tie the custom hull to the game's palette.
            Vector2 Prop(float x, float y) => center + new Vector2(left ? 64 - x : x - 64, y - Waterline) * scale;
            DrawItem(batch, ItemID.WoodenCrate, Prop(20, 24), scale * 0.5f, tint);
            DrawItem(batch, ItemID.PalmWoodLantern, Prop(118, 23), scale * 0.5f, Color.White);
            batch.Draw(Texture(), center, null, tint, 0, new Vector2(64, Waterline), scale,
                left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            var oar = BoatGeometry.Oar(center, scale, left, time, rowing);
            Line(batch, oar.Pivot, oar.Tip, Tint(new Color(183, 134, 73), tint), 2 * scale);
            Line(batch, oar.Tip, oar.Blade, Tint(new Color(128, 80, 42), tint), 4 * scale);
        }

        public static void DrawItem(SpriteBatch batch, int itemId, Vector2 bottom, float scale, Color color)
        {
            Main.instance.LoadItem(itemId);
            Texture2D item = TextureAssets.Item[itemId].Value;
            batch.Draw(item, new Vector2((float)Math.Round(bottom.X), (float)Math.Round(bottom.Y)), null,
                color, 0, new Vector2(item.Width / 2f, item.Height), scale, SpriteEffects.None, 0);
        }

        private static Color Tint(Color color, Color light) => new Color(color.R * light.R / 255,
            color.G * light.G / 255, color.B * light.B / 255, color.A);

        public static void Line(SpriteBatch batch, Vector2 start, Vector2 end, Color color, float width)
        {
            foreach (Rectangle pixel in BoatGeometry.PixelLine(start, end, width))
                batch.Draw(TextureAssets.MagicPixel.Value, pixel, new Rectangle(0, 0, 1, 1), color);
        }

        public static void Unload()
        {
            Texture2D old = texture;
            texture = null;
            if (old != null) Main.QueueMainThreadAction(() => { if (!old.IsDisposed) old.Dispose(); });
        }
    }
}
