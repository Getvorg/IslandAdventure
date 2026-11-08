using System;
using System.Collections.Generic;
using IslandAdventure.Subworlds;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Default;

namespace IslandAdventure.Systems
{
    public class AnglerCompanionSystem : ModSystem
    {
        private TagCompound profile;
        private bool arriving;
        private bool relocate;
        private bool detached;
        private NPC detachedNpc;
        public bool WorldIsIsland { get; private set; }
        public bool Known => profile != null;

        public override void OnWorldLoad()
        {
            profile = null;
            arriving = relocate = detached = false;
            detachedNpc = null;
            WorldIsIsland = SubworldSystem.IsActive<IslandSubworld>();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (!detached) CapturePresent();
            if (profile != null) tag["companion"] = profile;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (!tag.ContainsKey("companion")) return;
            profile = tag.GetCompound("companion");
            arriving = true;
            NPC.savedAngler = true;
        }

        private NPC FindKeeper()
        {
            NPC first = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.type != NPCID.Angler) continue;
                if (profile != null && npc.GivenName == profile.GetString("name")) return npc;
                first ??= npc;
            }
            return first;
        }

        public bool CanBoard(bool toIsland)
        {
            CapturePresent();
            if (profile != null || !toIsland) return true;
            Main.NewText("Сначала найди и разбуди Рыбака: теперь в лодке путешествует настоящий NPC.", Color.Wheat);
            return false;
        }

        private void CapturePresent()
        {
            NPC npc = FindKeeper();
            if (npc == null) return;
            TagCompound previous = profile;
            var globals = new List<TagCompound>();
            foreach (GlobalNPC global in npc.Globals)
            {
                // tML's unloaded-data placeholder deliberately throws from SaveData.
                if (global is UnloadedGlobalNPC) continue;
                TagCompound data = new TagCompound();
                global.SaveData(npc, data);
                if (data.Count > 0)
                    globals.Add(new TagCompound { ["mod"] = global.Mod.Name, ["name"] = global.Name, ["data"] = data });
            }
            profile = new TagCompound
            {
                ["name"] = npc.GivenName, ["life"] = npc.life,
                ["variation"] = npc.townNpcVariationIndex,
                ["shimmered"] = NPC.ShimmeredTownNPCs[NPCID.Angler],
                ["invulnerable"] = npc.dontTakeDamage,
                ["x"] = npc.position.X, ["y"] = npc.position.Y,
                ["globals"] = globals
            };
            if (!WorldIsIsland)
            {
                profile["homeX"] = npc.homeTileX; profile["homeY"] = npc.homeTileY;
                profile["homeless"] = npc.homeless;
            }
            else if (previous != null)
            {
                profile["homeX"] = previous.GetInt("homeX"); profile["homeY"] = previous.GetInt("homeY");
                profile["homeless"] = previous.GetBool("homeless");
            }
            else profile["homeless"] = true;
            NPC.savedAngler = true;
        }

        public TagCompound Export()
        {
            if (!detached) CapturePresent();
            return profile == null ? null : (TagCompound)profile.Clone();
        }

        public void Import(TagCompound data)
        {
            if (data == null || !data.ContainsKey("name")) return;
            profile = (TagCompound)data.Clone();
            arriving = relocate = true;
            detached = false;
            detachedNpc = null;
            NPC.savedAngler = true;
        }

        public void DetachForTransfer()
        {
            if (detached) return;
            CapturePresent();
            detachedNpc = FindKeeper();
            if (profile == null) return;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && (npc.type == NPCID.Angler || npc.type == NPCID.SleepingAngler)) npc.active = false;
            }
            detached = true;
            NPC.savedAngler = true;
        }

        public void RestoreRejectedDeparture()
        {
            if (!detached || Main.gameMenu) return;
            detached = false;
            if (detachedNpc != null && ReferenceEquals(Main.npc[detachedNpc.whoAmI], detachedNpc))
                detachedNpc.active = true;
            else arriving = true;
            detachedNpc = null;
        }

        public void Arrive()
        {
            if (profile == null) return;
            detached = false;
            arriving = relocate = true;
            Reconcile();
        }

        public override void PostUpdateNPCs()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || Main.gameMenu || WorldGen.generatingWorld || detached) return;
            Reconcile();
        }

        public void Reconcile()
        {
            NPC keeper = FindKeeper();
            if (keeper == null && arriving && profile != null)
            {
                Vector2 point = Main.LocalPlayer.position;
                int index = NPC.NewNPC(new EntitySource_Misc("IslandAdventure:AnglerArrival"),
                    (int)point.X, (int)point.Y, NPCID.Angler);
                if (index >= Main.maxNPCs) return;
                keeper = Main.npc[index];
                relocate = true;
            }
            if (keeper != null && arriving && profile != null)
            {
                keeper.GivenName = profile.GetString("name");
                keeper.life = Math.Clamp(profile.GetInt("life"), 1, keeper.lifeMax);
                keeper.townNpcVariationIndex = profile.GetInt("variation");
                keeper.dontTakeDamage = profile.GetBool("invulnerable");
                NPC.ShimmeredTownNPCs[NPCID.Angler] = profile.GetBool("shimmered");
                keeper.homeless = WorldIsIsland || profile.GetBool("homeless");
                keeper.homeTileX = profile.GetInt("homeX"); keeper.homeTileY = profile.GetInt("homeY");
                if (relocate) PlaceAtPier(keeper);
                foreach (TagCompound saved in profile.GetList<TagCompound>("globals"))
                    foreach (GlobalNPC global in keeper.Globals)
                        if (global.Mod.Name == saved.GetString("mod") && global.Name == saved.GetString("name"))
                            global.LoadData(keeper, saved.GetCompound("data"));
                keeper.netUpdate = true;
                arriving = relocate = false;
            }
            if (keeper != null)
            {
                NPC.savedAngler = true;
                // The sleeping copy may already be present in older island files.
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc != keeper && (npc.type == NPCID.Angler || npc.type == NPCID.SleepingAngler))
                        npc.active = false;
                }
                if (profile == null) CapturePresent();
            }
            else if (WorldIsIsland)
                for (int i = 0; i < Main.maxNPCs; i++)
                    if (Main.npc[i].active && Main.npc[i].type == NPCID.SleepingAngler) Main.npc[i].active = false;
        }

        private void PlaceAtPier(NPC npc)
        {
            HarborSystem harbor = ModContent.GetInstance<HarborSystem>();
            harbor.DiscoverExistingPier();
            if (harbor.HasHarbor)
                foreach (int offset in new[] { 2, -2, 3, -3, 0, 4, -4 })
                {
                    int x = harbor.Landing.X + offset, y = harbor.Landing.Y;
                    Vector2 point = new Vector2(x * 16 + 8 - npc.width / 2f, y * 16 - npc.height);
                    if (!WorldGen.SolidTile(x, y) || Collision.SolidCollision(point, npc.width, npc.height)) continue;
                    npc.position = point; npc.velocity = Vector2.Zero;
                    if (WorldIsIsland) { npc.homeTileX = x; npc.homeTileY = y - 1; }
                    return;
                }
            npc.position = Main.LocalPlayer.position;
            npc.velocity = Vector2.Zero;
        }

        public void ForgetDeadAngler(NPC npc)
        {
            if (profile != null && npc.GivenName == profile.GetString("name"))
            {
                profile = null;
                arriving = relocate = false;
            }
        }
    }
}
