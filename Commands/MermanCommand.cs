using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Content.NPCs;

namespace IslandAdventure.Commands
{
    public class MermanCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "merman";
        public override string Usage => "/merman [blue|red]";
        public override string Description => "Призвать русала в воде рядом для проверки";
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length > 1 || args.Length == 1 && args[0] != "blue" && args[0] != "red")
            { caller.Reply("Используй /merman, /merman blue или /merman red.", Color.Wheat); return; }
            if (Main.netMode != NetmodeID.SinglePlayer)
            { caller.Reply("Проверочная команда доступна только в одиночной игре.", Color.Wheat); return; }
            if (NPC.CountNPCS(ModContent.NPCType<Merman>()) >= 3)
            { caller.Reply("Рядом уже достаточно русалов для проверки.", Color.Wheat); return; }
            Point center = caller.Player.Center.ToTileCoordinates();
            for (int radius = 6; radius <= 18; radius += 2)
                for (int dy = -radius; dy <= radius; dy += 2)
                    foreach (int sign in new[] { caller.Player.direction, -caller.Player.direction })
                    {
                        int x = center.X + radius * sign, y = center.Y + dy;
                        if (!WorldGen.InWorld(x, y, 10)) continue;
                        Vector2 point = new Vector2(x * 16, y * 16);
                        if (Collision.SolidCollision(point, Merman.BodyWidth, Merman.BodyHeight) || !Collision.WetCollision(point, Merman.BodyWidth, Merman.BodyHeight)) continue;
                        // Require water around both the head and tail, not a tiny puddle.
                        Tile top = Main.tile[x + 1, y], bottom = Main.tile[x + 1, y + 3];
                        if (top.LiquidType != LiquidID.Water || bottom.LiquidType != LiquidID.Water
                            || top.LiquidAmount < 200 || bottom.LiquidAmount < 200) continue;
                        int index = NPC.NewNPC(new EntitySource_Misc("IslandAdventure:MermanPreview"),
                            x * 16 + Merman.BodyWidth / 2, y * 16 + Merman.BodyHeight, ModContent.NPCType<Merman>());
                        if (index < Main.maxNPCs)
                        {
                            NPC merman = Main.npc[index];
                            if (args.Length == 1) merman.ai[3] = args[0] == "red" ? 1 : 0;
                            merman.netUpdate = true;
                            bool red = merman.ai[3] == 1;
                            caller.Reply(red ? "Красный русал появился в воде рядом." : "Бирюзовый русал появился в воде рядом.",
                                red ? Color.Salmon : Color.LightSeaGreen);
                        }
                        else caller.Reply("Не удалось призвать русала: достигнут лимит существ.", Color.Wheat);
                        return;
                    }
            caller.Reply("Подойди к достаточно глубокому водоёму и повтори /merman.", Color.Wheat);
        }
    }
}
