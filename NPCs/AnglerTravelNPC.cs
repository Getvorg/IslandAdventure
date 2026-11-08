using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace IslandAdventure.NPCs
{
    public class AnglerTravelNPC : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Angler;

        public override void GetChat(NPC npc, ref string chat)
        {
            if (!SubworldSystem.AnyActive())
                chat += "\n\nУ левого причала ждёт моя лодка. Могу отвезти тебя на другой остров!";
        }
    }
}
