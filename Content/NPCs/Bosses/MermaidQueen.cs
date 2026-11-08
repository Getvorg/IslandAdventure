using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using IslandAdventure.Content.Projectiles;
using IslandAdventure.Systems;

namespace IslandAdventure.Content.NPCs.Bosses
{
    public enum QueenAttack { Hover, DashWindup, Dash, Recover, DiveWindup, Dive, Emerge, Summon, Retreat, WaveBarrage, Sing }

    [AutoloadBossHead]
    public class MermaidQueen : ModNPC
    {
        public const int BodyWidth = 132, BodyHeight = 180, FrameSize = 224;
        public const int FramesPerAnimation = 6;
        private float arenaX, diveX;
        private int lastAnimation = -1;
        public int AnimationRow => State switch {
            QueenAttack.DashWindup or QueenAttack.Dash => 1,
            QueenAttack.DiveWindup or QueenAttack.Dive or QueenAttack.WaveBarrage or QueenAttack.Emerge => 2,
            QueenAttack.Summon => 3, QueenAttack.Sing => 4, _ => 0
        };
        public QueenAttack State => (QueenAttack)(int)NPC.ai[0];
        public float SeaY => NPC.ai[3];
        private bool Authority => Main.netMode != NetmodeID.MultiplayerClient;
        private bool Fast => NPC.life < NPC.lifeMax / 2;
        public override string Texture => "IslandAdventure/Assets/NPCs/Bosses/MermaidQueen";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5 * FramesPerAnimation;
            NPCID.Sets.TrailCacheLength[Type] = 6;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers {
                Scale = 0.55f, PortraitScale = 0.6f
            });
        }

        public override void SetDefaults()
        {
            NPC.width = BodyWidth; NPC.height = BodyHeight;
            NPC.lifeMax = 4500; NPC.damage = 34; NPC.defense = 12;
            NPC.knockBackResist = 0;
            NPC.boss = true; NPC.aiStyle = -1;
            NPC.noGravity = true; NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 3);
            NPC.npcSlots = 12;
            NPC.HitSound = SoundID.NPCHit1; NPC.DeathSound = SoundID.NPCDeath1;
            Music = MusicID.Boss3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            arenaX = diveX = NPC.Center.X;
            if (NPC.ai[3] <= 0)
            {
                NPC.TargetClosest();
                if (NPC.HasValidTarget && QueenEncounter.TryFindSea(Main.player[NPC.target], out Vector2 water))
                    NPC.ai[3] = water.Y;
                else NPC.ai[3] = MathHelper.Clamp(NPC.Center.Y + 180, 200, Main.maxTilesY * 16 - 600);
            }
            NPC.netUpdate = true;
        }

        public override void SendExtraAI(BinaryWriter writer) { writer.Write(arenaX); writer.Write(diveX); }
        public override void ReceiveExtraAI(BinaryReader reader) { arenaX = reader.ReadSingle(); diveX = reader.ReadSingle(); }
        public override bool CheckActive() => false;

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry entry)
        {
            entry.Info.Add(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean);
            entry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.IslandAdventure.NPCs.MermaidQueen.Bestiary"));
        }

        private void Enter(QueenAttack next)
        {
            if (!Authority) return;
            NPC.ai[0] = (int)next; NPC.ai[1] = 0;
            NPC.netUpdate = true;
        }
        private void NextAttack()
        {
            if (!Authority) return;
            NPC.ai[2] = (NPC.ai[2] + 1) % 4;
            Enter(QueenAttack.Hover);
        }
        private void MoveTo(Vector2 point, float speed)
        {
            point.X = MathHelper.Clamp(point.X, 160, Main.maxTilesX * 16 - 160);
            point.Y = MathHelper.Clamp(point.Y, 140, Main.maxTilesY * 16 - 400);
            Vector2 delta = point - NPC.Center;
            Vector2 desired = delta.SafeNormalize(Vector2.Zero) * Math.Min(speed, delta.Length() / 12);
            NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.09f);
        }
        private void Cue(int dustCount)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < dustCount; i++)
            {
                var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard,
                    0, -1, 100, new Color(255, 190, 222), 1.2f);
                dust.noGravity = true;
            }
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            Player player = NPC.HasValidTarget ? Main.player[NPC.target] : null;
            if (State != QueenAttack.Retreat && (player == null || player.dead
                || Vector2.DistanceSquared(player.Center, new Vector2(arenaX, SeaY)) > 2400 * 2400))
                Enter(QueenAttack.Retreat);
            NPC.ai[1]++;
            float timer = NPC.ai[1];
            if (State == QueenAttack.Retreat)
            {
                NPC.damage = 0; NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(0, -6), 0.08f);
                NPC.Opacity = Math.Max(0, 1 - timer / 70);
                if (timer >= 70 && Authority) { ClearSummons(); NPC.active = false; NPC.netUpdate = true; }
                return;
            }
            if (player == null) return;
            NPC.dontTakeDamage = false;
            NPC.direction = player.Center.X >= NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = MathHelper.Clamp(NPC.velocity.Y * NPC.direction * 0.025f, -0.35f, 0.35f);
            float hoverY = Math.Min(player.Center.Y - 170, SeaY - 170);

            switch (State)
            {
                case QueenAttack.Hover:
                    MoveTo(new Vector2(player.Center.X - NPC.direction * 260, hoverY), 6);
                    if (timer >= (Fast ? 55 : 80))
                    {
                        switch ((int)NPC.ai[2] % 4)
                        {
                            case 0: Enter(QueenAttack.DashWindup); break;
                            case 1:
                                if (Authority) diveX = QueenEncounter.TryFindSea(player, out Vector2 water) ? water.X : arenaX;
                                Enter(QueenAttack.DiveWindup); break;
                            case 2: Enter(QueenAttack.Summon); break;
                            default: Enter(QueenAttack.Sing); break;
                        }
                    }
                    break;
                case QueenAttack.DashWindup:
                    if (timer < 30) MoveTo(new Vector2(player.Center.X - NPC.direction * 270, hoverY + 40), 5);
                    else NPC.velocity *= 0.9f;
                    if ((int)timer % 8 == 0) Cue(3);
                    if (timer >= (Fast ? 50 : 65) && Authority)
                    {
                        Vector2 aim = player.Center + player.velocity * 12 - NPC.Center;
                        NPC.velocity = aim.SafeNormalize(Vector2.UnitX * NPC.direction) * (Fast ? 16 : 13);
                        Enter(QueenAttack.Dash);
                    }
                    break;
                case QueenAttack.Dash:
                    if (timer == 1 && !Main.dedServ) SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                    if ((int)timer % 3 == 0) Cue(1);
                    if (timer >= 26) Enter(QueenAttack.Recover);
                    break;
                case QueenAttack.Recover:
                    NPC.velocity *= 0.93f;
                    if (timer >= 40) NextAttack();
                    break;
                case QueenAttack.DiveWindup:
                    MoveTo(new Vector2(diveX, SeaY - 180), 8);
                    if ((int)timer % 8 == 0) Cue(2);
                    if (timer >= 65 && Authority) { NPC.velocity = new Vector2(0, 15); Enter(QueenAttack.Dive); }
                    break;
                case QueenAttack.Dive:
                    NPC.velocity.X *= 0.85f; NPC.velocity.Y = 15;
                    NPC.rotation = NPC.direction * 0.6f;
                    if ((NPC.Center.Y >= SeaY + 115 || timer >= 50) && Authority)
                    { Enter(QueenAttack.WaveBarrage); NPC.velocity = Vector2.Zero; }
                    break;
                case QueenAttack.WaveBarrage:
                    // Keep the source fixed so all three pairs are evenly spaced.
                    NPC.velocity = Vector2.Zero;
                    if (Authority && (timer == 1 || timer == 39 || timer == 77))
                        SpawnWaves(((int)timer - 1) / 38);
                    if (timer >= 105) { Enter(QueenAttack.Emerge); NPC.velocity = new Vector2(0, -8); }
                    break;
                case QueenAttack.Emerge:
                    if (timer == 1 && !Main.dedServ) SoundEngine.PlaySound(SoundID.Splash, NPC.Center);
                    NPC.velocity.Y = -8;
                    if ((int)timer % 4 == 0) Cue(2);
                    if (timer >= 38) NextAttack();
                    break;
                case QueenAttack.Summon:
                    MoveTo(new Vector2(player.Center.X - NPC.direction * 220, hoverY - 20), 4);
                    if ((int)timer % 6 == 0) Cue(4);
                    if (timer == 50 && Authority) SpawnMorays();
                    if (timer >= 95) NextAttack();
                    break;
                case QueenAttack.Sing:
                    if (timer < 35) MoveTo(new Vector2(player.Center.X - NPC.direction * 260, hoverY - 20), 5);
                    else NPC.velocity *= 0.88f;
                    if (timer == 1 && !Main.dedServ)
                        CombatText.NewText(NPC.Hitbox, Color.LightPink,
                            Language.GetTextValue("Mods.IslandAdventure.QueenEncounter.Singing"));
                    if (timer < 70 && (int)timer % 8 == 0) Cue(3);
                    if (timer == 70 && Authority)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ModContent.ProjectileType<RoyalSong>(), 0, 0, Main.myPlayer, NPC.whoAmI + 1);
                    if (!Main.dedServ && (timer == 35 || timer == 52 || timer == 70))
                        SoundEngine.PlaySound(SoundID.Item26 with { Pitch = (timer - 52) / 85f }, NPC.Center);
                    if (timer >= 130) NextAttack();
                    break;
            }
            if (BoatTravelSystem.IsCinematic) NPC.damage = 0;
            else NPC.damage = NPC.defDamage;
        }

        private void SpawnWaves(int size)
        {
            int damage = NPC.GetAttackDamage_ForProjectiles(22 + size * 4, 18 + size * 3);
            foreach (int direction in new[] { -1, 1 })
                Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X, SeaY - 32),
                    new Vector2(direction * 6, 0), ModContent.ProjectileType<RoyalWave>(), damage, 2,
                    Main.myPlayer, NPC.whoAmI + 1, SeaY, size);
            NPC.netUpdate = true;
        }
        private void SpawnMorays()
        {
            int count = 0;
            foreach (NPC npc in Main.npc)
                if (npc.active && npc.type == ModContent.NPCType<RoyalMoray>() && npc.ai[0] == NPC.whoAmI + 1) count++;
            for (int i = 0; i < Math.Min(3, 6 - count); i++)
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)arenaX + (i - 1) * 110, (int)SeaY + 80,
                    ModContent.NPCType<RoyalMoray>(), ai0: NPC.whoAmI + 1, ai2: i * 2f, ai3: SeaY);
            NPC.netUpdate = true;
        }

        public void ClearSummons()
        {
            if (!Authority) return;
            foreach (NPC npc in Main.npc)
                if (npc.active && npc.type == ModContent.NPCType<RoyalMoray>() && npc.ai[0] == NPC.whoAmI + 1)
                {
                    npc.active = false; npc.netUpdate = true;
                    if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
                }
            foreach (Projectile projectile in Main.projectile)
                if (projectile.active && (projectile.type == ModContent.ProjectileType<RoyalWave>() || projectile.type == ModContent.ProjectileType<RoyalSong>()) && projectile.ai[0] == NPC.whoAmI + 1)
                    projectile.Kill();
        }
        public override void OnKill() => ClearSummons();
        public override void BossLoot(ref int potionType) => potionType = ItemID.HealingPotion;
        public override void ModifyNPCLoot(NPCLoot loot)
        {
            loot.Add(ItemDropRule.Common(ItemID.WhitePearl, 1, 3, 5));
            loot.Add(ItemDropRule.Common(ItemID.PinkPearl, 3));
            loot.Add(ItemDropRule.Common(ItemID.Coral, 1, 10, 18));
        }

        public override void FindFrame(int frameHeight)
        {
            int animation = AnimationRow;
            if (lastAnimation != animation) { NPC.frameCounter = 0; lastAnimation = animation; }
            NPC.frameCounter += State == QueenAttack.Dash || State == QueenAttack.Dive ? 0.3 : 0.14;
            NPC.frame.Y = (animation * FramesPerAnimation + (int)NPC.frameCounter % FramesPerAnimation) * frameHeight;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => !BoatTravelSystem.IsCinematic && State != QueenAttack.Retreat;
        public override bool PreDraw(SpriteBatch batch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            SpriteEffects flip = NPC.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 origin = new Vector2(texture.Width / 2f, FrameSize / 2f);
            if (State == QueenAttack.Dash)
                for (int i = NPC.oldPos.Length - 1; i >= 0; i--)
                {
                    if (NPC.oldPos[i] == Vector2.Zero) continue;
                    Color ghost = new Color(255, 158, 205) * (0.22f * (1 - i / (float)NPC.oldPos.Length));
                    batch.Draw(texture, NPC.oldPos[i] + NPC.Size / 2 - screenPos, NPC.frame,
                        ghost, NPC.rotation, origin, NPC.scale, flip, 0);
                }
            Color tint = State == QueenAttack.DashWindup
                ? Color.Lerp(drawColor, Color.LightPink, 0.35f + 0.2f * (float)Math.Sin(NPC.ai[1] * 0.25)) : drawColor;
            batch.Draw(texture, NPC.Center - screenPos, NPC.frame, tint * NPC.Opacity,
                NPC.rotation, origin, NPC.scale, flip, 0);
            return false;
        }
    }
}
