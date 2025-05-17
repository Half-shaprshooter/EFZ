using System.Diagnostics;

public class NpcDialogue
{
	public int Index;
	public List<InterfaceSelectionObject> InterfaceSelectionObjects;
	public List<NpcDialogue> NpcDialogues;
	public string DisplayText;

	public NpcDialogue(List<InterfaceSelectionObject> interfaceSelectionObjects, string displayText,  int index, bool isToDelete, List<NpcDialogue> npcDialogues = null)
	{
		Index = index;
		InterfaceSelectionObjects = interfaceSelectionObjects;
		if (npcDialogues != null)
		{
			NpcDialogues = npcDialogues;
		}
		DisplayText = displayText;
	}
}
