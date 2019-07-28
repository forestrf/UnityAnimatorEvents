using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using BlendTree = UnityEditor.Animations.BlendTree;

public static class ScrubAnimatorUtil {
	/// <summary>
	/// Get the Currently used AnimatorController in the current AnimatorWindow and its related selected Animator, if any.
	/// </summary>
	/// <param name="controller">Current controller displayed in the Animator Window</param>
	/// <param name="animator">Current Animator component if inspecting one. May be null</param>
	public static void GetCurrentAnimatorAndController(out AnimatorController controller, out Animator animator) {
		Type animatorWindowType = Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
		var window = EditorWindow.GetWindow(animatorWindowType);

		var animatorField = animatorWindowType.GetField("m_PreviewAnimator", BindingFlags.Instance | BindingFlags.NonPublic);
		animator = animatorField.GetValue(window) as Animator;

		var controllerField = animatorWindowType.GetField("m_AnimatorController", BindingFlags.Instance | BindingFlags.NonPublic);
		controller = controllerField.GetValue(window) as AnimatorController;
	}

	/// <summary>
	/// Draw a timeline that can be scrubbed to allow picking a specific normalized time of an animation
	/// </summary>
	public static void DrawScrub(Rect rect, StateMachineBehaviour target, SerializedProperty normalizedTime, SerializedProperty repeat, SerializedProperty atLeastOnce, SerializedProperty neverWhileExit) {
		bool updatePreview = false;
		float timeBefore = normalizedTime.floatValue;
		GUI.Label(new Rect(rect.x, rect.y, 50, 20), "Time");
		EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50 - 42, 20), normalizedTime, GUIContent.none);
		if (GUI.Button(new Rect(rect.x + rect.width - 40, rect.y, 40, 20), "View")) {
			updatePreview = true;
		}

		DrawSmallProperty(new Rect(rect.x, rect.y + 20, rect.width / 3f, 20), new GUIContent("Loop", "Enable this to allow execution every time the state loops. Otherwise it will only happen once."), repeat);
		DrawSmallProperty(new Rect(rect.x + rect.width / 3f, rect.y + 20, rect.width / 3f, 20), new GUIContent("At Least Once", "Execute when the exit transition ends if this hasn't been executed yet."), atLeastOnce);
		DrawSmallProperty(new Rect(rect.x + rect.width * 2f / 3f, rect.y + 20, rect.width / 3f, 20), new GUIContent("Never While Exit", "Prevent executing during the exit transition."), neverWhileExit);

		if (timeBefore != normalizedTime.floatValue) updatePreview = true;

		if (updatePreview) {
			if (!AnimationMode.InAnimationMode())
				AnimationMode.StartAnimationMode();

			AnimatorController ignore;
			Animator animator;
			GetCurrentAnimatorAndController(out ignore, out animator);

			var contexts = AnimatorController.FindStateMachineBehaviourContext(target);

			foreach (var context in contexts) {
				AnimatorState state = context.animatorObject as AnimatorState;
				if (null == state) continue;

				AnimationClip previewClip = GetFirstAvailableClip(state.motion);
				if (null == previewClip) continue;

				AnimationMode.BeginSampling();
				AnimationMode.SampleAnimationClip(animator.gameObject, previewClip, normalizedTime.floatValue * previewClip.length);
				AnimationMode.EndSampling();
			}
		}
	}

	private static void DrawSmallProperty(Rect rect, GUIContent content, SerializedProperty property) {
		EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(content).x;
		EditorGUI.PropertyField(rect, property, content);
	}

	private static AnimationClip GetFirstAvailableClip(Motion motion) {
		AnimationClip clip = motion as AnimationClip;
		if (clip != null)
			return clip;

		BlendTree tree = motion as BlendTree;
		if (tree != null) {
			foreach (ChildMotion childMotion in tree.children) {
				// Should we be worried about `cycleOffset`? https://docs.unity3d.com/ScriptReference/Animations.AnimatorState-cycleOffset.html

				var child = childMotion.motion;
				BlendTree childTree = child as BlendTree;
				if (childTree != null) {
					var childClip = GetFirstAvailableClip(childTree);
					if (childClip != null)
						return childClip;
				}
				else {
					AnimationClip childClip = child as AnimationClip;
					if (childClip != null)
						return childClip;
				}
			}
		}

		return null;
	}
}
