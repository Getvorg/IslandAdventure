using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Tiles;

namespace IslandAdventure.Content.Trees
{
    // Native Trees on native Grass in the gallery use the regional renderer.
    // This registration also supports legacy grove soil in saved worlds.
    public class AncientCamphor : ModTree
    {
        public const int CrownWidth = 80, CrownHeight = 80;
        public const int BranchWidth = 40, BranchHeight = 40;
        public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings();
        public override void SetStaticDefaults() => GrowsOnTileId = new[] { ModContent.TileType<GroveGrass>() };
        public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Camphor");
        public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Camphor_Tops");
        public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/Camphor_Branches");
        public override void SetTreeFoliageSettings(int i,int j,Tile tile,int xoffset,ref int treeFrame,int floorY,ref int width,ref int height)
        { treeFrame=System.Math.Clamp(treeFrame,0,2); width=CrownWidth; height=CrownHeight; }
        public override int DropWood() => ItemID.Wood;
    }
}
