using System;
using UnityEngine;

namespace Ashkatchap.AnimatorEvents {
	public class SetParamSMB : StateMachineBehaviourExtended {
		public enum When { OnStateEnterTransitionStarts, OnStateEnterTransitionEnds, OnStateExitTransitionStarts, OnStateExitTransitionEnds, OnNormalizedTimeReached, WhileUpdating }

		public When when = When.OnStateEnterTransitionStarts;
		public AnimatorControllerParameterType what = AnimatorControllerParameterType.Bool;
		public string paramName;
		public bool wantedBool;
		public int wantedInt;
		public float wantedFloat;

		// Variables used by OnNormalizedTimeReached
		[Range(0, 1)]
		public float normalizedTime;
		[UnityEngine.Serialization.FormerlySerializedAs("executeOnExitEnds")]
		public bool atLeastOnce;
		public bool neverWhileExit;
		[NonSerialized] public int nextNormalizedTime;

		// Variables used by WhileUpdating
		[Tooltip("Horizontal axis equals time, Vertical axis equals a value at that time.")]
		public AnimationCurve curve;

		// Variables used by OnNormalizedTimeReached and WhileUpdating
		public bool repeat;


		public override void StateEnter_TransitionStarts(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			TryExecute(animator, When.OnStateEnterTransitionStarts);
		}
		public override void StateEnter_TransitionEnds(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			TryExecute(animator, When.OnStateEnterTransitionEnds);
		}
		public override void StateExit_TransitionStarts(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			TryExecute(animator, When.OnStateExitTransitionStarts);
		}
		public override void StateExit_TransitionEnds(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			TryExecute(animator, When.OnStateExitTransitionEnds);

			// Finalize events that want to execute on exit, and reset them for later
			if (atLeastOnce && nextNormalizedTime == 0) {
				TryExecuteNoCheck(animator);
			}
			nextNormalizedTime = 0;
		}

		public override void StateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			switch (when) {
				case When.OnNormalizedTimeReached:
					if (stateInfo.normalizedTime >= normalizedTime + nextNormalizedTime) {
						TryExecuteNoCheck(animator);
						if (repeat)
							nextNormalizedTime++;
						else
							nextNormalizedTime = int.MaxValue;
					}
					break;
				case When.WhileUpdating:
					var timeToEvaluate = stateInfo.normalizedTime;
					if (repeat) {
						Debug.Assert(curve.length > 0);
						timeToEvaluate %= curve[curve.length - 1].time;
					}
					float evaluatedCurve = curve.Evaluate(timeToEvaluate);
					switch (what) {
						case AnimatorControllerParameterType.Bool: animator.SetBool(paramName, evaluatedCurve > 0); break;
						case AnimatorControllerParameterType.Trigger: if (evaluatedCurve > 0) animator.SetTrigger(paramName); else animator.ResetTrigger(paramName); break;
						case AnimatorControllerParameterType.Int: animator.SetInteger(paramName, Mathf.RoundToInt(evaluatedCurve)); break;
						case AnimatorControllerParameterType.Float: animator.SetFloat(paramName, evaluatedCurve); break;
					}
					break;
			}
		}

		private void TryExecute(Animator animator, When origin) {
			if (origin != when) return;
			TryExecuteNoCheck(animator);
		}
		private void TryExecuteNoCheck(Animator animator) {
			switch (what) {
				case AnimatorControllerParameterType.Bool: animator.SetBool(paramName, wantedBool); break;
				case AnimatorControllerParameterType.Trigger: if (wantedBool) animator.SetTrigger(paramName); else animator.ResetTrigger(paramName); break;
				case AnimatorControllerParameterType.Int: animator.SetInteger(paramName, wantedInt); break;
				case AnimatorControllerParameterType.Float: animator.SetFloat(paramName, wantedFloat); break;
			}
		}
	}
}
