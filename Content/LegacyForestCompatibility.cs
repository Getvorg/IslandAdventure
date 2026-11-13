// Compatibility IDs for 0.3.2 saves ONLY. ForestMaterialMigration converts
// these to vanilla materials. Do not remove until old saves are migrated.
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace IslandAdventure.Content.Tiles
{
    public abstract class LegacyForestTile : ModTile
    {
        protected abstract int Vanilla { get; }
        protected virtual int Drop => ItemID.DirtBlock;
        public override string Texture => $"Terraria/Images/Tiles_{Vanilla}";
        public override void SetStaticDefaults()
        {
            bool plant = Vanilla == TileID.Plants || Vanilla == TileID.Vines;
            Main.tileSolid[Type] = Main.tileBlockLight[Type] = !plant;
            Main.tileCut[Type] = plant;
            Main.tileFrameImportant[Type] = Vanilla == TileID.Plants;
            DustType = DustID.Dirt; HitSound = SoundID.Dig;
            AddMapEntry(new Color(80, 80, 80), CreateMapEntryName());
            VanillaFallbackOnModDeletion = (ushort)Vanilla;
            if (!plant) RegisterItemDrop(Drop);
        }
    }
    public class DarkSoilTile : LegacyForestTile { protected override int Vanilla => TileID.Dirt; }
    public class DarkGrassTile : LegacyForestTile { protected override int Vanilla => TileID.Grass; }
    public class DarkStoneTile : LegacyForestTile { protected override int Vanilla => TileID.Stone; protected override int Drop => ItemID.StoneBlock; }
    public class DarkWoodTile : LegacyForestTile { protected override int Vanilla => TileID.WoodBlock; protected override int Drop => ItemID.Wood; }
    public class DarkPlantTile : LegacyForestTile { protected override int Vanilla => TileID.Plants; }
    public class DarkVineTile : LegacyForestTile { protected override int Vanilla => TileID.Vines; }
}
namespace IslandAdventure.Content.Items
{
    public abstract class LegacyForestItem : ModItem
    {
        protected abstract int VanillaItem { get; }
        protected abstract int VanillaTile { get; }
        public override string Texture => $"Terraria/Images/Item_{VanillaItem}";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(VanillaTile);
            Item.width = Item.height = 16; Item.maxStack = 9999;
        }
    }
    public class DarkSoil : LegacyForestItem { protected override int VanillaItem => ItemID.DirtBlock; protected override int VanillaTile => TileID.Dirt; }
    public class DarkStone : LegacyForestItem { protected override int VanillaItem => ItemID.StoneBlock; protected override int VanillaTile => TileID.Stone; }
    public class DarkWood : LegacyForestItem { protected override int VanillaItem => ItemID.Wood; protected override int VanillaTile => TileID.WoodBlock; }
}
namespace IslandAdventure.Content.Walls
{
    public abstract class LegacyForestWall : ModWall
    {
        protected abstract int Vanilla { get; }
        public override string Texture => $"Terraria/Images/Wall_{Vanilla}";
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = false; AddMapEntry(new Color(50, 50, 50));
            VanillaFallbackOnModDeletion = (ushort)Vanilla;
        }
    }
    public class DarkSoilWall : LegacyForestWall { protected override int Vanilla => WallID.DirtUnsafe; }
    public class DarkStoneWall : LegacyForestWall { protected override int Vanilla => WallID.Stone; }
}
