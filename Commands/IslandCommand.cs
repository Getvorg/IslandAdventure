using IslandAdventure.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace IslandAdventure.Commands
{
    public class IslandCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "island";
        public override string Usage => "/island";
        public override string Description => "Отправиться на тестовый остров (одиночная игра).";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                caller.Reply("Пока путешествия доступны только в одиночной игре.");
                return;
            }

            if (SubworldSystem.AnyActive())
            {
                caller.Reply("Сначала вернись в основной мир. С нашего острова: /home.");
                return;
            }

            if (caller.Player.dead)
            {
                caller.Reply("Дождись возрождения перед путешествием.");
                return;
            }

            if (!SubworldSystem.Enter<IslandSubworld>())
                caller.Reply("Не удалось начать переход. Дождись окончания текущего перехода.");
        }
    }
}
