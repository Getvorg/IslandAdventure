using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.NPCs;

namespace IslandAdventure.Commands
{
    public class MermaidCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "mermaid";
        public override string Usage => "/mermaid [blue|red]";
        public override string Description => "Призвать русалку в воде рядом для проверки";
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length > 1 || args.Length == 1 && args[0] != "blue" && args[0] != "red")
            { caller.Reply("Используй /mermaid, /mermaid blue или /mermaid red.", Color.Wheat); return; }
            if (Main.netMode != NetmodeID.SinglePlayer)
            { caller.Reply("Проверочная команда доступна только в одиночной игре.", Color.Wheat); return; }
            if (NPC.CountNPCS(ModContent.NPCType<Mermaid>()) >= 3)
            { caller.Reply("Рядом уже достаточно русалок для проверки.", Color.Wheat); return; }
            Point center = caller.Player.Center.ToTileCoordinates();
            for (int radius = 6; radius <= 18; radius += 2)
                for (int dy = -radius; dy <= radius; dy += 2)
                    foreach (int sign in new[] { caller.Player.direction, -caller.Player.direction })
                    {
                        int x = center.X + radius * sign, y = center.Y + dy;
                        if (!WorldGen.InWorld(x, y, 10)) continue;
                        Vector2 point = new Vector2(x * 16, y * 16);
                        if (Collision.SolidCollision(point, 36, 44) || !Collision.WetCollision(point, 36, 44)) continue;
                        // Require water around both the head and tail, not a tiny puddle.
                        Tile top = Main.tile[x + 1, y], bottom = Main.tile[x + 1, y + 2];
                        if (top.LiquidType != LiquidID.Water || bottom.LiquidType != LiquidID.Water
                            || top.LiquidAmount < 200 || bottom.LiquidAmount < 200) continue;
                        int index = NPC.NewNPC(new EntitySource_Misc("IslandAdventure:MermaidPreview"),
                            x * 16 + 18, y * 16 + 44, ModContent.NPCType<Mermaid>());
                        if (index < Main.maxNPCs)
                        {
                            NPC mermaid = Main.npc[index];
                            if (args.Length == 1) mermaid.ai[3] = args[0] == "red" ? 1 : 0;
                            mermaid.netUpdate = true;
                            bool red = mermaid.ai[3] == 1;
                            caller.Reply(red ? "Красная русалка появилась в воде рядом." : "Бирюзовая русалка появилась в воде рядом.",
                                red ? Color.Salmon : Color.LightSeaGreen);
                        }
                        else caller.Reply("Не удалось призвать русалку: достигнут лимит существ.", Color.Wheat);
                        return;
                    }
            caller.Reply("Подойди к достаточно глубокому водоёму и повтори /mermaid.", Color.Wheat);
        }
    }
}
