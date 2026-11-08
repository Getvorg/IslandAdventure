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
    // A sound front rather than a damaging projectile: immunity frames do not
    // swallow the debuff, and each player can only be affected once per song.
    public class RoyalSong : ModProjectile
    {
        public const int ConfusionTicks = 5 * 60;
        public const int Duration = 100;
        private readonly bool[] affected = new bool[Main.maxPlayers];
        public float Radius => 12 + (Duration - Projectile.timeLeft) * 9f;
        public override string Texture => "IslandAdventure/Assets/Projectiles/RoyalWave";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.tileCollide = false; Projectile.ignoreWater = true;
            Projectile.timeLeft = Duration; Projectile.penetrate = -1;
        }
        public override bool? CanDamage() => false;
        public bool Touches(Rectangle target)
        {
            Vector2 origin = Projectile.Center;
            float nearest = Vector2.Distance(origin, new Vector2(
                MathHelper.Clamp(origin.X, target.Left, target.Right),
                MathHelper.Clamp(origin.Y, target.Top, target.Bottom)));
            float farthestX = Math.Max(Math.Abs(origin.X - target.Left), Math.Abs(origin.X - target.Right));
            float farthestY = Math.Max(Math.Abs(origin.Y - target.Top), Math.Abs(origin.Y - target.Bottom));
            return nearest <= Radius + 12 && farthestX * farthestX + farthestY * farthestY >= (Radius - 12) * (Radius - 12);
        }
        public override void AI()
        {
            int owner = (int)Projectile.ai[0] - 1;
            if (owner < 0 || owner >= Main.maxNPCs || !Main.npc[owner].active
                || Main.npc[owner].type != ModContent.NPCType<MermaidQueen>())
            { Projectile.Kill(); return; }
            Projectile.Opacity = Math.Min(1, Projectile.timeLeft / 20f);
            if (Main.netMode == NetmodeID.MultiplayerClient || BoatTravelSystem.IsCinematic) return;
            foreach (Player player in Main.player)
                if (player.active && !player.dead && !player.ghost && !affected[player.whoAmI] && Touches(player.Hitbox))
                {
                    affected[player.whoAmI] = true;
                    player.AddBuff(BuffID.Confused, ConfusionTicks);
                }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float radius = Radius;
            int segments = Math.Max(32, (int)(MathHelper.TwoPi * radius / 6));
            Vector2 center = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 point = center + direction * radius;
                point = new Vector2((int)(point.X / 2) * 2, (int)(point.Y / 2) * 2);
                Main.spriteBatch.Draw(pixel, point, new Rectangle(0, 0, 1, 1),
                    new Color(255, 172, 222) * Projectile.Opacity, 0, Vector2.Zero, new Vector2(4), SpriteEffects.None, 0);
                if (i % 2 == 0)
                    Main.spriteBatch.Draw(pixel, center + direction * (radius - 8), new Rectangle(0, 0, 1, 1),
                        new Color(255, 235, 248) * (Projectile.Opacity * 0.65f), 0, Vector2.Zero, new Vector2(2), SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
