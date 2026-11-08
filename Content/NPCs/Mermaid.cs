using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Subworlds;
using IslandAdventure.Systems;
using SubworldLibrary;

namespace IslandAdventure.Content.NPCs
{
    public class Mermaid : ModNPC
    {
        public const int FrameCount = 6;
        public const string RedTexturePath = "IslandAdventure/Assets/NPCs/Mermaid_Red";
        private static Asset<Texture2D> redTexture;
        public bool IsRed => NPC.ai[3] == 1;
        public override string Texture => "IslandAdventure/Assets/NPCs/Mermaid";

        public override void Load()
        {
            if (!Main.dedServ) redTexture = ModContent.Request<Texture2D>(RedTexturePath);
        }
        public override void Unload() => redTexture = null;

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            NPC.ai[3] = Main.rand.NextBool(3) ? 1 : 0;
            NPC.netUpdate = true;
        }

        public override void SetStaticDefaults() => Main.npcFrameCount[Type] = FrameCount;

        public override void SetDefaults()
        {
            NPC.width = 36; NPC.height = 44;
            NPC.lifeMax = 80; NPC.damage = 22; NPC.defense = 6;
            NPC.knockBackResist = 0.45f;
            NPC.value = Item.buyPrice(silver: 2);
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.Add(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean);
            bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.IslandAdventure.NPCs.Mermaid.Bestiary"));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!SubworldSystem.IsActive<IslandSubworld>() || !spawnInfo.Water || !spawnInfo.Player.ZoneBeach
                || spawnInfo.PlayerSafe || spawnInfo.PlayerInTown || BoatTravelSystem.IsCinematic
                || spawnInfo.SpawnTileY > Main.worldSurface + 30 || NPC.CountNPCS(Type) >= 3) return 0;
            bool coast = spawnInfo.SpawnTileX < 380 || spawnInfo.SpawnTileX > Main.maxTilesX - 380;
            return coast ? (Main.dayTime ? 0.08f : 0.14f) : 0;
        }

        public override void AI()
        {
            NPC.TargetClosest(faceTarget: false);
            NPC.ai[2]++;
            NPC.noGravity = NPC.wet;
            if (!NPC.wet)
            {
                NPC.ai[0] = NPC.ai[1] = 0;
                NPC.rotation *= 0.85f;
                NPC.velocity.X *= 0.94f;
                if (NPC.velocity.Y == 0 && NPC.ai[2] % 60 == 0)
                {
                    // Flop towards nearby water, rather than pursuing players on land.
                    int toward = WaterDirection();
                    NPC.velocity = new Vector2(toward * 1.4f, -2.8f);
                    if (toward != 0) NPC.direction = toward;
                }
                NPC.spriteDirection = NPC.direction;
                return;
            }

            Player target = NPC.target >= 0 && NPC.target < Main.maxPlayers ? Main.player[NPC.target] : null;
            bool hunting = target != null && target.active && !target.dead
                && Vector2.DistanceSquared(NPC.Center, target.Center) < 640 * 640
                && !BoatTravelSystem.IsCinematic;
            Vector2 offset = hunting ? target.Center - NPC.Center
                : new Vector2(NPC.direction * 80, (float)System.Math.Sin(NPC.ai[2] / 45) * 25);
            if (NPC.collideX) { offset.X = -NPC.direction * 80; NPC.velocity.X *= -0.5f; }
            if (NPC.collideY) { offset.Y = NPC.velocity.Y > 0 ? -60 : 60; NPC.velocity.Y *= -0.5f; }
            NPC.ai[0]++;

            if (NPC.ai[1] > 0)
            {
                NPC.ai[1]--;
                NPC.velocity *= 0.985f;
                if (NPC.collideX || NPC.collideY) NPC.ai[1] = 0;
            }
            else
            {
                Vector2 desired = offset.SafeNormalize(Vector2.UnitX * NPC.direction) * (hunting ? 2.2f : 0.9f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.045f);
                if (hunting && target.wet && NPC.ai[0] >= 110 && offset.LengthSquared() < 320 * 320
                    && offset.LengthSquared() > 64 * 64 && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height)
                    && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity = offset.SafeNormalize(Vector2.UnitX) * 4.2f;
                    NPC.ai[0] = 0; NPC.ai[1] = 20;
                    NPC.netUpdate = true;
                }
            }

            // Do not leap out of the sea when a target is standing on the pier.
            Vector2 next = NPC.position + NPC.velocity * 8;
            if (!Collision.WetCollision(next, NPC.width, NPC.height))
            {
                NPC.ai[1] = 0;
                NPC.velocity.X *= 0.85f;
                NPC.velocity.Y = System.Math.Max(0.6f, NPC.velocity.Y);
            }
            if (System.Math.Abs(NPC.velocity.X) > 0.15f) NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = MathHelper.Clamp(NPC.velocity.Y * NPC.direction * 0.035f, -0.18f, 0.18f);
        }

        private int WaterDirection()
        {
            foreach (int distance in new[] { 32, 64, 96, 128 })
                foreach (int direction in new[] { NPC.direction, -NPC.direction })
                    if (Collision.WetCollision(NPC.position + new Vector2(direction * distance, 16), NPC.width, NPC.height))
                        return direction;
            return 0;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += NPC.wet ? (NPC.ai[1] > 0 ? 0.25 : 0.13) : 0.06;
            NPC.frame.Y = ((int)NPC.frameCounter % FrameCount) * frameHeight;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => !BoatTravelSystem.IsCinematic;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = IsRed ? redTexture.Value : TextureAssets.Npc[Type].Value;
            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation,
                new Vector2(texture.Width / 2f, texture.Height / FrameCount / 2f), NPC.scale,
                NPC.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Coral, 3, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemID.Seashell, 5));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < (NPC.life <= 0 ? 16 : 4); i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Water, hit.HitDirection, -1,
                    newColor: IsRed ? new Color(228, 103, 91) : new Color(93, 205, 190));
        }
    }
}
