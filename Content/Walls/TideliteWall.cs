using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace IslandAdventure.Content.Walls
{
    public class TideliteWall : ModWall
    {
        public override string Texture => "IslandAdventure/Assets/Walls/TideliteWall";
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = DustID.Stone;
            AddMapEntry(new Color(43, 72, 82), Language.GetText("Mods.IslandAdventure.Walls.TideliteWall.DisplayName"));
        }
    }
}
