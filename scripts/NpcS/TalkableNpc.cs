public partial class TalkableNpc : NpcObject
{
	protected string NpcName;

	public TalkableNpc()
	{
		var a = this.GetNode<HostImpl>("HostImpl");
		a._host = Host.Friendly;
		// GD.Print("Friendly and Neutral is: " + a._host);
	}
	
	protected void AttackPlayer()
	{
		//Реализация атаки
	}
}
