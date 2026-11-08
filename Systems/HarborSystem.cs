using System;
using Microsoft.Xna.Framework;
using IslandAdventure.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace IslandAdventure.Systems
{
    public class HarborSystem : ModSystem
    {
        public Point Landing { get; private set; }
        public Point Boat { get; private set; }
        public bool FacesLeft { get; private set; }
        private bool searched;
        public bool HasHarbor => Landing.X > 20 && Landing.X < Main.maxTilesX - 20
            && Landing.Y > 20 && Landing.Y < Main.maxTilesY - 20
            && Boat.X > 10 && Boat.X < Main.maxTilesX - 10 && Boat.Y > 20 && Boat.Y < Main.maxTilesY - 20;
        public Vector2 BoatPosition => new Vector2(Boat.X * 16 + 8 + (FacesLeft ? -32 : 32), Boat.Y * 16);

        public void SetHarbor(Point landing, Point boat, bool facesLeft)
        {
            Landing = landing;
            Boat = boat;
            FacesLeft = facesLeft;
            searched = true;
        }

        public override void OnWorldLoad() { Landing = Boat = Point.Zero; searched = false; }
        public override void OnWorldUnload() { Landing = Boat = Point.Zero; searched = false; }
        public override void SaveWorldData(TagCompound tag)
        {
            if (!HasHarbor) return;
            tag["landingX"] = Landing.X; tag["landingY"] = Landing.Y;
            tag["boatX"] = Boat.X; tag["boatY"] = Boat.Y;
            tag["facesLeft"] = FacesLeft;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("landingX"))
                SetHarbor(new Point(tag.GetInt("landingX"), tag.GetInt("landingY")),
                    new Point(tag.GetInt("boatX"), tag.GetInt("boatY")), tag.GetBool("facesLeft"));
        }

        public override void PostUpdateWorld()
        {
            if (!Main.dedServ && !searched && !Main.gameMenu)
                DiscoverExistingPier();
        }

        public bool DiscoverExistingPier()
        {
            if (HasHarbor) return true;
            if (searched) return false;
            searched = true;
            if (SubworldSystem.AnyActive() && !SubworldSystem.IsActive<IslandSubworld>()) return false;
            bool leftSide = !SubworldSystem.IsActive<IslandSubworld>();
            int first = leftSide ? 60 : Main.maxTilesX - 750;
            int last = leftSide ? 750 : Main.maxTilesX - 60;
            int bottom = Math.Min(Main.maxTilesY - 220, (int)Main.worldSurface + 80);
            for (int y = 60; y < bottom; y++)
                for (int x = first; x < last; x++)
                {
                    int start = x;
                    while (x < last && IsDeck(x, y)) x++;
                    if (x - start < 24) continue;
                    int seaEnd = leftSide ? start : x - 1;
                    int boatX = seaEnd + (leftSide ? -5 : 5);
                    for (int waterY = y + 1; waterY < Math.Min(y + 35, bottom); waterY++)
                    {
                        Tile water = Main.tile[boatX, waterY];
                        if (water.LiquidType != LiquidID.Water || water.LiquidAmount < 127) continue;
                        // Prefer a clear deck tile rather than a room joined to it.
                        for (int offset = x - start - 5; offset >= 5; offset--)
                        {
                            int candidate = leftSide ? start + offset : x - 1 - offset;
                            if (Main.tile[candidate, y - 1].WallType != WallID.WoodenFence) continue;
                            SetHarbor(new Point(candidate, y), new Point(boatX, waterY), leftSide);
                            if (TryLanding(Main.LocalPlayer, out _)) return true;
                            Landing = Boat = Point.Zero;
                        }
                    }
                }
            return false;
        }

        private static bool IsDeck(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && (tile.TileType == TileID.PalmWood
                || tile.TileType == TileID.BorealWood || tile.TileType == TileID.RichMahogany);
        }

        public bool TryLanding(Player player, out Vector2 position)
        {
            position = player.position;
            if (!HasHarbor) return false;
            for (int radius = 0; radius <= 24; radius++)
                foreach (int sign in new[] { -1, 1 })
                {
                    int x = Landing.X + radius * sign;
                    int y = Landing.Y;
                    if (x < 10 || x > Main.maxTilesX - 10 || !WorldGen.SolidTile(x, y)) continue;
                    Vector2 candidate = new Vector2(x * 16 + 8 - player.width / 2f, y * 16 - player.height);
                    if (Main.tile[x, y - 1].LiquidAmount > 127
                        || Collision.SolidCollision(candidate, player.width, player.height)) continue;
                    position = candidate;
                    return true;
                }
            return false;
        }
    }
}
