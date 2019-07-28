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

		var when = serializedObject.FindProperty("when");
		var what = serializedObject.FindProperty("what");
		var paramName = serializedObject.FindProperty("paramName");

		if (null == animatorController) {
			Animator ignore;
			ScrubAnimatorUtil.GetCurrentAnimatorAndController(out animatorController, out ignore);
		}
		var parameters = animatorController.parameters;

		var currentParamName = paramName.stringValue;
		bool paramFound = false;

		GenericMenu menu = new GenericMenu();
		foreach (AnimatorControllerParameter elem in parameters) {
			menu.AddItem(new GUIContent(elem.name), false, OnParameterSelected, elem);
			if (elem.name == currentParamName) paramFound = true;
		}

		EditorGUILayout.PropertyField(when);

		GUILayout.BeginHorizontal();
		var previousColor = GUI.color;
		if (!paramFound) GUI.color = Color.red;
		EditorGUILayout.PropertyField(paramName);
		GUI.color = previousColor;
		if (EditorGUILayout.DropdownButton(GUIContent.none, FocusType.Keyboard, GUILayout.Width(16))) {
			menu.ShowAsContext();
		}
		GUILayout.EndHorizontal();

		foreach (var elem in parameters) {
			if (elem.name == paramName.stringValue) {
				what.intValue = (int) elem.type;

				if (when.intValue == (int) SetParamSMB.When.WhileUpdating) {
					EditorGUILayout.PropertyField(serializedObject.FindProperty("curve"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("repeat"));

					switch ((AnimatorControllerParameterType) what.intValue) {
						case AnimatorControllerParameterType.Bool:
						case AnimatorControllerParameterType.Trigger:
							EditorGUILayout.HelpBox("True if the value is greater than 0", MessageType.Info);
							break;
						case AnimatorControllerParameterType.Int:
							EditorGUILayout.HelpBox("Rounded to the nearest integer", MessageType.Info);
							break;
					}
				}
				else {
					switch ((AnimatorControllerParameterType) what.intValue) {
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
				}

				break;
			}
		}

		if (when.intValue == (int) SetParamSMB.When.OnNormalizedTimeReached) {
			ScrubAnimatorUtil.DrawScrub(GUILayoutUtility.GetRect(0, 0, 40, 0),
				(StateMachineBehaviour) target,
				serializedObject.FindProperty("normalizedTime"),
				serializedObject.FindProperty("repeat"),
				serializedObject.FindProperty("atLeastOnce"),
				serializedObject.FindProperty("neverWhileExit"));
		}

		serializedObject.ApplyModifiedProperties();
	}

	void OnParameterSelected(object parameter) {
		serializedObject.FindProperty("paramName").stringValue = ((AnimatorControllerParameter) parameter).name;
		serializedObject.ApplyModifiedProperties();
	}
}
