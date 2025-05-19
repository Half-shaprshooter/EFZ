public partial class Quest : Area2D
{
	public static Vector2 globalPos { get; set; }
	[Export] public String questText { get; set; }
	[Export] public Quest nextQuest { get; set; }
	public override void _Ready()
	{
		globalPos = this.GlobalPosition;
	}
	
	public override void _Process(double delta)
	{
		globalPos = this.GlobalPosition;
	}
}
