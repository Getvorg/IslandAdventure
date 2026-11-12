using System;
using System.Collections.Generic;
using System.IO;
using IslandAdventure.Subworlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace IslandAdventure.Systems
{
    // Client-only visual variants. The world always retains vanilla tile IDs,
    // collisions, slopes, framing, item drops and tree growth behavior.
    public class DarkForestTextures : ModSystem
    {
        private sealed record Swap(Asset<Texture2D>[] Slots, int Index, Asset<Texture2D> Original, Asset<Texture2D> Dark);
        private readonly List<Swap> swaps = new();
        private bool active, failed;
        public static Color Shade(Color c, Vector3 factors) => new Color(
            (byte)(c.R * factors.X), (byte)(c.G * factors.Y), (byte)(c.B * factors.Z), c.A);
        private void Add(Asset<Texture2D>[] slots, int id, Vector3 shade, bool branches = false)
        {
            if (id >= slots.Length || slots[id] == null) return;
            var original = slots[id];
            Texture2D source = Main.Assets.Request<Texture2D>(original.Name, AssetRequestMode.ImmediateLoad).Value;
            var colors = new Color[source.Width * source.Height]; source.GetData(colors);
            for (int i = 0; i < colors.Length; i++) colors[i] = Shade(colors[i], shade);
            if (branches && source.Width == 84 && source.Height == 126)
            {
                // Preserve the EXACT vanilla alpha and attachment edges. Reuse the
                // new leaf detail only inside that silhouette, preventing gaps.
                var art = ModContent.Request<Texture2D>("IslandAdventure/Assets/Trees/DarkTree_Branches", AssetRequestMode.ImmediateLoad).Value;
                if (art.Width == source.Width && art.Height == source.Height)
                {
                    var detail = new Color[colors.Length]; art.GetData(detail);
                    ApplyBranchDetail(colors, detail, source.Width);
                }
            }
            using var texture = new Texture2D(Main.instance.GraphicsDevice, source.Width, source.Height);
            texture.SetData(colors);
            using var stream = new MemoryStream(); texture.SaveAsPng(stream, source.Width, source.Height); stream.Position = 0;
            var asset = Main.Assets.CreateUntracked<Texture2D>(stream, ".png", AssetRequestMode.ImmediateLoad);
            swaps.Add(new Swap(slots, id, original, asset));
        }
        public static void ApplyBranchDetail(Color[] colors, Color[] detail, int width)
        {
            if (width != 84 || colors.Length != detail.Length) return;
            for (int i = 0; i < colors.Length; i++)
            {
                int x = i % width % 42;
                if (x >= 6 && x < 34 && colors[i].A > 0 && detail[i].A > 0)
                    colors[i] = Color.FromNonPremultiplied(detail[i].R, detail[i].G, detail[i].B, colors[i].A);
            }
        }
        private void Prepare()
        {
            var foliage = new Vector3(.48f, .43f, .35f);
            foreach (int id in new[] { TileID.Grass, TileID.Plants, TileID.Plants2, TileID.Vines })
            {
                Main.instance.LoadTiles(id);
                Add(TextureAssets.Tile, id, foliage);
            }
            Main.instance.LoadTiles(TileID.Trees); Main.instance.LoadTiles(TileID.Dirt); Main.instance.LoadTiles(TileID.Stone);
            Add(TextureAssets.Tile, TileID.Trees, new Vector3(.67f,.48f,.34f));
            Add(TextureAssets.Tile, TileID.Dirt, new Vector3(.78f,.70f,.62f));
            Add(TextureAssets.Tile, TileID.Stone, new Vector3(.75f,.75f,.80f));
            for (int i = 0; i < TextureAssets.Wood.Length; i++) Add(TextureAssets.Wood, i, new Vector3(.67f,.48f,.34f));
            for (int i = 0; i < TextureAssets.TreeTop.Length; i++) Add(TextureAssets.TreeTop, i, foliage);
            for (int i = 0; i < TextureAssets.TreeBranch.Length; i++) Add(TextureAssets.TreeBranch, i, foliage, true);
            foreach (int id in new[] { WallID.DirtUnsafe, WallID.DirtUnsafe1, WallID.DirtUnsafe2, WallID.DirtUnsafe3, WallID.DirtUnsafe4,
                WallID.GrassUnsafe, WallID.FlowerUnsafe, WallID.Stone, WallID.RocksUnsafe1, WallID.RocksUnsafe2, WallID.RocksUnsafe3, WallID.RocksUnsafe4 })
            {
                Main.instance.LoadWall(id);
                Add(TextureAssets.Wall, id, new Vector3(.72f,.68f,.62f));
            }
        }
        public override void PostUpdateEverything()
        {
            if (Main.dedServ) return;
            bool wanted = !failed && !Main.gameMenu && SubworldSystem.IsActive<IslandSubworld>();
            if (wanted == active) return;
            if (wanted && swaps.Count == 0)
            {
                try { Prepare(); }
                catch (Exception error)
                {
                    foreach (var swap in swaps) swap.Dark.Value.Dispose(); swaps.Clear(); failed = true;
                    Mod.Logger.Error("Could not prepare dark forest visual textures; keeping vanilla textures.", error);
                    return;
                }
            }
            foreach (var swap in swaps) swap.Slots[swap.Index] = wanted ? swap.Dark : swap.Original;
            active = wanted;
            Main.instance.TilePaintSystem.Reset();
        }
        public override void OnWorldUnload()
        {
            if (Main.dedServ || !active) return;
            Main.QueueMainThreadAction(() => {
                foreach (var swap in swaps) swap.Slots[swap.Index] = swap.Original;
                active = false; Main.instance.TilePaintSystem.Reset();
            });
        }
        public override void Unload()
        {
            if (Main.dedServ) return;
            // Texture operations belong to the graphics thread, also during reload.
            Main.QueueMainThreadAction(() => {
                foreach (var swap in swaps) { swap.Slots[swap.Index] = swap.Original; swap.Dark.Value.Dispose(); }
                swaps.Clear(); active = false;
            });
        }
    }
}
