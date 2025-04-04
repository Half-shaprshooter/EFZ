public partial class InterfaceManager : CanvasLayer
{
	public static DialogueManager dialogueManager;
	public override void _Ready()
	{
		dialogueManager = GetNode("DialogueManager") as DialogueManager;
		ProcessMode = ProcessModeEnum.Always;
	}
	public override void _Process(double delta)
	{
		
	}
}
