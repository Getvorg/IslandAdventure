using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace IslandAdventure.Content.Walls
{
    public class DarkSoilWall : ModWall
    {
        public override string Texture => "IslandAdventure/Assets/Walls/DarkForest/Soil";
        public override void SetStaticDefaults() { Main.wallHouse[Type]=false; AddMapEntry(new Color(29,20,16)); VanillaFallbackOnModDeletion=WallID.DirtUnsafe; }
    }
    public class DarkStoneWall : ModWall
    {
        public override string Texture => "IslandAdventure/Assets/Walls/DarkForest/Stone";
        public override void SetStaticDefaults() { Main.wallHouse[Type]=false; AddMapEntry(new Color(25,25,29)); VanillaFallbackOnModDeletion=WallID.Stone; }
    }
}
