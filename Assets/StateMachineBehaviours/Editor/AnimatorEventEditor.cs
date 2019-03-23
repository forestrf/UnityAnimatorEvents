using Malee.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimatorEvent))]
public class AnimatorEventEditor : Editor {
	private ReorderableList list;
	private SerializedProperty tmpSecondaryList; 

	private void InitializeIfNeeded() {
		if (list != null) return;
		var l = serializedObject.FindProperty("events");

		list = new ReorderableList(l, true, true, true);
		list.onAddCallback += (list) => {
			list.AddItem();
		};
		list.onRemoveCallback += (list) => {
			list.RemoveItem(list.Index);
		};
		list.getElementNameCallback += (obj) => {
			return obj.FindPropertyRelative("name").stringValue;
		};
	}

	public override void OnInspectorGUI() {
		//DrawDefaultInspector();

		InitializeIfNeeded();

		if (Application.isPlaying) GUI.enabled = false;

		serializedObject.Update();

		list.filterString = EditorGUILayout.TextField(new GUIContent("Search"), list.filterString != null ? list.filterString : "").Trim();

		list.sortable =
		list.draggable = list.filterString == "";

		list.DoLayoutList();

		serializedObject.ApplyModifiedProperties();

		if (Application.isPlaying) GUI.enabled = true;
	}
}
