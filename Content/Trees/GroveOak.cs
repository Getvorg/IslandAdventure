using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;
namespace IslandAdventure.Content.Trees
{
    public class GroveOak : ModTree
    {
        public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings();
        public override void SetStaticDefaults() => GrowsOnTileId = new[] { ModContent.TileType<GroveGrass>() };
        public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree");
        public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree_Tops");
        public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Tree_Branches");
        public override void SetTreeFoliageSettings(int i,int j,Tile tile,int xoffset,ref int treeFrame,int floorY,ref int width,ref int height)
        { treeFrame=0;width=192;height=128; }
        public override int DropWood() => ItemID.Wood;
    }
}
