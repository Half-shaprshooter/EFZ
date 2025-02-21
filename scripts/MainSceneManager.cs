public partial class MainSceneManager : Node2D
{
	public bool GamePaused = false;
	public static MainSceneManager GlobalMainSceneManager;
	public override void _Ready()
	{
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
