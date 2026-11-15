using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using IslandAdventure.Systems;

namespace IslandAdventure
{
	public class IslandAdventure : Mod
	{
		public override void HandlePacket(BinaryReader reader,int whoAmI)
		{
			byte kind=reader.ReadByte();
			if(kind!=1)return;
			int x=reader.ReadInt32(),floor=reader.ReadInt32();bool add=reader.ReadBoolean();
			if(Main.netMode==NetmodeID.MultiplayerClient)ModContent.GetInstance<BunaTreeRegistry>().ApplyRemote(x,floor,add);
		}
	}
}
