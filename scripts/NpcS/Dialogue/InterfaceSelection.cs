public partial class InterfaceSelection : Control
{
	public bool Selected;
	public InterfaceSelectionObject interfaceSelectionObject;
	public override void _Ready()
	{
		this.GetNode<Label>("Label").Text = interfaceSelectionObject.SelectionText;
		GetNode<TextureRect>("Selected").Visible = false;
	}

	public void SetSelected(bool selected)
	{
		this.Selected = selected;
		if (selected)
		{
			GetNode<TextureRect>("Selected").Visible = true;
		}
		else
		{
			GetNode<TextureRect>("Selected").Visible = false;
		}
	}
}
