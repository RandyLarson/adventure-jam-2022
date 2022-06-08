//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(RoomItemKind))]
//public class RoomItemKindDrawer : PropertyDrawer
//{
//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//	{
//		// Using BeginProperty / EndProperty on the parent property means that
//		// prefab override logic works on the entire property.
//		EditorGUI.BeginProperty(position, GUIContent.none, property);

//		var amountRect = new Rect(position.x, position.y, 40, position.height);
//		var prop = property.FindPropertyRelative("Name");

////		EditorGUI.PropertyField(amountRect, prop, GUIContent.none);



//		var tagRect = new Rect(position.x, position.y, 100, 44);

//		GUIContent[] items = new GUIContent[]
//		{
//			new GUIContent("AA"),
//			new GUIContent("BB"),
//			new GUIContent("CC"),
//		};

//		EditorGUI.Popup(tagRect, 0, items);

//		prop.stringValue = EditorGUI.TagField(tagRect, prop.stringValue);

//		//int selected = 0;
//		//string[] options = new string[]
//		//{
//	 //"Option1", "Option2", "Option3",
//		//};
//		//selected = EditorGUILayout.Popup("Label", selected, options); 
		
//	//	EditorGUI.Drop(Rect position, string text, string[] dropDownElement);



//		EditorGUI.EndProperty();
//	}
//}

