public partial class MainSceneManager : Node2D
{
	public bool GamePaused = false;
	public static MainSceneManager GlobalMainSceneManager;

	private CollisionObject2D some;
	public override void _Ready()
	{
		some = GetNode<CollisionObject2D>("Wolf");
		
		
		// if (GlobalMainSceneManager == null)
		// {
		 	GlobalMainSceneManager = this;
		// }
		// else
		// {
		// 	QueueFree();
		// }
	}
	
	
	public override void _Process(double delta)
	{
		
	}
}
