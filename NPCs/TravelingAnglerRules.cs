using IslandAdventure.Subworlds;
using IslandAdventure.Systems;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace IslandAdventure.NPCs
{
    public class TravelingAnglerRules : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
            => entity.type == NPCID.Angler || entity.type == NPCID.SleepingAngler;

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type == NPCID.SleepingAngler && (SubworldSystem.IsActive<IslandSubworld>()
                || ModContent.GetInstance<AnglerCompanionSystem>().Known)) npc.active = false;
            if (npc.type == NPCID.Angler && SubworldSystem.IsActive<IslandSubworld>())
                for (int i = 0; i < Main.maxNPCs; i++)
                    if (Main.npc[i] != npc && Main.npc[i].active && Main.npc[i].type == NPCID.Angler)
                    { npc.active = false; break; }
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.type != NPCID.SleepingAngler || !SubworldSystem.IsActive<IslandSubworld>()) return true;
            npc.active = false;
            return false;
        }

        public override void OnKill(NPC npc)
        {
            if (npc.type == NPCID.Angler) ModContent.GetInstance<AnglerCompanionSystem>().ForgetDeadAngler(npc);
        }
    }
}
