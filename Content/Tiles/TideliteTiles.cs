using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.DataStructures;

namespace IslandAdventure.Content.Tiles
{
    public class TideliteOreTile : ModTile
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/TideliteOre";
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 310;
            Main.tileShine[Type] = 900;
            TileID.Sets.Ore[Type] = true;
            TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
            DustType = DustID.GemEmerald; HitSound = SoundID.Tink;
            MinPick = 45; MineResist = 2f;
            AddMapEntry(new Color(56, 170, 161), CreateMapEntryName());
            VanillaFallbackOnModDeletion = TileID.Stone;
        }
    }

    public class TideliteBrickTile : ModTile
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/TideliteBrick";
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;
            DustType = DustID.Stone; HitSound = SoundID.Tink;
            AddMapEntry(new Color(67, 110, 120), CreateMapEntryName());
            VanillaFallbackOnModDeletion = TileID.GrayBrick;
        }
    }

    public class TideliteBarTile : ModTile
    {
        public override string Texture => "IslandAdventure/Assets/Tiles/TideliteBar";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileNoAttach[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 1, 0);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            DustType = DustID.GemEmerald; HitSound = SoundID.Tink;
            AddMapEntry(new Color(87, 211, 184), CreateMapEntryName());
        }
    }
}
