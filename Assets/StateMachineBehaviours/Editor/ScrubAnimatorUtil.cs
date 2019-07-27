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
	public static void DrawScrub(Rect rect, StateMachineBehaviour target, SerializedProperty normalizedTime, SerializedProperty repeat, SerializedProperty executeOnExitEnds) {
		bool updatePreview = false;
		float timeBefore = normalizedTime.floatValue;
		GUI.Label(new Rect(rect.x, rect.y, 50, 20), "Time");
		EditorGUI.PropertyField(new Rect(rect.x + 50, rect.y, rect.width - 50 - 42, 20), normalizedTime, GUIContent.none);
		if (GUI.Button(new Rect(rect.x + rect.width - 40, rect.y, 40, 20), "View")) {
			updatePreview = true;
		}
		EditorGUI.PropertyField(new Rect(rect.x, rect.y + 20, rect.width / 2, 20), repeat);
		EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + 20, rect.width / 2, 20), executeOnExitEnds);

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

	private static AnimationClip GetFirstAvailableClip(Motion motion) {
		if (motion is AnimationClip clip)
			return clip;

		if (motion is BlendTree tree) {
			foreach (ChildMotion childMotion in tree.children) {
				// Should we be worried about `cycleOffset`? https://docs.unity3d.com/ScriptReference/Animations.AnimatorState-cycleOffset.html

				var child = childMotion.motion;
				if (child is BlendTree childBlendTree) {
					var childClip = GetFirstAvailableClip(childBlendTree);
					if (childClip != null)
						return childClip;
				}
				else if (child is AnimationClip childClip)
					return childClip;
			}
		}

		return null;
	}
}
