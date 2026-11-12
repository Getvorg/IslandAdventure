using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;
using IslandAdventure.Content.Items;
namespace IslandAdventure.Content.Trees
{
    public class DarkForestTree : ModTree
    {
        public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings();
        public override void SetStaticDefaults() => GrowsOnTileId = System.Array.Empty<int>();
        public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Trees/DarkTree");
        public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Trees/DarkTree_Tops");
        public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Trees/DarkTree_Branches");
        public override int DropWood() => Terraria.ID.ItemID.Wood;
    }
}
