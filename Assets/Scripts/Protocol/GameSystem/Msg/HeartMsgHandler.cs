namespace GameSystem
{
	public class HeartMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			HeartMsg msg = message as HeartMsg;	
		}
	}
}
