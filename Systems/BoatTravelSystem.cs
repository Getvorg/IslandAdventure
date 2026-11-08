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
using ReLogic.Content;

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
        private static Player passenger;
        private static bool daylight;
        private static int moonPhase;
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
            daylight = Main.dayTime;
            moonPhase = Main.moonPhase;
            try
            {
                // The same preview path used by Terraria's character-selection UI.
                passenger = Main.LocalPlayer.SerializedClone();
                passenger.active = true;
                passenger.dead = false;
                passenger.direction = toIsland ? -1 : 1;
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
                passenger = null;
            }
        }

        public override void PreSaveAndQuit() { sequence.Reset(); reveal = 0; sourceWorld = mainWorld = Guid.Empty; passenger = null; }
        public override void Unload() { sequence.Reset(); reveal = 0; sourceWorld = mainWorld = Guid.Empty; passenger = null; BoatArt.Unload(); }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || Main.gameMenu) return;
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            if (harbor.HasHarbor)
                Lighting.AddLight(harbor.BoatPosition + new Vector2(harbor.FacesLeft ? -54 : 54, -26), 0.45f, 0.3f, 0.12f);
        }

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
            Color light = Color.Lerp(Lighting.GetColor(harbor.Boat.X, harbor.Boat.Y - 1), Color.White, 0.15f);
            BoatArt.Draw(Main.spriteBatch, position, 1f, harbor.FacesLeft, Main.GlobalTimeWrappedHourly, lighting: light);
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
                Rectangle boatBounds = new Rectangle((int)harbor.BoatPosition.X - 68, (int)harbor.BoatPosition.Y - 58, 136, 78);
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
            if (sequence.Active) DrawScene(Main.spriteBatch, true);
            else Utils.DrawBorderString(Main.spriteBatch, Main.statusText, Canvas / 2, Color.White, 1, 0.5f);
        }

        private static void DrawScene(SpriteBatch batch, bool loading)
        {
            Vector2 size = Canvas;
            float time = Main.GlobalTimeWrappedHourly;
            float horizon = size.Y * 0.49f;
            float sea = size.Y * 0.73f;
            float scale = size.X >= 900 ? 2 : 1;
            bool left = sequence.ToIsland;
            float xBoat = size.X / 2;
            if (!loading)
            {
                float duration = sequence.Phase == TravelPhase.Departing
                    ? TravelSequence.DepartureSeconds : TravelSequence.ArrivalSeconds;
                float p = MathHelper.SmoothStep(0, 1, sequence.Elapsed / duration);
                xBoat = sequence.Phase == TravelPhase.Departing
                    ? MathHelper.Lerp(size.X / 2, left ? -170 * scale : size.X + 170 * scale, p)
                    : MathHelper.Lerp(left ? size.X + 170 * scale : -170 * scale, size.X / 2, p);
            }
            // Batch the tiled water and pier before drawing shader-equipped actors.
            batch.End();
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            DrawSky(batch, size, horizon, time);
            DrawSea(batch, size, horizon, scale, time);
            if (!loading)
                DrawPier(batch, size, sea, scale, sequence.Phase == TravelPhase.Departing ? left : !left);
            for (int ripple = 0; ripple < 7; ripple++)
            {
                float p = (time * 0.65f + ripple / 7f) % 1;
                float wakeX = xBoat + (left ? 1 : -1) * (46 + p * 47) * scale;
                float wakeY = sea + (2 + (float)Math.Sin(p * 9 + ripple)) * scale;
                Box(batch, new Rectangle((int)wakeX, (int)wakeY, (int)((9 - p * 6) * scale), (int)scale),
                    new Color(172, 211, 239) * ((1 - p) * 0.6f));
            }
            batch.End();
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            Vector2 boat = new Vector2((float)Math.Round(xBoat), sea);
            float bob = (float)Math.Round(Math.Sin(time * 2.2f) * scale);
            DrawPassengers(batch, boat + new Vector2(0, -8 * scale + bob), scale, left);
            Color boatLight = daylight ? Color.White : new Color(155, 178, 216);
            BoatArt.Draw(batch, boat, scale, left, time, rowing: true, lighting: boatLight);
            Box(batch, new Rectangle((int)(xBoat - 49 * scale), (int)(sea + 4 * scale), (int)(99 * scale), (int)(4 * scale)),
                new Color(32, 93, 171) * 0.35f);
            string title = sequence.ToIsland ? "К новому острову" : "Обратно домой";
            Utils.DrawBorderString(batch, title, new Vector2(size.X / 2, 22), Color.White, 1, 0.5f);
            if (loading)
            {
                int width = Math.Min(600, (int)size.X - 32);
                Rectangle panel = new Rectangle(((int)size.X - width) / 2, (int)size.Y - 90, width, 58);
                Utils.DrawInvBG(batch, panel, new Color(37, 48, 96) * 0.9f);
                float textScale = Math.Min(0.8f, (width - 32) / Math.Max(1, FontAssets.MouseText.Value.MeasureString(Main.statusText).X));
                Utils.DrawBorderString(batch, Main.statusText, new Vector2(size.X / 2, panel.Y + 18), Color.White, textScale, 0.5f);
            }
            else
            {
                float fade = sequence.Phase == TravelPhase.Departing
                    ? MathHelper.Clamp((sequence.Elapsed - TravelSequence.DepartureSeconds + 0.65f) / 0.65f, 0, 1)
                    : Math.Max(1 - MathHelper.Clamp(sequence.Elapsed / 0.6f, 0, 1),
                        MathHelper.Clamp((sequence.Elapsed - TravelSequence.ArrivalSeconds + 0.45f) / 0.45f, 0, 1));
                Box(batch, new Rectangle(0, 0, (int)size.X, (int)size.Y), Color.Black * fade);
            }
        }

        private static Texture2D Vanilla(Asset<Texture2D> asset)
            => asset.IsLoaded ? asset.Value : Main.Assets.Request<Texture2D>(asset.Name, AssetRequestMode.ImmediateLoad).Value;

        private static void DrawSky(SpriteBatch batch, Vector2 size, float horizon, float time)
        {
            Color top = daylight ? new Color(63, 112, 224) : new Color(9, 14, 40);
            Color bottom = daylight ? new Color(143, 182, 240) : new Color(38, 49, 89);
            for (int row = 0; row < 40; row++)
            {
                float t = row / 39f;
                Box(batch, new Rectangle(0, (int)(horizon * row / 40), (int)size.X, (int)(horizon / 40) + 2), Color.Lerp(top, bottom, t));
            }
            Texture2D skyObject = daylight ? Vanilla(TextureAssets.Sun) : Vanilla(TextureAssets.Moon[0]);
            int height = daylight ? skyObject.Height : skyObject.Height / 8;
            Rectangle frame = new Rectangle(0, daylight ? 0 : height * (moonPhase % 8), skyObject.Width, height);
            batch.Draw(skyObject, new Vector2((int)(size.X * 0.76f), (int)(horizon * 0.25f)), frame,
                Color.White, 0, new Vector2(skyObject.Width / 2f, height / 2f), 1, SpriteEffects.None, 0);
            if (!daylight)
                for (int star = 0; star < 50; star++)
                {
                    int x = (int)((star * 137.3f + 31) % size.X);
                    int y = (int)((star * 47.7f + 11) % (horizon - 20));
                    Box(batch, new Rectangle(x, y, star % 3 == 0 ? 2 : 1, 2), Color.White * (0.45f + 0.2f * (float)Math.Sin(time + star)));
                }
            for (int cloud = 0; cloud < 7; cloud++)
            {
                Texture2D image = Vanilla(TextureAssets.Cloud[(cloud * 3) % 12]);
                float x = (cloud * 311 + time * (3 + cloud % 3)) % (size.X + 260) - 160;
                float y = 35 + cloud % 3 * (horizon - 100) / 3;
                batch.Draw(image, new Vector2((int)x, (int)y), null,
                    daylight ? Color.White * 0.8f : new Color(90, 105, 146) * 0.6f,
                    0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
        }

        private static void DrawSea(SpriteBatch batch, Vector2 size, float horizon, float scale, float time)
        {
            Texture2D water = Vanilla(TextureAssets.Liquid[0]);
            int frameY = ((int)(time * 7) % Math.Max(1, water.Height / 18)) * 18;
            frameY = Math.Min(frameY, Math.Max(0, water.Height - 16));
            Rectangle source = new Rectangle(0, frameY, Math.Min(16, water.Width), Math.Min(16, water.Height));
            int tile = (int)(16 * scale);
            for (int y = (int)horizon; y < size.Y; y += tile)
            {
                float depth = (y - horizon) / Math.Max(1, size.Y - horizon);
                Color tint = Color.Lerp(daylight ? new Color(135, 174, 235) : new Color(65, 86, 143),
                    daylight ? new Color(43, 70, 145) : new Color(22, 31, 70), depth);
                Box(batch, new Rectangle(0, y, (int)size.X, tile), Color.Lerp(new Color(37, 91, 197), new Color(10, 24, 79), depth));
                for (int x = 0; x < size.X; x += tile)
                    batch.Draw(water, new Rectangle(x, y, tile, tile), source, tint * 0.72f);
            }
            for (int wave = -1; wave < (int)size.X / 32 + 1; wave++)
            {
                int x = wave * 32;
                int y = (int)horizon + (int)(Math.Sin(wave * 0.9 + time * 1.5) * scale);
                Box(batch, new Rectangle(x, y, 18, (int)Math.Max(1, scale)), new Color(133, 181, 231) * 0.8f);
            }
        }

        private static void DrawPier(SpriteBatch batch, Vector2 size, float sea, float scale, bool right)
        {
            Main.instance.LoadTiles(TileID.PalmWood);
            Main.instance.LoadTiles(TileID.WoodenBeam);
            Texture2D planks = TextureAssets.Tile[TileID.PalmWood].Value;
            Texture2D beam = TextureAssets.Tile[TileID.WoodenBeam].Value;
            int tile = (int)(16 * scale);
            int edge = (int)(size.X * (right ? 0.65f : 0.35f));
            int deckY = (int)(sea - 27 * scale);
            int start = right ? edge : 0;
            int end = right ? (int)size.X + tile : edge;
            Rectangle plankFrame = new Rectangle(18, 18, 16, 16);
            Rectangle beamFrame = new Rectangle(0, 0, 16, 16);
            for (int x = start; x < end; x += tile)
            {
                batch.Draw(planks, new Rectangle(x, deckY, tile, tile), plankFrame, Color.White);
                if ((x - start) / tile % 3 == 0)
                    for (int y = deckY + tile; y < size.Y; y += tile)
                        batch.Draw(beam, new Rectangle(x, y, tile, tile), beamFrame,
                            y > sea ? new Color(93, 121, 171) : Color.White);
            }
            int postX = right ? edge + tile : edge - tile * 2;
            for (int y = deckY - tile * 4; y < deckY; y += tile)
                batch.Draw(beam, new Rectangle(postX, y, tile, tile), beamFrame, Color.White);
            for (int x = right ? edge : edge - tile * 2; x < (right ? edge + tile * 2 : edge); x += tile)
                batch.Draw(planks, new Rectangle(x, deckY - tile * 4, tile, tile), plankFrame, Color.White);
            BoatArt.DrawItem(batch, ItemID.PalmWoodLantern,
                new Vector2(right ? edge + tile / 2f : edge - tile / 2f, deckY - tile), scale, Color.White);
            BoatArt.DrawItem(batch, ItemID.WoodenCrate,
                new Vector2(right ? edge + 65 * scale : edge - 65 * scale, deckY), scale * 0.65f, Color.White);
        }

        private static void DrawPassengers(SpriteBatch batch, Vector2 deck, float scale, bool left)
        {
            if (passenger != null)
            {
                using (new Main.CurrentPlayerOverride(passenger))
                {
                    passenger.direction = left ? -1 : 1;
                    passenger.bodyFrame.Y = passenger.legFrame.Y = passenger.headFrame.Y = 0;
                    Vector2 feet = deck + new Vector2(-18 * scale, 0);
                    Vector2 position = feet - new Vector2(passenger.width / 2f, passenger.height * (scale + 1) / 2f);
                    Main.PlayerRenderer.DrawPlayer(Main.Camera, passenger, position + Main.screenPosition,
                        0, Vector2.Zero, 0, scale);
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
            }
            Main.instance.LoadNPC(NPCID.Angler);
            Texture2D npc = TextureAssets.Npc[NPCID.Angler].Value;
            int frameHeight = npc.Height / Main.npcFrameCount[NPCID.Angler];
            batch.Draw(npc, deck + new Vector2(18 * scale, 0), new Rectangle(0, 0, npc.Width, frameHeight),
                daylight ? Color.White : new Color(179, 192, 225), 0, new Vector2(npc.Width / 2f, frameHeight), scale,
                left ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        private static void Box(SpriteBatch batch, Rectangle rectangle, Color color)
            => batch.Draw(TextureAssets.MagicPixel.Value, rectangle, new Rectangle(0, 0, 1, 1), color);
    }
}
