namespace GameSystem
{
	public class QuitMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			QuitMsg msg = message as QuitMsg;	
		}
	}
}
