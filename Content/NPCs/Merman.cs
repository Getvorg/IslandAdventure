using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace IslandAdventure.Content.NPCs
{
    // Shares the established swimming behaviour, but has independent art/assets.
    public class Merman : Mermaid
    {
        public const int BodyWidth = 42, BodyHeight = 50;
        public new const string RedTexturePath = "IslandAdventure/Assets/NPCs/Merman_Red";
        private static Asset<Texture2D> redMermanTexture;
        public override string Texture => "IslandAdventure/Assets/NPCs/Merman";

        public override void Load()
        {
            if (!Main.dedServ) redMermanTexture = ModContent.Request<Texture2D>(RedTexturePath);
        }
        public override void Unload() => redMermanTexture = null;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = BodyWidth;
            NPC.height = BodyHeight;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.Add(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean);
            bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.IslandAdventure.NPCs.Merman.Bestiary"));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = IsRed ? redMermanTexture.Value : TextureAssets.Npc[Type].Value;
            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation,
                new Vector2(texture.Width / 2f, texture.Height / FrameCount / 2f), NPC.scale,
                NPC.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }
    }
}
