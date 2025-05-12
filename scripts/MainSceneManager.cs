public partial class MainSceneManager : Node2D
{
	//public bool GamePaused = false;
	public static MainSceneManager GlobalMainSceneManager;

	private AudioStreamPlayer2D audioPlayer;

	private AudioStream CAMP_MUSIC = (AudioStream)GD.Load("res://sounds/music/CampMusic.mp3");
	private AudioStream FOREST_MUSIC = (AudioStream)GD.Load("res://sounds/music/ForestMusic.mp3");
	private AudioStream CHURCH_MUSIC = (AudioStream)GD.Load("res://sounds/music/ChurchMusic.mp3");

	private CollisionObject2D some;

	[Export] private NavigationRegion2D NavReg2D;
	public override void _Ready()
	{
		audioPlayer = GetNode<AudioStreamPlayer2D>("/root/main/Player/AudioStreamPlayer2D");
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

	public void _on_new_bie_camp_body_entered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			audioPlayer.Stream = CAMP_MUSIC;
			audioPlayer.Play();
		}
	}

	public void _on_church_area_2d_body_entered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			GD.Print("Вашол");
			audioPlayer.Stream = CHURCH_MUSIC;
			audioPlayer.Play();
		}
	}
	
	public void _on_forest_body_entered(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			audioPlayer.Stream = FOREST_MUSIC;
			audioPlayer.Play();
		}
	}
}
