using System;
using System.Collections.Generic;
using IslandAdventure.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace IslandAdventure.Systems
{
    public class WorldTravelStateSystem : ModSystem
    {
        private const string BridgeKey = "IslandAdventure:SharedTravelState";
        private static TagCompound pending;
        private static Guid rootId;
        internal static bool HasPendingRestore => pending != null;

        public override void OnWorldLoad()
        {
            if (!SubworldSystem.IsActive<IslandSubworld>()) rootId = Main.ActiveWorldFileData.UniqueId;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (rootId != Guid.Empty) tag["rootId"] = rootId.ToString();
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (SubworldSystem.IsActive<IslandSubworld>() && Guid.TryParse(tag.GetString("rootId"), out Guid id)) rootId = id;
        }

        public static TagCompound Capture()
        {
            return new TagCompound
            {
                ["enabled"] = true,
                ["rootId"] = rootId.ToString(),
                ["time"] = Main.time, ["day"] = Main.dayTime, ["moon"] = Main.moonPhase,
                ["dawn"] = Main.fastForwardTimeToDawn, ["dusk"] = Main.fastForwardTimeToDusk,
                ["quest"] = Main.anglerQuest, ["questFinished"] = Main.anglerQuestFinished,
                ["questPlayers"] = new List<string>(Main.anglerWhoFinishedToday),
                ["savedAngler"] = NPC.savedAngler,
                ["angler"] = ModContent.GetInstance<AnglerCompanionSystem>().Export() ?? new TagCompound()
            };
        }

        public static void CopyToBridge()
        {
            bool ownDestination = SubworldSystem.IsActive<IslandSubworld>();
            bool returning = SubworldSystem.Current == null && ModContent.GetInstance<AnglerCompanionSystem>().WorldIsIsland;
            if (!ownDestination && !returning)
            {
                pending = null;
                SubworldSystem.CopyWorldData(BridgeKey, new TagCompound { ["enabled"] = false });
                return;
            }
            ModContent.GetInstance<AnglerCompanionSystem>().DetachForTransfer();
            if (!ModContent.GetInstance<AnglerCompanionSystem>().WorldIsIsland)
                rootId = Main.ActiveWorldFileData.UniqueId;
            pending = Capture();
            SubworldSystem.CopyWorldData(BridgeKey, pending);
        }

        public static void ReadBridge()
        {
            if (SubworldSystem.Current != null && !SubworldSystem.IsActive<IslandSubworld>()) return;
            TagCompound data = SubworldSystem.ReadCopiedWorldData<TagCompound>(BridgeKey);
            if (data == null || !data.GetBool("enabled")) return;
            if (SubworldSystem.Current == null && Guid.TryParse(data.GetString("rootId"), out Guid id)
                && id != Guid.Empty && id != Main.ActiveWorldFileData.UniqueId) return;
            QueueIncoming(data);
        }

        public static void QueueIncoming(TagCompound data)
        {
            pending = data;
            if (Guid.TryParse(data.GetString("rootId"), out Guid id)) rootId = id;
            Apply(data);
        }

        public static void Apply(TagCompound data)
        {
            if (data == null || !data.GetBool("enabled")) return;
            double time = data.GetDouble("time");
            Main.dayTime = data.GetBool("day");
            Main.time = double.IsFinite(time) ? Math.Clamp(time, 0, Main.dayTime ? 54000 : 32400) : 0;
            Main.moonPhase = Math.Clamp(data.GetInt("moon"), 0, 7);
            Main.fastForwardTimeToDawn = data.GetBool("dawn");
            Main.fastForwardTimeToDusk = data.GetBool("dusk");
            Main.anglerQuest = data.GetInt("quest");
            Main.anglerQuestFinished = data.GetBool("questFinished");
            Main.anglerWhoFinishedToday = new List<string>(data.GetList<string>("questPlayers"));
            NPC.savedAngler = data.GetBool("savedAngler");
            ModContent.GetInstance<AnglerCompanionSystem>().Import(data.GetCompound("angler"));
            Main.UpdateTimeRate();
        }

        public static void CompleteLoad()
        {
            // Fresh vanilla generation resets time/NPC flags after the first read.
            // Restore for load-time consumers; retain the snapshot until the
            // post-spawn pre-tick callback, after WorldFile restores its temps.
            if (pending == null) return;
            Apply(pending);
        }

        public override void PreUpdateWorld()
        {
            // Subworld Library calls WorldFile.SetOngoingToTemps AFTER OnLoad,
            // overwriting Main.time/dayTime/moonPhase with the destination file.
            // Restore once after spawning, before the first clock tick (not after it).
            if (Main.gameMenu || WorldGen.generatingWorld || pending == null) return;
            Apply(pending);
            pending = null;
        }

        public override void Unload() { pending = null; rootId = Guid.Empty; }
    }
}
