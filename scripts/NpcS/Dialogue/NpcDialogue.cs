using System.Diagnostics;

public class NpcDialogue
{
	public int Index;
	public List<InterfaceSelectionObject> InterfaceSelectionObjects;
	public List<NpcDialogue> NpcDialogues;
	public string DisplayText;
	public bool isToDelete { get; set; }

	public NpcDialogue(List<InterfaceSelectionObject> interfaceSelectionObjects, string displayText,  int index, bool toDelete, List<NpcDialogue> npcDialogues = null)
	{
		isToDelete = toDelete;
		Index = index;
		InterfaceSelectionObjects = interfaceSelectionObjects;
		if (npcDialogues != null)
		{
			NpcDialogues = npcDialogues;
		}
		DisplayText = displayText;
	}
}
