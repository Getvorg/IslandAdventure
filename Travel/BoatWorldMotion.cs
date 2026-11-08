using System;
using Microsoft.Xna.Framework;

namespace IslandAdventure.Travel
{
    public static class BoatWorldMotion
    {
        public static Vector2 Position(Vector2 dock, bool facesLeft, bool departing,
            float progress, float worldWidth)
        {
            float space = facesLeft ? dock.X - 128 : worldWidth - 128 - dock.X;
            float distance = Math.Clamp(space, 0, 320);
            float p = Math.Clamp(progress, 0, 1);
            p = p * p * (3 - 2 * p);
            float offset = distance * (departing ? p : 1 - p);
            return dock + new Vector2(facesLeft ? -offset : offset, 0);
        }

        public static Vector2 Camera(Vector2 boat, Vector2 landing, Vector2 viewport, Vector2 world, float zoom = 1)
        {
            zoom = float.IsFinite(zoom) && zoom > 0 ? zoom : 1;
            Vector2 visible = viewport / zoom;
            float skySpace = Math.Min(100, visible.Y * 0.18f);
            Vector2 focus = Vector2.Lerp(landing, boat, 0.85f) + new Vector2(0, -skySpace);
            // Keep the wider hull inside the view even at a high zoom.
            float horizontalRoom = Math.Max(0, visible.X / 2 - BoatPixelArt.Width / 2f - 16);
            focus.X = Math.Clamp(focus.X, boat.X - horizontalRoom, boat.X + horizontalRoom);
            Vector2 topLeft = focus - visible / 2;
            Vector2 visibleTopLeft = new Vector2(Math.Clamp(topLeft.X, 16, Math.Max(16, world.X - visible.X - 16)),
                Math.Clamp(topLeft.Y, 16, Math.Max(16, world.Y - visible.Y - 16)));
            // GameViewMatrix scales around the viewport center, not its origin.
            return visibleTopLeft - (viewport - visible) / 2;
        }
    }
}
