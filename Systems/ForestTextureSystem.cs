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
    // Visual variants of the game's installed textures, not new tile/item IDs.
    public class ForestTextureSystem : ModSystem
    {
        private sealed record Variant(Asset<Texture2D>[] Slots, int Index, Asset<Texture2D> Original, Asset<Texture2D> Forest);
        private readonly List<Variant> variants = new();
        private bool active, failed;

        private static Color[] ReadPixels(Texture2D source)
        {
            var pixels = new Color[source.Width * source.Height];
            if (source.Format == SurfaceFormat.Color) { source.GetData(pixels); return pixels; }
            // XNB/resource-pack textures may be GPU-compressed. Read a color
            // render target rather than treating compressed bytes as RGBA.
            var device = Main.instance.GraphicsDevice;
            var targets = device.GetRenderTargets(); var viewport = device.Viewport;
            using var target = new RenderTarget2D(device, source.Width, source.Height, false, SurfaceFormat.Color, DepthFormat.None);
            try
            {
                device.SetRenderTarget(target); device.Clear(Color.Transparent);
                using var batch = new SpriteBatch(device);
                batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                batch.Draw(source, Vector2.Zero, Color.White); batch.End();
            }
            finally { device.SetRenderTargets(targets); device.Viewport = viewport; }
            target.GetData(pixels);
            return pixels;
        }
        private void Add(Asset<Texture2D>[] slots, int index, ForestMaterial material)
        {
            if (index >= slots.Length || slots[index] == null) return;
            Asset<Texture2D> original = slots[index];
            Texture2D source = Main.Assets.Request<Texture2D>(original.Name, AssetRequestMode.ImmediateLoad).Value;
            Color[] pixels = ReadPixels(source);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = ForestPalette.Recolor(pixels[i], material);
            using var output = new Texture2D(Main.instance.GraphicsDevice, source.Width, source.Height);
            output.SetData(pixels);
            using var stream = new MemoryStream();
            output.SaveAsPng(stream, source.Width, source.Height); stream.Position = 0;
            var asset = Main.Assets.CreateUntracked<Texture2D>(stream, ".png", AssetRequestMode.ImmediateLoad);
            variants.Add(new Variant(slots, index, original, asset));
        }
        // Native forest foliage variants; palm crowns (15 and 21), mushroom
        // caps and other biomes retain their installed textures.
        public static bool IsForestCanopy(int index) => index == 0 || index >= 6 && index <= 10;
        private void Prepare()
        {
            foreach (int id in new[] { TileID.Grass, TileID.Plants, TileID.Plants2, TileID.Vines, TileID.Trees, TileID.Dirt, TileID.Stone })
                Main.instance.LoadTiles(id);
            Add(TextureAssets.Tile, TileID.Grass, ForestMaterial.Grass);
            Add(TextureAssets.Tile, TileID.Plants, ForestMaterial.Plants);
            Add(TextureAssets.Tile, TileID.Plants2, ForestMaterial.Plants);
            Add(TextureAssets.Tile, TileID.Vines, ForestMaterial.Leaves);
            Add(TextureAssets.Tile, TileID.Trees, ForestMaterial.Bark);
            Add(TextureAssets.Tile, TileID.Dirt, ForestMaterial.Soil);
            Add(TextureAssets.Tile, TileID.Stone, ForestMaterial.Stone);
            Add(TextureAssets.Wood, 0, ForestMaterial.Bark);
            for (int i = 0; i < TextureAssets.TreeTop.Length; i++)
                if (IsForestCanopy(i)) Add(TextureAssets.TreeTop, i, ForestMaterial.Leaves);
            for (int i = 0; i < TextureAssets.TreeBranch.Length; i++)
                if (IsForestCanopy(i)) Add(TextureAssets.TreeBranch, i, ForestMaterial.Leaves);
            foreach (int id in new[] { WallID.DirtUnsafe, WallID.DirtUnsafe1, WallID.DirtUnsafe2, WallID.DirtUnsafe3, WallID.DirtUnsafe4,
                WallID.GrassUnsafe, WallID.FlowerUnsafe, WallID.Stone, WallID.RocksUnsafe1, WallID.RocksUnsafe2, WallID.RocksUnsafe3, WallID.RocksUnsafe4 })
            {
                Main.instance.LoadWall(id);
                Add(TextureAssets.Wall, id, id is WallID.Stone or WallID.RocksUnsafe1 or WallID.RocksUnsafe2 or WallID.RocksUnsafe3 or WallID.RocksUnsafe4
                    ? ForestMaterial.Stone : ForestMaterial.Soil);
            }
        }
        private void Restore()
        {
            if (!active) return;
            foreach (var variant in variants) variant.Slots[variant.Index] = variant.Original;
            active = false;
            Main.instance.TilePaintSystem.Reset();
        }
        public override void PostUpdateEverything()
        {
            if (Main.dedServ) return;
            bool wanted = !failed && !Main.gameMenu && SubworldSystem.IsActive<IslandSubworld>();
            if (wanted == active) return;
            if (!wanted) { Restore(); return; }
            if (variants.Count == 0)
            {
                try { Prepare(); }
                catch (Exception error)
                {
                    foreach (var variant in variants) variant.Forest.Value.Dispose(); variants.Clear(); failed = true;
                    Mod.Logger.Error("Forest texture preparation failed; keeping original Terraria textures.", error);
                    return;
                }
            }
            foreach (var variant in variants) variant.Slots[variant.Index] = variant.Forest;
            active = true;
            Main.instance.TilePaintSystem.Reset();
        }
        public override void OnWorldUnload()
        {
            // SL can unload worlds from a worker thread. All GPU work is queued.
            if (!Main.dedServ) Main.QueueMainThreadAction(Restore);
        }
        public override void Unload()
        {
            if (Main.dedServ) return;
            Main.QueueMainThreadAction(() => {
                Restore();
                foreach (var variant in variants) variant.Forest.Value.Dispose();
                variants.Clear();
            });
        }
    }
}
