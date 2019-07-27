using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(AnimatorEventSMB))]
public class AnimatorEventSMBEditor : Editor {
	private ReorderableList list_onStateEnterTransitionStart;
	private ReorderableList list_onStateEnterTransitionEnd;
	private ReorderableList list_onStateExitTransitionStart;
	private ReorderableList list_onStateExitTransitionEnd;
	private ReorderableList list_onNormalizedTimeReached;
	private ReorderableList list_onStateUpdated;

	private readonly List<int> eventsAvailable = new List<int>();
	private readonly List<AnimatorEvent> matchingAnimatorEvent = new List<AnimatorEvent>();

	private UnityEditor.Animations.AnimatorController controller;

	private static GUIStyle eventNameStyle;

	private void InitializeIfNeeded() {
		if (controller != null) return;

		if (eventNameStyle == null) {
			eventNameStyle = GUI.skin.label;
			eventNameStyle.richText = true;
		}

		ScrubAnimatorUtil.GetCurrentAnimatorAndController(out controller, out var _);

		UpdateMatchingAnimatorEventList();

		CreateReorderableList("On State Enter Transition Start", 20, ref list_onStateEnterTransitionStart, serializedObject.FindProperty("onStateEnterTransitionStart"),
			(rect, index, isActive, isFocused) => {
				DrawCallbackField(rect, serializedObject.FindProperty("onStateEnterTransitionStart").GetArrayElementAtIndex(index));
			});
		CreateReorderableList("On State Enter Transition End", 20, ref list_onStateEnterTransitionEnd, serializedObject.FindProperty("onStateEnterTransitionEnd"),
			(rect, index, isActive, isFocused) => {
				DrawCallbackField(rect, serializedObject.FindProperty("onStateEnterTransitionEnd").GetArrayElementAtIndex(index));
			});
		CreateReorderableList("On State Exit Transition Start", 20, ref list_onStateExitTransitionStart, serializedObject.FindProperty("onStateExitTransitionStart"),
			(rect, index, isActive, isFocused) => {
				DrawCallbackField(rect, serializedObject.FindProperty("onStateExitTransitionStart").GetArrayElementAtIndex(index));
			});
		CreateReorderableList("On State Exit Transition End", 20, ref list_onStateExitTransitionEnd, serializedObject.FindProperty("onStateExitTransitionEnd"),
			(rect, index, isActive, isFocused) => {
				DrawCallbackField(rect, serializedObject.FindProperty("onStateExitTransitionEnd").GetArrayElementAtIndex(index));
			});
		CreateReorderableList("On State Update", 20, ref list_onStateUpdated, serializedObject.FindProperty("onStateUpdated"),
			(rect, index, isActive, isFocused) => {
				DrawCallbackField(rect, serializedObject.FindProperty("onStateUpdated").GetArrayElementAtIndex(index));
			});
		CreateReorderableList("On Normalized Time Reached", 60, ref list_onNormalizedTimeReached, serializedObject.FindProperty("onNormalizedTimeReached"),
			(rect, index, isActive, isFocused) => {
				var property = serializedObject.FindProperty("onNormalizedTimeReached").GetArrayElementAtIndex(index);

				DrawCallbackField(rect, property);

				rect.y += 20;

				ScrubAnimatorUtil.DrawScrub(rect,
					(StateMachineBehaviour) target,
					property.FindPropertyRelative("normalizedTime"),
					property.FindPropertyRelative("repeat"),
					property.FindPropertyRelative("executeOnExitEnds"));
			});
	}

	void UpdateMatchingAnimatorEventList() {
		var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
		AnimatorEvent[] animatorEvents;
		if (prefabStage != null) {
			animatorEvents = prefabStage.stageHandle.FindComponentsOfType<AnimatorEvent>();
		}
		else {
			animatorEvents = FindObjectsOfType<AnimatorEvent>();
		}
		matchingAnimatorEvent.Clear();
		foreach (var ae in animatorEvents) {
			var runtimeController = ae.GetComponent<Animator>().runtimeAnimatorController;
			var overrided = runtimeController as AnimatorOverrideController;
			if (runtimeController == controller || (overrided != null && overrided.runtimeAnimatorController == controller)) {
				matchingAnimatorEvent.Add(ae);


				List<int> sortedEventIndices = new List<int>();
				for (int i = 0; i < ae.events.Length; i++) {
					sortedEventIndices.Add(i);
				}
				sortedEventIndices.Sort((a, b) => ae.events[a].name.CompareTo(ae.events[b].name));


				foreach (var i in sortedEventIndices) {
					var ev = ae.events[i];
					if (!eventsAvailable.Contains(ev.id)) {
						eventsAvailable.Add(ev.id);
					}
				}
			}
		}
	}

	private void DrawCallbackField(Rect rect, SerializedProperty property) {
		var callbackProperty = property.FindPropertyRelative("callback");
		var callbackIdProperty = property.FindPropertyRelative("callbackId");
		if (callbackIdProperty.intValue == 0) {
			callbackIdProperty.intValue = Animator.StringToHash(callbackProperty.stringValue);
		}
		var ev = matchingAnimatorEvent[0].GetEventById(callbackIdProperty.intValue);
		float idWidth = 120;
		GUI.Label(new Rect(rect.x, rect.y, rect.width - idWidth, 18), ev != null ? ev.name : "<color=red><b>EVENT ID NOT FOUND</b></color>", eventNameStyle);
		var prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 20;
		EditorGUI.PropertyField(new Rect(rect.x + rect.width - idWidth, rect.y, idWidth, 18), callbackIdProperty, new GUIContent("ID ", "ID of the event. Change it manually only if needed."));
		EditorGUIUtility.labelWidth = prevLabelWidth;
	}

	private void CreateReorderableList(string title, int height, ref ReorderableList reorderableList, SerializedProperty soList, ReorderableList.ElementCallbackDelegate drawCallback) {
		reorderableList = new ReorderableList(serializedObject, soList, true, false, true, true);
		reorderableList.elementHeight = height;
		reorderableList.drawHeaderCallback = (rect) => {
			GUI.Label(rect, title);
		};
		reorderableList.drawElementCallback = drawCallback;
		reorderableList.onAddDropdownCallback = (buttonRect, list) => {
			if (matchingAnimatorEvent.Count == 0) return;

			var menu = new GenericMenu();
			for (int i = 0; i < eventsAvailable.Count; i++) {
				int j = i;
				menu.AddItem(new GUIContent(matchingAnimatorEvent[0].GetEventById(eventsAvailable[i]).name),
				false, (data) => {
					serializedObject.Update();
					soList.InsertArrayElementAtIndex(soList.arraySize);
					soList.GetArrayElementAtIndex(soList.arraySize - 1).FindPropertyRelative("callbackId").intValue = eventsAvailable[j];
					serializedObject.ApplyModifiedProperties();
				}, eventsAvailable[i]);
			}
			menu.ShowAsContext();
		};
	}

	public override void OnInspectorGUI() {
		//DrawDefaultInspector();

		InitializeIfNeeded();

		if (Application.isPlaying) GUI.enabled = false;

		serializedObject.Update();

		list_onStateEnterTransitionStart.DoLayoutList();
		list_onStateEnterTransitionEnd.DoLayoutList();
		list_onStateExitTransitionStart.DoLayoutList();
		list_onStateExitTransitionEnd.DoLayoutList();
		list_onStateUpdated.DoLayoutList();
		list_onNormalizedTimeReached.DoLayoutList();

		serializedObject.ApplyModifiedProperties();

		if (Application.isPlaying) GUI.enabled = true;
	}

	private void OnDestroy() {
		if (AnimationMode.InAnimationMode())
			AnimationMode.StopAnimationMode();
	}
}
