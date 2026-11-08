using System;
using System.Collections.Generic;
using IslandAdventure.Subworlds;
using IslandAdventure.Travel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace IslandAdventure.Systems
{
    public class BoatTravelSystem : ModSystem
    {
        private static readonly TravelSequence sequence = new TravelSequence();
        private static Vector2 anchor;
        private static DateTime loadingSince;
        private static float reveal;
        private static Guid sourceWorld;
        private static Guid mainWorld;
        private static Vector2 worldMouse;
        public static bool IsCinematic => sequence.Cinematic;
        public static bool IsLoading => sequence.Phase == TravelPhase.Loading;

        public static bool TryStart(bool toIsland)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return Reject("Переправа пока доступна только в одиночной игре.");
            if (Main.gameMenu || Main.LocalPlayer.dead || !Main.LocalPlayer.active || sequence.Active)
                return false;
            if (toIsland ? SubworldSystem.AnyActive() : !SubworldSystem.IsActive<IslandSubworld>())
                return Reject("Переправа доступна между основным миром и островом Island Adventure.");

            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            if (!harbor.DiscoverExistingPier() && toIsland)
                return Reject("Причал не найден. Нужен мир с прибрежной постройкой Island Adventure.");
            if (!sequence.Begin(toIsland)) return false;
            Main.LocalPlayer.mount.Dismount(Main.LocalPlayer);
            Main.LocalPlayer.RemoveAllGrapplingHooks();
            Main.LocalPlayer.StopVanityActions();
            Main.LocalPlayer.SetTalkNPC(-1);
            Main.npcChatText = "";
            Main.playerInventory = false;
            Main.mapFullscreen = false;
            anchor = Main.LocalPlayer.position;
            sourceWorld = Main.ActiveWorldFileData.UniqueId;
            if (toIsland) mainWorld = sourceWorld;
            SoundEngine.PlaySound(SoundID.Splash with { Volume = 0.45f }, Main.LocalPlayer.Center);
            return true;
        }

        private static bool Reject(string message)
        {
            Main.NewText(message, new Color(245, 205, 125));
            return false;
        }

        public static void HoldPlayer(Player player)
        {
            player.position = anchor;
            player.velocity = Vector2.Zero;
            player.fallStart = (int)(player.position.Y / 16);
            player.breath = player.breathMax;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.dedServ) return;
            if (!Main.gameMenu) reveal = Math.Max(0, reveal - (float)gameTime.ElapsedGameTime.TotalSeconds / 0.65f);
            if (!sequence.Active) return;
            if (!Main.gameMenu && Main.LocalPlayer.dead)
            {
                sequence.Reset();
                reveal = 0;
                return;
            }
            if (IsLoading)
            {
                if (DateTime.UtcNow - loadingSince > TimeSpan.FromMinutes(10))
                {
                    sequence.Reset();
                    return;
                }
                bool destination = sequence.ToIsland
                    ? SubworldSystem.IsActive<IslandSubworld>() : !SubworldSystem.AnyActive();
                if (!Main.gameMenu && Main.ActiveWorldFileData.UniqueId != sourceWorld
                    && (!destination || (!sequence.ToIsland && mainWorld != Guid.Empty
                        && Main.ActiveWorldFileData.UniqueId != mainWorld)))
                {
                    sequence.Reset();
                    return;
                }
                if (!Main.gameMenu && destination && Main.LocalPlayer.active && !WorldGen.generatingWorld)
                {
                    HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
                    harbor.DiscoverExistingPier();
                    if (harbor.TryLanding(Main.LocalPlayer, out Vector2 landing))
                        Main.LocalPlayer.Teleport(landing, TeleportationStyleID.RodOfDiscord);
                    anchor = Main.LocalPlayer.position;
                    sequence.DestinationReady();
                }
                return;
            }
            if (Main.gameMenu) return;
            Main.playerInventory = false;
            Main.LocalPlayer.mouseInterface = true;
            Process(sequence.Advance((float)gameTime.ElapsedGameTime.TotalSeconds));
        }

        private static void Process(TravelAction action)
        {
            if (action == TravelAction.LoadDestination)
            {
                loadingSince = DateTime.UtcNow;
                try
                {
                    if (sequence.ToIsland)
                    {
                        if (!SubworldSystem.Enter<IslandSubworld>())
                        {
                            sequence.Reset();
                            Reject("Не удалось начать переход. Попробуй ещё раз.");
                        }
                    }
                    else
                    {
                        SubworldSystem.Exit();
                        if (!Main.gameMenu)
                        {
                            sequence.Reset();
                            Reject("Предыдущий переход ещё не завершён. Попробуй вернуться через несколько секунд.");
                        }
                    }
                }
                catch (Exception exception)
                {
                    sequence.Reset();
                    ModContent.GetInstance<BoatTravelSystem>().Mod.Logger.Error("Boat transition failed", exception);
                    Reject("Ошибка переправы. Подробности записаны в client.log.");
                }
            }
            else if (action == TravelAction.Finish)
            {
                Player player = Main.LocalPlayer;
                if (!ModContent.GetInstance<HarborSystem>().HasHarbor)
                    Main.NewText("Причал повреждён или не найден: высадка в безопасной точке появления.", Color.Wheat);
                reveal = 1;
                player.velocity = Vector2.Zero;
                player.fallStart = (int)(player.position.Y / 16);
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 120);
                SoundEngine.PlaySound(SoundID.Splash with { Volume = 0.4f }, player.Center);
            }
        }

        public override void PreSaveAndQuit() { sequence.Reset(); reveal = 0; sourceWorld = mainWorld = Guid.Empty; }
        public override void Unload() { sequence.Reset(); reveal = 0; sourceWorld = mainWorld = Guid.Empty; BoatArt.Unload(); }

        public override void PostDrawTiles()
        {
            if (Main.dedServ || sequence.Active) return;
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            if (!harbor.HasHarbor) return;
            Vector2 position = harbor.BoatPosition - Main.screenPosition;
            if (position.X < -150 || position.X > Main.screenWidth + 150
                || position.Y < -100 || position.Y > Main.screenHeight + 100) return;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
            BoatArt.Draw(Main.spriteBatch, position, 1f, harbor.FacesLeft, Main.GlobalTimeWrappedHourly);
            Main.spriteBatch.End();
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Add(new LegacyGameInterfaceLayer("IslandAdventure: Boat travel", DrawLayer, InterfaceScaleType.UI));
        }

        private static bool DrawLayer()
        {
            PlayerInput.SetZoom_World();
            worldMouse = Main.MouseWorld;
            PlayerInput.SetZoom_UI();
            // Player-head shaders need immediate drawing; keep the boat pixel sharp.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            try { return DrawInterface(); }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            }
        }

        private static bool DrawInterface()
        {
            if (Main.gameMenu || Main.dedServ) return true;
            if (IsCinematic)
            {
                DrawScene(Main.spriteBatch, false);
                Vector2 size = Canvas;
                if (Button(new Rectangle((int)size.X / 2 - 90, (int)size.Y - 62, 180, 34), "Пропустить сцену"))
                    Process(sequence.Skip());
                return true;
            }
            if (IsLoading)
            {
                Box(Main.spriteBatch, new Rectangle(0, 0, (int)Canvas.X, (int)Canvas.Y), Color.Black);
                return true;
            }
            if (reveal > 0)
            {
                Box(Main.spriteBatch, new Rectangle(0, 0, (int)Canvas.X, (int)Canvas.Y), Color.Black * reveal);
                return true;
            }
            if (Main.mapFullscreen || Main.LocalPlayer.dead) return true;
            int talk = Main.LocalPlayer.talkNPC;
            bool angler = talk >= 0 && talk < Main.maxNPCs && Main.npc[talk].active
                && Main.npc[talk].type == NPCID.Angler && !SubworldSystem.AnyActive();
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            bool nearDock = false;
            if (harbor.HasHarbor)
            {
                Vector2 land = new Vector2(harbor.Landing.X * 16, harbor.Landing.Y * 16);
                Vector2 route = harbor.BoatPosition - land;
                float t = MathHelper.Clamp(Vector2.Dot(Main.LocalPlayer.Center - land, route) / Math.Max(1, route.LengthSquared()), 0, 1);
                nearDock = Vector2.Distance(Main.LocalPlayer.Center, land + route * t) < 160;
                Rectangle boatBounds = new Rectangle((int)harbor.BoatPosition.X - 52, (int)harbor.BoatPosition.Y - 55, 104, 70);
                if (nearDock && talk < 0 && boatBounds.Contains(worldMouse.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    Utils.DrawBorderString(Main.spriteBatch, "ПКМ — сесть в лодку", Main.MouseScreen + new Vector2(15, 18), Color.Wheat, 0.8f);
                    if (Main.mouseRight && Main.mouseRightRelease)
                    {
                        Main.mouseRightRelease = false;
                        TryStart(!SubworldSystem.IsActive<IslandSubworld>());
                    }
                }
            }
            if (angler || (nearDock && talk < 0))
            {
                Vector2 size = Canvas;
                bool island = SubworldSystem.IsActive<IslandSubworld>();
                string label = island ? "На лодке домой" : "На лодке с Рыбаком на остров";
                Rectangle bounds = new Rectangle((int)size.X / 2 - 165, (int)size.Y - 125, 330, 42);
                if (Button(bounds, label)) TryStart(!island);
                Utils.DrawBorderString(Main.spriteBatch, "Лодочная переправа", new Vector2(size.X / 2, bounds.Y - 23),
                    new Color(210, 231, 231), 0.8f, 0.5f);
            }
            return true;
        }

        // SetZoom_UI has already converted dimensions and mouse coordinates.
        private static Vector2 Canvas => new Vector2(Main.screenWidth, Main.screenHeight);

        private static bool Button(Rectangle bounds, string text)
        {
            Vector2 mouse = Main.MouseScreen;
            bool hover = bounds.Contains(mouse.ToPoint());
            Box(Main.spriteBatch, bounds, hover ? new Color(49, 113, 126) : new Color(25, 60, 78));
            Box(Main.spriteBatch, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), new Color(202, 161, 95));
            Utils.DrawBorderString(Main.spriteBatch, text, new Vector2(bounds.Center.X, bounds.Y + 10), Color.White, 0.85f, 0.5f);
            if (!hover) return false;
            Main.LocalPlayer.mouseInterface = true;
            if (!Main.mouseLeft || !Main.mouseLeftRelease) return false;
            Main.mouseLeftRelease = false;
            SoundEngine.PlaySound(SoundID.MenuTick);
            return true;
        }

        public static void DrawLoading()
        {
            if (sequence.Active) DrawScene(Main.spriteBatch, true);
            else Utils.DrawBorderString(Main.spriteBatch, Main.statusText, Canvas / 2, Color.White, 1, 0.5f);
        }

        private static void DrawScene(SpriteBatch batch, bool loading)
        {
            Vector2 size = Canvas;
            float time = Main.GlobalTimeWrappedHourly;
            float horizon = size.Y * 0.46f;
            float sea = size.Y * 0.69f;
            Box(batch, new Rectangle(0, 0, (int)size.X, (int)size.Y), new Color(39, 80, 111));
            for (int row = 0; row < 30; row++)
            {
                float p = row / 30f;
                Color color = Color.Lerp(new Color(78, 148, 167), new Color(15, 46, 76), p);
                Box(batch, new Rectangle(0, (int)(horizon + p * (size.Y - horizon)), (int)size.X,
                    (int)((size.Y - horizon) / 30) + 2), color);
            }
            Box(batch, new Rectangle((int)(size.X * 0.77f), (int)(horizon * 0.3f), 42, 42), new Color(246, 210, 144));
            for (int row = 0; row < 12; row++)
                for (int wave = -1; wave < 10; wave++)
                {
                    float x = wave * 130 + (float)Math.Sin(row * 9) * 65 + time * (row % 2 == 0 ? 13 : -10) % 130;
                    float y = horizon + 14 + row * (size.Y - horizon) / 12;
                    Box(batch, new Rectangle((int)x, (int)y, 28 + row * 2, 2), new Color(144, 212, 214) * 0.3f);
                }
            float scale = Math.Min(3f, size.X / 260f);
            bool left = sequence.ToIsland;
            float xBoat = size.X / 2;
            if (!loading)
            {
                if (sequence.Phase == TravelPhase.Departing)
                {
                    float p = MathHelper.SmoothStep(0, 1, sequence.Elapsed / TravelSequence.DepartureSeconds);
                    xBoat = MathHelper.Lerp(size.X / 2, left ? -170 * scale : size.X + 170 * scale, p);
                }
                else
                {
                    float p = MathHelper.SmoothStep(0, 1, sequence.Elapsed / TravelSequence.ArrivalSeconds);
                    xBoat = MathHelper.Lerp(left ? size.X + 170 * scale : -170 * scale, size.X / 2, p);
                }
            }
            Vector2 boat = new Vector2(xBoat, sea);
            for (int ripple = 0; ripple < 9; ripple++)
            {
                float p = (time * 0.7f + ripple / 9f) % 1;
                float wakeX = xBoat + (left ? 1 : -1) * (40 + p * 65) * scale;
                float wakeY = sea + (3 + (float)Math.Sin(p * 9 + ripple)) * scale;
                Box(batch, new Rectangle((int)wakeX, (int)wakeY, (int)((12 - p * 9) * scale), 2),
                    new Color(190, 231, 221) * ((1 - p) * 0.65f));
            }
            if (!loading)
            {
                bool right = sequence.Phase == TravelPhase.Departing ? left : !left;
                int edge = (int)(size.X * (right ? 0.65f : 0.35f));
                int deckY = (int)(sea - 12 * scale);
                int start = right ? edge : 0;
                int width = right ? (int)size.X - edge : edge;
                Box(batch, new Rectangle(start, deckY, width, (int)(3 * scale)), new Color(136, 90, 57));
                Box(batch, new Rectangle(start, deckY, width, (int)scale), new Color(213, 166, 93));
                for (int x = start + 12; x < start + width; x += 70)
                {
                    Box(batch, new Rectangle(x, deckY + 3, 6, 115), new Color(69, 53, 44));
                    Box(batch, new Rectangle(x, deckY - 22, 4, 22), new Color(147, 105, 65));
                }
            }
            DrawPassengers(batch, boat + new Vector2(0, -14 * scale + (float)Math.Sin(time * 2.2f) * 1.4f * scale), scale, loading, left);
            BoatArt.Draw(batch, boat, scale, left, time);
            Box(batch, new Rectangle(0, 0, (int)size.X, 45), Color.Black * 0.7f);
            Box(batch, new Rectangle(0, (int)size.Y - 83, (int)size.X, 83), Color.Black * 0.7f);
            string title = sequence.ToIsland ? "К новому острову" : "Обратно домой";
            Utils.DrawBorderString(batch, title, new Vector2(size.X / 2, 14), Color.Wheat, 1, 0.5f);
            if (loading)
                Utils.DrawBorderString(batch, Main.statusText, new Vector2(size.X / 2, size.Y - 55), Color.White, 0.8f, 0.5f);
            else
            {
                float fade = sequence.Phase == TravelPhase.Departing
                    ? MathHelper.Clamp((sequence.Elapsed - TravelSequence.DepartureSeconds + 0.65f) / 0.65f, 0, 1)
                    : Math.Max(1 - MathHelper.Clamp(sequence.Elapsed / 0.6f, 0, 1),
                        MathHelper.Clamp((sequence.Elapsed - TravelSequence.ArrivalSeconds + 0.45f) / 0.45f, 0, 1));
                Box(batch, new Rectangle(0, 0, (int)size.X, (int)size.Y), Color.Black * fade);
            }
        }

        private static void DrawPassengers(SpriteBatch batch, Vector2 deck, float scale, bool loading, bool left)
        {
            float actorScale = scale * 0.55f;
            Vector2 playerFeet = deck + new Vector2(-14 * scale, 0);
            Player player = Main.LocalPlayer;
            Box(batch, new Rectangle((int)(playerFeet.X - 5 * actorScale), (int)(playerFeet.Y - 20 * actorScale),
                (int)(10 * actorScale), (int)(18 * actorScale)), player.shirtColor);
            if (!loading)
            {
                Main.PlayerRenderer.DrawPlayerHead(Main.Camera, player, playerFeet - new Vector2(0, 32 * actorScale), scale: actorScale);
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }
            else
                Box(batch, new Rectangle((int)(playerFeet.X - 5 * actorScale), (int)(playerFeet.Y - 31 * actorScale),
                    (int)(10 * actorScale), (int)(11 * actorScale)), player.skinColor);

            if (!loading)
            {
                Main.instance.LoadNPC(NPCID.Angler);
                Texture2D npc = TextureAssets.Npc[NPCID.Angler].Value;
                int frameHeight = npc.Height / Main.npcFrameCount[NPCID.Angler];
                batch.Draw(npc, deck + new Vector2(12 * scale, 0), new Rectangle(0, 0, npc.Width, frameHeight), Color.White,
                    0, new Vector2(npc.Width / 2f, frameHeight), actorScale,
                    left ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }
            else
            {
                Vector2 captain = deck + new Vector2(12 * scale, 0);
                Box(batch, new Rectangle((int)(captain.X - 5 * actorScale), (int)(captain.Y - 21 * actorScale),
                    (int)(10 * actorScale), (int)(20 * actorScale)), new Color(158, 93, 65));
                Box(batch, new Rectangle((int)(captain.X - 5 * actorScale), (int)(captain.Y - 32 * actorScale),
                    (int)(10 * actorScale), (int)(11 * actorScale)), new Color(236, 176, 132));
            }
        }

        private static void Box(SpriteBatch batch, Rectangle rectangle, Color color)
            => batch.Draw(TextureAssets.MagicPixel.Value, rectangle, color);
    }
}
