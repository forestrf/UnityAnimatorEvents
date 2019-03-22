using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(SetParamSMB))]
public class SetParamSMBEditor : Editor {
	private StateMachineBehaviourContext[] contexts;
	private AnimatorController animatorController;

	public override void OnInspectorGUI() {
		//serializedObject.ShowScriptInput();
		serializedObject.Update();

		if (null == animatorController) {
			contexts = AnimatorController.FindStateMachineBehaviourContext((SetParamSMB) target);
			if (contexts == null) {
				GUI.enabled = false;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("when"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("paramName"));
				switch ((AnimatorControllerParameterType) serializedObject.FindProperty("what").intValue) {
					case AnimatorControllerParameterType.Bool:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedBool"));
						break;
					case AnimatorControllerParameterType.Trigger:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedBool"));
						break;
					case AnimatorControllerParameterType.Int:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedInt"));
						break;
					case AnimatorControllerParameterType.Float:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedFloat"));
						break;
				}
				GUI.enabled = true;
				return;
			}
			animatorController = contexts[0].animatorController;
		}
		var parameters = animatorController.parameters;

		var currentParamName = serializedObject.FindProperty("paramName").stringValue;
		bool paramFound = false;

		GenericMenu menu = new GenericMenu();
		foreach (AnimatorControllerParameter elem in parameters) {
			menu.AddItem(new GUIContent(elem.name), false, OnParameterSelected, elem);
			if (elem.name == currentParamName) paramFound = true;
		}


		EditorGUILayout.PropertyField(serializedObject.FindProperty("when"));

		GUILayout.BeginHorizontal();
		var previousColor = GUI.color;
		if (!paramFound) GUI.color = Color.red;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("paramName"));
		GUI.color = previousColor;
		if (EditorGUILayout.DropdownButton(GUIContent.none, FocusType.Keyboard, GUILayout.Width(16))) {
			menu.ShowAsContext();
		}
		GUILayout.EndHorizontal();

		foreach (var elem in parameters) {
			if (elem.name == serializedObject.FindProperty("paramName").stringValue) {
				serializedObject.FindProperty("what").intValue = (int) elem.type;

				switch ((AnimatorControllerParameterType) serializedObject.FindProperty("what").intValue) {
					case AnimatorControllerParameterType.Bool:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedBool"));
						break;
					case AnimatorControllerParameterType.Trigger:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedBool"));
						break;
					case AnimatorControllerParameterType.Int:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedInt"));
						break;
					case AnimatorControllerParameterType.Float:
						EditorGUILayout.PropertyField(serializedObject.FindProperty("wantedFloat"));
						break;
				}

				break;
			}
		}

		serializedObject.ApplyModifiedProperties();
	}

	void OnParameterSelected(object parameter) {
		serializedObject.FindProperty("paramName").stringValue = ((AnimatorControllerParameter) parameter).name;
		serializedObject.ApplyModifiedProperties();
	}
}
