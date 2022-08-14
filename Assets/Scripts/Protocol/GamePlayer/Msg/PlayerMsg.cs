using System;
using System.Collections.Generic;
using System.Text;
namespace GamePlayer
{
	public class PlayerMsg : BaseMsg
	{
		public int playerID;
		public PlayerData data;
		public override int GetBytesNum()
		{
			int num = 8;
			num += 4;
			num += data.GetBytesNum();
			return num;
		}
		public override byte[] Writing()
		{
			int index = 0;
			byte[] bytes = new byte[GetBytesNum()];
			WriteInt(bytes, GetID(), ref index);
			WriteInt(bytes, bytes.Length - 8, ref index);
			WriteInt(bytes, playerID, ref index);
			WriteData(bytes, data, ref index);
			return bytes;
		}
		public override int Reading(byte[] bytes, int beginIndex = 0)
		{
			int index = beginIndex;
			playerID = ReadInt(bytes, ref index);
			data = ReadData<PlayerData>(bytes, ref index);
			return index - beginIndex;
		}
		public override int GetID()
		{
			return 1001;
		}
	}
}