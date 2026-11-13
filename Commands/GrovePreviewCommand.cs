using IslandAdventure.Systems;
using Terraria.ID;
using Terraria.ModLoader;
namespace IslandAdventure.Commands
{
    public class GrovePreviewCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "grovepreview";
        public override string Usage => "/grovepreview [back]";
        public override string Description => "Тестовый участок травы и растений священной рощи";
        public override void Action(CommandCaller caller,string input,string[] args)
        {
            if(Terraria.Main.netMode!=NetmodeID.SinglePlayer){caller.Reply("Только одиночная игра.");return;}
            var preview=ModContent.GetInstance<GrovePreviewSystem>();
            if(args.Length==1&&args[0]=="back"){caller.Reply(preview.Back(caller.Player)?"Возвращаемся.":"Нет сохранённой безопасной точки возврата.");return;}
            if(args.Length!=0){caller.Reply(Usage);return;}
            preview.Visit(caller.Player,out string message);caller.Reply(message);
        }
    }
}
