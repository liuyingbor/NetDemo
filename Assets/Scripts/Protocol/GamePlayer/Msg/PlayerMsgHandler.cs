namespace GamePlayer
{
	public class PlayerMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			PlayerMsg msg = message as PlayerMsg;	
			//处理消息逻辑
		}
	}
}
