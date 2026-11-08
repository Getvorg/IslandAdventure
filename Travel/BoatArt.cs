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
        public const int Width = BoatPixelArt.Width;
        public const int Height = BoatPixelArt.Height;
        public const int Waterline = 40;
        private static Texture2D texture;

        private static Texture2D Texture()
        {
            if (texture != null && !texture.IsDisposed) return texture;
            texture = new Texture2D(Main.instance.GraphicsDevice, Width, Height);
            texture.SetData(BoatPixelArt.CreatePixels());
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
            DrawItem(batch, ItemID.WoodenCrate, Prop(20, 24), scale, tint);
            batch.Draw(Texture(), center, null, tint, 0, new Vector2(64, Waterline), scale,
                left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            Main.instance.LoadItem(ItemID.PalmWoodLantern);
            Texture2D lantern = TextureAssets.Item[ItemID.PalmWoodLantern].Value;
            Vector2 lanternTop = Prop(118, 5);
            batch.Draw(lantern, lanternTop, null, Color.White, 0,
                new Vector2(lantern.Width / 2f, 0), scale, SpriteEffects.None, 0);
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
            foreach (Rectangle pixel in BoatGeometry.PixelGridLine(start, end, width))
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
