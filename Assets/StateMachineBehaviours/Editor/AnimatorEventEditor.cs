using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimatorEvent))]
public class AnimatorEventEditor : Editor {
	private SerializedProperty events;
	private Dictionary<string, bool> hierarchyExpanded = new Dictionary<string, bool>();
	List<int> sortedEventIndices = new List<int>();
	string filterString = "";

	private void InitializeIfNeeded() {
		if (events != null && sortedEventIndices.Count == ((AnimatorEvent) target).events.Length) return;
		events = serializedObject.FindProperty("events");
		CalculatedSortedIndices();
	}

	private void OnEnable() {
		Undo.undoRedoPerformed += MyUndoCallback;
	}

	private void OnDisable() {
		Undo.undoRedoPerformed -= MyUndoCallback;
	}

	private void MyUndoCallback() {
		CalculatedSortedIndices();
	}

	public override void OnInspectorGUI() {
		//DrawDefaultInspector();

		serializedObject.Update();

		InitializeIfNeeded();

		filterString = EditorGUILayout.TextField(new GUIContent("Search"), filterString).Trim();

		if (Application.isPlaying) GUI.enabled = false;

		if (GUILayout.Button("Add")) {
			Undo.RecordObject(target, "Added event");
			List<AnimatorEvent.EventElement> evs = new List<AnimatorEvent.EventElement>(((AnimatorEvent) target).events);
			evs.Add(new AnimatorEvent.EventElement() {
				id = Random.Range(int.MinValue, int.MaxValue),
				name = ""
			});
			((AnimatorEvent) target).events = evs.ToArray();
			serializedObject.Update();
			CalculatedSortedIndices();
		}

		int j = 0;
		RecursiveDrawing(ref j, "");

		serializedObject.ApplyModifiedProperties();

		if (Application.isPlaying) GUI.enabled = true;
	}

	void RecursiveDrawing(ref int i, string start) {
		if (start.Length == 0 || (hierarchyExpanded[start] = EditorGUILayout.Foldout(Get(start), start, true))) {
			EditorGUI.indentLevel++;
			for (; i < events.arraySize;) {
				var property = events.GetArrayElementAtIndex(sortedEventIndices[i]);
				var name = property.FindPropertyRelative("name").stringValue;
				if (!name.StartsWith(start)) {
					break;
				}
				var barPos = start.Length < name.Length ? name.IndexOf('/', start.Length + 1) : -1;
				if (barPos == -1) {
					if (filterString.Length == 0 || name.ToLowerInvariant().Contains(filterString.ToLowerInvariant())) {
						if (hierarchyExpanded[name] = EditorGUILayout.Foldout(Get(name), name, true)) {
							EditorGUILayout.BeginVertical("box");
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.BeginHorizontal();
							var prevLabelWidth = EditorGUIUtility.labelWidth;
							EditorGUIUtility.labelWidth = 60;
							EditorGUILayout.DelayedTextField(property.FindPropertyRelative("name"));
							bool remove = GUILayout.Button("Remove", GUILayout.Width(100));
							EditorGUILayout.EndHorizontal();
							if (EditorGUI.EndChangeCheck()) {
								CalculatedSortedIndices();
							}
							EditorGUILayout.PropertyField(property.FindPropertyRelative("action"), true);
							GUI.skin.label.alignment = TextAnchor.UpperRight;
							EditorGUILayout.BeginHorizontal();
							GUILayout.Label(""); // align this to the right, then the property will also be aligned to the rigth. Hack because idk how to align the proprety to the right
							EditorGUIUtility.labelWidth = 40;
							EditorGUILayout.PropertyField(property.FindPropertyRelative("id"), new GUIContent("ID ", "ID of the event. Change it manually only if needed."), GUILayout.Width(140));
							EditorGUIUtility.labelWidth = prevLabelWidth;
							EditorGUILayout.EndHorizontal();
							GUI.skin.label.alignment = TextAnchor.UpperLeft;
							EditorGUILayout.EndVertical();

							if (remove) {
								events.DeleteArrayElementAtIndex(sortedEventIndices[i]);
								CalculatedSortedIndices();
							}
						}
					}
					i++;
				}
				else {
					RecursiveDrawing(ref i, name.Substring(0, barPos + 1));
				}
			}
			EditorGUI.indentLevel--;
		}
		else {
			for (; i < events.arraySize;) {
				var property = events.GetArrayElementAtIndex(sortedEventIndices[i]);
				var name = property.FindPropertyRelative("name").stringValue;
				if (!name.StartsWith(start)) {
					break;
				}
				i++;
			}
		}
	}

	void CalculatedSortedIndices() {
		sortedEventIndices.Clear();
		events = serializedObject.FindProperty("events");
		for (int i = 0; i < events.arraySize; i++) {
			var eventsI = events.GetArrayElementAtIndex(i);
			if (eventsI.FindPropertyRelative("id").intValue == 0) {
				eventsI.FindPropertyRelative("id").intValue = Animator.StringToHash(eventsI.FindPropertyRelative("name").stringValue);
			}
			sortedEventIndices.Add(i);
		}
		sortedEventIndices.Sort((a, b) => events.GetArrayElementAtIndex(a).FindPropertyRelative("name").stringValue.CompareTo(events.GetArrayElementAtIndex(b).FindPropertyRelative("name").stringValue));
	}

	bool Get(string text) {
		if (!hierarchyExpanded.TryGetValue(text, out var state)) {
			hierarchyExpanded.Add(text, false);
		}
		return state;
	}
}
