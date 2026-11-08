using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IslandAdventure.NPCs
{
    public class AnglerTravelNPC : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Angler;

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
            => !BoatTravelSystem.IsWorldScene;

        public override void GetChat(NPC npc, ref string chat)
        {
            if (!SubworldSystem.AnyActive())
                chat += "\n\nУ левого причала ждёт моя лодка. Могу отвезти тебя на другой остров!";
        }
    }
}
