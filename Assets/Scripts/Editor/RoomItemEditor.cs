using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomItem))]
public class RoomItemEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		RoomItem editedItem = (RoomItem)target;
		if (editedItem != null)
		{
			if ( (System.Guid)editedItem.ItemId == System.Guid.Empty )
            {
				editedItem.ItemId = System.Guid.NewGuid();
            }
		}
	}
}

