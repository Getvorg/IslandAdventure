using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.Items;
using IslandAdventure.Systems;

namespace IslandAdventure.Commands
{
    public class QueenCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "queen";
        public override string Usage => "/queen [item]";
        public override string Description => "Проверка Королевы Русалок: призыв у океана или получение раковины";
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            { caller.Reply("Первая версия боя доступна в одиночной игре.", Color.Wheat); return; }
            if (args.Length == 1 && args[0] == "item")
            {
                caller.Player.QuickSpawnItem(caller.Player.GetSource_Misc("QueenPreview"), ModContent.ItemType<RoyalConch>());
                caller.Reply("Королевская раковина выдана. Используй её у океана второго острова.", Color.LightPink);
                return;
            }
            if (args.Length != 0) { caller.Reply(Usage, Color.Wheat); return; }
            bool success = QueenEncounter.TrySummon(caller.Player, false, out string message);
            caller.Reply(message, success ? Color.LightPink : Color.Wheat);
        }
    }
}
