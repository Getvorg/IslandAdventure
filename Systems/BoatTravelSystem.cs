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
using Terraria.WorldBuilding;

namespace IslandAdventure.Systems
{
    public class BoatTravelSystem : ModSystem
    {
        private static readonly TravelSequence sequence = new TravelSequence();
        private static Vector2 anchor, landingAnchor, sceneDock, worldMouse;
        private static bool sceneHasHarbor, sceneFacesLeft;
        private static DateTime loadingSince;
        private static float reveal;
        private static Guid sourceWorld, mainWorld;
        private static Player passenger;
        public static bool IsCinematic => sequence.Cinematic;
        public static bool IsLoading => sequence.Phase == TravelPhase.Loading;
        public static bool IsWorldScene => IsCinematic && sceneHasHarbor;
        public static bool RenderingPassenger { get; private set; }
        private static bool BoatFacesLeft => sequence.Phase == TravelPhase.Departing ? sceneFacesLeft : !sceneFacesLeft;
        private static Vector2 MovingBoat => BoatWorldMotion.Position(sceneDock, sceneFacesLeft,
            sequence.Phase == TravelPhase.Departing,
            sequence.Elapsed / (sequence.Phase == TravelPhase.Departing
                ? TravelSequence.DepartureSeconds : TravelSequence.ArrivalSeconds), Main.maxTilesX * 16f);

        public static bool TryStart(bool toIsland)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                return Reject("Переправа пока доступна только в одиночной игре.");
            if (Main.gameMenu || Main.LocalPlayer.dead || !Main.LocalPlayer.active || sequence.Active)
                return false;
            if (toIsland ? SubworldSystem.AnyActive() : !SubworldSystem.IsActive<IslandSubworld>())
                return Reject("Переправа доступна между основным миром и островом Island Adventure.");

            Player player = Main.LocalPlayer;
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            bool foundHarbor = harbor.DiscoverExistingPier();
            Vector2 landing = player.position;
            bool safeHarbor = foundHarbor && harbor.TryLanding(player, out landing);
            if (!safeHarbor && toIsland)
                return Reject("Не найден безопасный причал. Проверь, что на настиле есть свободное место.");
            if (!ModContent.GetInstance<AnglerCompanionSystem>().CanBoard(toIsland)) return false;
            if (!sequence.Begin(toIsland)) return false;
            try
            {
                passenger = player.SerializedClone();
                passenger.active = true;
                passenger.dead = false;
                passenger.inventory[passenger.selectedItem] = new Item();
                passenger.mount.Dismount(passenger);
                using (new Main.CurrentPlayerOverride(passenger))
                {
                    passenger.ResetEffects();
                    passenger.ResetVisibleAccessories();
                    passenger.UpdateDyes();
                    passenger.PlayerFrame();
                }
            }
            catch (Exception exception)
            {
                sequence.Reset();
                passenger = null;
                ModContent.GetInstance<BoatTravelSystem>().Mod.Logger.Error("Boat passenger preview failed", exception);
                return Reject("Не удалось подготовить персонажа к переправе. Подробности в client.log.");
            }
            player.mount.Dismount(player);
            player.RemoveAllGrapplingHooks();
            player.StopVanityActions();
            player.channel = false;
            player.itemAnimation = player.itemTime = 0;
            player.SetTalkNPC(-1);
            Main.npcChatText = "";
            Main.playerInventory = Main.mapFullscreen = false;
            sourceWorld = Main.ActiveWorldFileData.UniqueId;
            if (toIsland) mainWorld = sourceWorld;
            ConfigureScene(harbor, safeHarbor, landing);
            if (safeHarbor)
            {
                UpdateSeat();
                MovePlayer(anchor);
                SoundEngine.PlaySound(SoundID.Splash with { Volume = 0.45f }, MovingBoat);
            }
            else Process(sequence.Skip()); // Keep /home usable on older islands without a pier.
            return true;
        }

        private static void ConfigureScene(HarborSystem harbor, bool safeHarbor, Vector2 landing)
        {
            landingAnchor = landing;
            anchor = landing;
            sceneHasHarbor = safeHarbor;
            sceneDock = harbor.BoatPosition;
            sceneFacesLeft = harbor.FacesLeft;
        }

        private static bool Reject(string message)
        {
            Main.NewText(message, new Color(245, 205, 125));
            return false;
        }

        private static void MovePlayer(Vector2 position)
        {
            Player player = Main.LocalPlayer;
            player.position = player.oldPosition = position;
            player.velocity = Vector2.Zero;
            player.fallStart = (int)(position.Y / 16);
            player.gfxOffY = 0;
        }

        private static void UpdateSeat()
        {
            if (!IsWorldScene) return;
            float bob = (float)Math.Round(Math.Sin(Main.GlobalTimeWrappedHourly * 2.2f));
            Player player = Main.LocalPlayer;
            anchor = MovingBoat + new Vector2(-24, BoatArt.SeatHeight + bob) - new Vector2(player.width / 2f, player.height);
        }

        public static void HoldPlayer(Player player)
        {
            player.position = anchor;
            player.velocity = Vector2.Zero;
            player.fallStart = (int)(anchor.Y / 16);
            player.breath = player.breathMax;
        }

        public static void ApplyCamera()
        {
            if (!IsWorldScene || Main.gameMenu) return;
            Main.screenPosition = BoatWorldMotion.Camera(MovingBoat,
                landingAnchor + Main.LocalPlayer.Size / 2,
                new Vector2(Main.screenWidth, Main.screenHeight),
                new Vector2(Main.maxTilesX * 16, Main.maxTilesY * 16), Main.GameViewMatrix.Zoom.X);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.dedServ) return;
            if (!Main.gameMenu) reveal = Math.Max(0, reveal - (float)gameTime.ElapsedGameTime.TotalSeconds / 0.65f);
            if (!sequence.Active) return;
            if (!Main.gameMenu && Main.LocalPlayer.dead) { Cancel(false); return; }
            if (IsLoading)
            {
                if (DateTime.UtcNow - loadingSince > TimeSpan.FromMinutes(10)) { Cancel(false); return; }
                bool destination = sequence.ToIsland
                    ? SubworldSystem.IsActive<IslandSubworld>() : !SubworldSystem.AnyActive();
                if (!Main.gameMenu && Main.ActiveWorldFileData.UniqueId != sourceWorld
                    && (!destination || (!sequence.ToIsland && mainWorld != Guid.Empty
                        && Main.ActiveWorldFileData.UniqueId != mainWorld)))
                {
                    Cancel(false);
                    return;
                }
                if (!Main.gameMenu && destination && Main.LocalPlayer.active && !WorldGen.generatingWorld)
                {
                    HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
                    bool found = harbor.DiscoverExistingPier();
                    Vector2 landing = Main.LocalPlayer.position;
                    bool safe = found && harbor.TryLanding(Main.LocalPlayer, out landing);
                    ConfigureScene(harbor, safe, landing);
                    ModContent.GetInstance<AnglerCompanionSystem>().Arrive();
                    sequence.DestinationReady();
                    if (safe)
                    {
                        UpdateSeat();
                        MovePlayer(anchor);
                    }
                    else Process(sequence.Skip());
                }
                return;
            }
            if (Main.gameMenu) return;
            Main.playerInventory = false;
            Main.LocalPlayer.mouseInterface = true;
            Process(sequence.Advance((float)gameTime.ElapsedGameTime.TotalSeconds));
            UpdateSeat();
        }

        private static void Process(TravelAction action)
        {
            if (action == TravelAction.LoadDestination)
            {
                loadingSince = DateTime.UtcNow;
                try
                {
                    ModContent.GetInstance<AnglerCompanionSystem>().DetachForTransfer();
                    bool accepted;
                    if (sequence.ToIsland) accepted = SubworldSystem.Enter<IslandSubworld>();
                    else { SubworldSystem.Exit(); accepted = Main.gameMenu; }
                    if (!accepted)
                    {
                        Cancel(true);
                        Reject("Не удалось начать переход. Попробуй ещё раз через несколько секунд.");
                    }
                    else sceneHasHarbor = false;
                }
                catch (Exception exception)
                {
                    Cancel(true);
                    ModContent.GetInstance<BoatTravelSystem>().Mod.Logger.Error("Boat transition failed", exception);
                    Reject("Ошибка переправы. Подробности записаны в client.log.");
                }
            }
            else if (action == TravelAction.Finish)
            {
                Player player = Main.LocalPlayer;
                if (!sceneHasHarbor)
                    Main.NewText("Причал повреждён или не найден: высадка в безопасной точке появления.", Color.Wheat);
                // Recheck after the scene in case a tile changed while approaching.
                HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
                if (harbor.TryLanding(player, out Vector2 landing)) landingAnchor = landing;
                MovePlayer(landingAnchor);
                reveal = 1;
                player.immune = true;
                player.immuneTime = Math.Max(player.immuneTime, 120);
                SoundEngine.PlaySound(SoundID.Splash with { Volume = 0.4f }, player.Center);
                sceneHasHarbor = false;
                passenger = null;
            }
        }

        private static void Cancel(bool restoreLanding)
        {
            if (restoreLanding) ModContent.GetInstance<AnglerCompanionSystem>().RestoreRejectedDeparture();
            if (restoreLanding && sceneHasHarbor && !Main.gameMenu && !Main.LocalPlayer.dead)
                MovePlayer(landingAnchor);
            sequence.Reset();
            sceneHasHarbor = false;
            passenger = null;
            reveal = restoreLanding ? 1 : 0;
        }

        public override void PreSaveAndQuit()
        {
            Cancel(true);
            reveal = 0;
            sourceWorld = mainWorld = Guid.Empty;
        }

        public override void Unload()
        {
            Cancel(false);
            sourceWorld = mainWorld = Guid.Empty;
            BoatArt.Unload();
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || Main.gameMenu) return;
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            if (IsWorldScene || harbor.HasHarbor)
            {
                Vector2 boat = IsWorldScene ? MovingBoat : harbor.BoatPosition;
                bool left = IsWorldScene ? BoatFacesLeft : harbor.FacesLeft;
                Lighting.AddLight(boat + new Vector2(left ? -79 : 79, -38), 0.45f, 0.3f, 0.12f);
            }
        }

        public override void PostDrawTiles()
        {
            if (Main.dedServ || Main.gameMenu) return;
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            bool moving = IsWorldScene;
            if (!moving && (sequence.Active || !harbor.HasHarbor)) return;
            Vector2 world = moving ? MovingBoat : harbor.BoatPosition;
            Vector2 position = world - Main.screenPosition;
            if (position.X < -170 || position.X > Main.screenWidth + 170
                || position.Y < -130 || position.Y > Main.screenHeight + 130) return;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {
                bool left = moving ? BoatFacesLeft : harbor.FacesLeft;
                Color light = Color.Lerp(Lighting.GetColor((int)(world.X / 16), (int)(world.Y / 16) - 1), Color.White, 0.12f);
                if (moving)
                {
                    float bob = (float)Math.Round(Math.Sin(Main.GlobalTimeWrappedHourly * 2.2f));
                    DrawPassengers(world + new Vector2(0, BoatArt.SeatHeight + bob), left, light);
                }
                BoatArt.Draw(Main.spriteBatch, position, 1, left, Main.GlobalTimeWrappedHourly, rowing: moving, lighting: light);
            }
            finally { Main.spriteBatch.End(); }
        }

        private static void DrawPassengers(Vector2 deck, bool left, Color light)
        {
            if (passenger != null)
            {
                RenderingPassenger = true;
                try
                {
                    using (new Main.CurrentPlayerOverride(passenger))
                    {
                        passenger.direction = left ? -1 : 1;
                        passenger.bodyFrame.Y = passenger.legFrame.Y = passenger.headFrame.Y = 0;
                        passenger.position = deck + new Vector2(-24 - passenger.width / 2f, -passenger.height);
                        Main.PlayerRenderer.DrawPlayer(Main.Camera, passenger, passenger.position, 0, Vector2.Zero);
                    }
                }
                finally
                {
                    RenderingPassenger = false;
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
            }
            Main.instance.LoadNPC(NPCID.Angler);
            Texture2D npc = TextureAssets.Npc[NPCID.Angler].Value;
            int frameHeight = npc.Height / Main.npcFrameCount[NPCID.Angler];
            Main.spriteBatch.Draw(npc, deck + new Vector2(24, 0) - Main.screenPosition,
                new Rectangle(0, 0, npc.Width, frameHeight), light, 0,
                new Vector2(npc.Width / 2f, frameHeight), 1,
                left ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // This list is a per-frame copy; normal HUD/mod panels return afterwards.
            if (sequence.Active || reveal > 0) layers.Clear();
            layers.Add(new LegacyGameInterfaceLayer("IslandAdventure: Boat travel", DrawLayer, InterfaceScaleType.UI));
        }

        private static bool DrawLayer()
        {
            PlayerInput.SetZoom_World();
            worldMouse = Main.MouseWorld;
            PlayerInput.SetZoom_UI();
            return DrawInterface();
        }

        private static bool DrawInterface()
        {
            if (Main.gameMenu || Main.dedServ) return true;
            if (IsCinematic)
            {
                Vector2 size = Canvas;
                Box(Main.spriteBatch, new Rectangle(0, 0, (int)size.X, 38), Color.Black * 0.35f);
                Box(Main.spriteBatch, new Rectangle(0, (int)size.Y - 70, (int)size.X, 70), Color.Black * 0.35f);
                Utils.DrawBorderString(Main.spriteBatch, sequence.ToIsland ? "К новому острову" : "Обратно домой",
                    new Vector2(size.X / 2, 10), Color.White, 0.9f, 0.5f);
                float duration = sequence.Phase == TravelPhase.Departing
                    ? TravelSequence.DepartureSeconds : TravelSequence.ArrivalSeconds;
                float fade = Math.Max(1 - MathHelper.Clamp(sequence.Elapsed / 0.4f, 0, 1),
                    MathHelper.Clamp((sequence.Elapsed - duration + 0.6f) / 0.6f, 0, 1));
                if (!IsWorldScene) fade = 1;
                Box(Main.spriteBatch, new Rectangle(0, 0, (int)size.X, (int)size.Y), Color.Black * fade);
                if (Button(new Rectangle((int)size.X / 2 - 90, (int)size.Y - 53, 180, 34), "Пропустить сцену"))
                    Process(sequence.Skip());
                return true;
            }
            if (IsLoading || reveal > 0)
            {
                Box(Main.spriteBatch, new Rectangle(0, 0, (int)Canvas.X, (int)Canvas.Y), Color.Black * (IsLoading ? 1 : reveal));
                return true;
            }
            if (Main.mapFullscreen || Main.LocalPlayer.dead) return true;
            int talk = Main.LocalPlayer.talkNPC;
            bool angler = talk >= 0 && talk < Main.maxNPCs && Main.npc[talk].active
                && Main.npc[talk].type == NPCID.Angler
                && (!SubworldSystem.AnyActive() || SubworldSystem.IsActive<IslandSubworld>());
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            bool nearDock = false;
            if (harbor.HasHarbor)
            {
                Vector2 land = new Vector2(harbor.Landing.X * 16, harbor.Landing.Y * 16);
                Vector2 route = harbor.BoatPosition - land;
                float t = MathHelper.Clamp(Vector2.Dot(Main.LocalPlayer.Center - land, route) / Math.Max(1, route.LengthSquared()), 0, 1);
                nearDock = Vector2.Distance(Main.LocalPlayer.Center, land + route * t) < 160;
                Rectangle bounds = new Rectangle((int)harbor.BoatPosition.X - BoatArt.Width / 2,
                    (int)harbor.BoatPosition.Y - BoatArt.Waterline, BoatArt.Width, BoatArt.Height);
                if (nearDock && talk < 0 && bounds.Contains(worldMouse.ToPoint()))
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

        private static Vector2 Canvas => new Vector2(Main.screenWidth, Main.screenHeight);

        private static bool Button(Rectangle bounds, string text)
        {
            bool hover = bounds.Contains(Main.MouseScreen.ToPoint());
            Utils.DrawInvBG(Main.spriteBatch, bounds, hover ? new Color(70, 91, 154) : new Color(40, 53, 105));
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
            Vector2 size = Canvas;
            Box(Main.spriteBatch, new Rectangle(0, 0, (int)size.X, (int)size.Y), Color.Black);
            int width = Math.Min(600, (int)size.X - 32);
            Rectangle panel = new Rectangle(((int)size.X - width) / 2, (int)size.Y / 2 - 55, width, 110);
            Utils.DrawInvBG(Main.spriteBatch, panel, new Color(37, 48, 96));
            string title = sequence.Active ? sequence.ToIsland ? "Плывём к острову..." : "Возвращаемся домой..." : "Загрузка мира...";
            Utils.DrawBorderString(Main.spriteBatch, title, new Vector2(size.X / 2, panel.Y + 12), Color.White, 0.9f, 0.5f);
            float textScale = Math.Min(0.8f, (width - 32) / Math.Max(1, FontAssets.MouseText.Value.MeasureString(Main.statusText).X));
            Utils.DrawBorderString(Main.spriteBatch, Main.statusText, new Vector2(size.X / 2, panel.Y + 44), Color.White, textScale, 0.5f);
            Rectangle bar = new Rectangle(panel.X + 20, panel.Bottom - 23, width - 40, 7);
            Box(Main.spriteBatch, bar, new Color(11, 18, 42));
            double progress = WorldGenerator.CurrentGenerationProgress?.TotalProgress ?? -1;
            if (double.IsFinite(progress) && progress >= 0)
                Box(Main.spriteBatch, new Rectangle(bar.X, bar.Y, (int)(bar.Width * Math.Clamp(progress, 0, 1)), bar.Height), new Color(136, 185, 214));
            else
            {
                int segment = bar.Width / 4;
                int offset = (int)(Main.GlobalTimeWrappedHourly * 90) % Math.Max(1, bar.Width - segment);
                Box(Main.spriteBatch, new Rectangle(bar.X + offset, bar.Y, segment, bar.Height), new Color(136, 185, 214));
            }
        }

        private static void Box(SpriteBatch batch, Rectangle rectangle, Color color)
            => batch.Draw(TextureAssets.MagicPixel.Value, rectangle, new Rectangle(0, 0, 1, 1), color);
    }
}
