using IslandAdventure.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace IslandAdventure.Commands
{
    public class HomeCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "home";
        public override string Usage => "/home";
        public override string Description => "Вернуться с тестового острова в основной мир.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                caller.Reply("Пока путешествия доступны только в одиночной игре.");
                return;
            }

            if (!SubworldSystem.IsActive<IslandSubworld>())
            {
                caller.Reply("Эта команда работает только на острове Island Adventure.");
                return;
            }

            SubworldSystem.Exit();
        }
    }
}
