using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.NPCs.Bosses;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.Projectiles
{
    public class RoyalWave : ModProjectile
    {
        public override string Texture => "IslandAdventure/Assets/Projectiles/RoyalWave";
        public override void SetDefaults()
        {
            Projectile.width = 112; Projectile.height = 96;
            Projectile.hostile = true; Projectile.friendly = false;
            Projectile.tileCollide = false; Projectile.ignoreWater = true;
            Projectile.penetrate = -1; Projectile.timeLeft = 180;
        }
        public override void AI()
        {
            int owner = (int)Projectile.ai[0] - 1;
            if (owner < 0 || owner >= Main.maxNPCs || !Main.npc[owner].active
                || Main.npc[owner].type != ModContent.NPCType<MermaidQueen>())
            { Projectile.Kill(); return; }
            Vector2 center = Projectile.Center;
            Projectile.scale = 1 + MathHelper.Clamp(Projectile.ai[2], 0, 2) * 0.5f;
            // The hitbox grows with the visible wave; every size stays at the sea surface.
            Projectile.width = (int)(112 * Projectile.scale);
            Projectile.height = (int)(96 * Projectile.scale);
            Projectile.Center = new Vector2(center.X, Projectile.ai[1] + 16 - Projectile.height / 2f
                + (float)Math.Sin(Projectile.timeLeft * 0.2f) * 2);
            Projectile.Opacity = Math.Min(1, Projectile.timeLeft / 20f) * 0.9f;
            if (Projectile.position.X < 16 || Projectile.position.X > Main.maxTilesX * 16 - 144)
                Projectile.Kill();
        }
        public override bool? CanDamage() => BoatTravelSystem.IsCinematic ? false : null;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
                Color.Lerp(lightColor, Color.White, 0.25f) * Projectile.Opacity, 0,
                texture.Size() / 2, Projectile.scale, Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
