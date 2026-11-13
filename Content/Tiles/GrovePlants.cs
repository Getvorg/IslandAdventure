using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace IslandAdventure.Content.Tiles
{
    public class GroveGrass : ModTile
    {
        // Actual dirt pixels remain the installed vanilla dirt; only the rim is new.
        public override string Texture => $"Terraria/Images/Tiles_{TileID.Dirt}";
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type][TileID.Dirt] = Main.tileMerge[TileID.Dirt][Type] = true;
            DustType = DustID.Grass; HitSound = SoundID.Dig;
            RegisterItemDrop(ItemID.DirtBlock); VanillaFallbackOnModDeletion = TileID.Grass;
            AddMapEntry(new Color(62, 78, 35), CreateMapEntryName());
        }
        public override void PostDraw(int i, int j, SpriteBatch batch)
        {
            if (WorldGen.SolidTile(i,j-1)) return;
            var tile = Main.tile[i,j];
            if (tile.Slope is SlopeType.SlopeUpLeft or SlopeType.SlopeUpRight) return;
            var texture = ModContent.Request<Texture2D>("IslandAdventure/Assets/Grove/GrassEdge", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 point = new Vector2(i*16,j*16) - Main.screenPosition;
            if (!Main.drawToScreen) point += new Vector2(Main.offScreenRange);
            int frame = (i % 3) * 18;
            Color light = Lighting.GetColor(i,j);
            // Pixel-column clipping follows vanilla hammered surfaces exactly.
            for (int x=0; x<16; x++)
            {
                int top = tile.IsHalfBlock ? 8 : tile.Slope == SlopeType.SlopeDownRight ? 15-x
                    : tile.Slope == SlopeType.SlopeDownLeft ? x : 0;
                int height = System.Math.Min(8,16-top);
                batch.Draw(texture,point+new Vector2(x,top),new Rectangle(frame+x,0,1,height),light);
            }
        }
    }
    public abstract class GrovePlant : ModTile
    {
        protected virtual bool Small => false;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = Main.tileCut[Type] = Main.tileNoFail[Type] = Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(Small ? TileObjectData.Style1x1 : TileObjectData.Style2x2);
            TileObjectData.newTile.AnchorValidTiles = new[] { TileID.Dirt, TileID.Grass, ModContent.TileType<GroveGrass>() };
            if (Small) { TileObjectData.newTile.StyleHorizontal = true; TileObjectData.newTile.RandomStyleRange = 3; }
            TileObjectData.addTile(Type);
            DustType = DustID.Grass; HitSound = SoundID.Grass;
            AddMapEntry(new Color(62,78,35),CreateMapEntryName());
            VanillaFallbackOnModDeletion = TileID.Plants;
        }
    }
    public class GroveBush : GrovePlant { public override string Texture => "IslandAdventure/Assets/Grove/bush"; }
    public class GroveFern : GrovePlant { public override string Texture => "IslandAdventure/Assets/Grove/fern"; }
    public class GroveTuft : GrovePlant
    {
        public override string Texture => "IslandAdventure/Assets/Grove/Tuft";
        protected override bool Small => true;
    }
}
