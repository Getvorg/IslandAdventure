using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.NPCs.Bosses
{
    public class RoyalMoray : ModNPC
    {
        public override string Texture => "IslandAdventure/Assets/NPCs/RoyalMoray";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
        }
        public override void SetDefaults()
        {
            NPC.width = 76; NPC.height = 30;
            NPC.lifeMax = 70; NPC.damage = 18; NPC.defense = 4;
            NPC.knockBackResist = 0.7f;
            NPC.noGravity = true; NPC.noTileCollide = true;
            NPC.aiStyle = -1; NPC.value = 0; NPC.dontCountMe = true;
            NPC.HitSound = SoundID.NPCHit1; NPC.DeathSound = SoundID.NPCDeath1;
            NPC.alpha = 255;
        }
        public override bool CheckActive() => false;
        public override void AI()
        {
            int owner = (int)NPC.ai[0] - 1;
            NPC.ai[1]++;
            if (owner < 0 || owner >= Main.maxNPCs || !Main.npc[owner].active
                || Main.npc[owner].type != ModContent.NPCType<MermaidQueen>() || NPC.ai[1] > 900)
            {
                NPC.active = false; NPC.netUpdate = true;
                if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                return;
            }
            NPC.Opacity = MathHelper.Clamp(NPC.ai[1] / 20, 0, 1);
            NPC.TargetClosest(false);
            if (!NPC.HasValidTarget) { NPC.velocity *= 0.95f; return; }
            Player player = Main.player[NPC.target];
            float phase = NPC.ai[1] % 120;
            Vector2 point = player.Center + new Vector2((float)Math.Sin(NPC.ai[1] / 24 + NPC.ai[2]) * 90, 70);
            point.Y = Math.Max(NPC.ai[3] - 45, point.Y);
            if (phase < 75)
            {
                Vector2 desired = (point - NPC.Center).SafeNormalize(Vector2.UnitX) * 3.1f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.07f);
            }
            else if (phase < 90) NPC.velocity *= 0.88f;
            else if (phase == 90 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 7;
                NPC.netUpdate = true;
            }
            else NPC.velocity *= 0.985f;
            if (NPC.Center.Y < NPC.ai[3] - 100) NPC.velocity.Y = Math.Max(1, NPC.velocity.Y);
            NPC.rotation = NPC.velocity.LengthSquared() > 0.1f ? NPC.velocity.ToRotation() : NPC.rotation;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.18;
            NPC.frame.Y = (int)NPC.frameCounter % 6 * frameHeight;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
            => !BoatTravelSystem.IsCinematic && NPC.ai[1] > 20;
        public override bool PreDraw(SpriteBatch batch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            batch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor * NPC.Opacity, NPC.rotation,
                new Vector2(texture.Width / 2f, texture.Height / 6f / 2), NPC.scale,
                NPC.velocity.X < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
            return false;
        }
    }
}
