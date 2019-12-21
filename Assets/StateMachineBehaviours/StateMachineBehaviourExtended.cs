using UnityEngine;
using UnityEngine.Animations;

namespace Ashkatchap.AnimatorEvents {
	public abstract class StateMachineBehaviourExtended : StateMachineBehaviour {
		public virtual void StateEnter_TransitionStarts(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
		public virtual void StateEnter_TransitionEnds(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
		public virtual void StateExit_TransitionStarts(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
		public virtual void StateExit_TransitionEnds(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
		public virtual void StateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

		// Removed: transitions MUST go through the states Enter or Exit inside the SubSM for this to work: e.g. AnyState breaks it
		// DO NOT USE, so removed to prevent future problems
		public sealed override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) { }
		public sealed override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller) { }
		public sealed override void OnStateMachineExit(Animator animator, int stateMachinePathHash) { }
		public sealed override void OnStateMachineExit(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller) { }
		// Overriding this makes the versions without AnimatorControllerPlayable not work //public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }
		// Overriding this makes the versions without AnimatorControllerPlayable not work //public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }
		// Overriding this makes the versions without AnimatorControllerPlayable not work //public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }
		public sealed override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
		public sealed override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

		public enum State {
			NotPaying,
			EnteredTransitioning,
			Updating,
			ExitTransitioning
		}

		private State stateCur = State.NotPaying;
		private State statePrev = State.NotPaying;
		private int transitionHash;

		public State currentState => stateCur;

		public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			//Debug.Log("(" + Time.frameCount + ") " + debugName + " Start");
			bool isInTransition = animator.IsInTransition(layerIndex);
			if (isInTransition) {
				statePrev = stateCur;
				stateCur = State.NotPaying;

				if (statePrev == State.EnteredTransitioning) {
					statePrev = State.Updating;
					StateEnter_TransitionEnds(animator, stateInfo, layerIndex);
				}
				if (statePrev == State.Updating) {
					statePrev = State.ExitTransitioning;
					StateExit_TransitionStarts(animator, stateInfo, layerIndex);
				}
			}


			stateCur = State.EnteredTransitioning;
			StateEnter_TransitionStarts(animator, stateInfo, layerIndex);
			if (!isInTransition) {
				stateCur = State.Updating;
				StateEnter_TransitionEnds(animator, stateInfo, layerIndex);
			}
			else {
				transitionHash = animator.GetAnimatorTransitionInfo(layerIndex).fullPathHash;
			}

			// First frame calls StateEnter, next ones call Update. Call Update AFTER Start too, on the same frame
			StateUpdate(animator, stateInfo, layerIndex);
		}

		public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			//Debug.Log("(" + Time.frameCount + ") " + debugName + " Exit");
			if (statePrev == State.ExitTransitioning) {
				statePrev = State.NotPaying;
				StateExit_TransitionEnds(animator, stateInfo, layerIndex);
			}
			else {
				if (stateCur == State.EnteredTransitioning) {
					stateCur = State.Updating;
					StateEnter_TransitionEnds(animator, stateInfo, layerIndex);
					StateUpdate(animator, stateInfo, layerIndex);
				}

				if (stateCur == State.Updating) {
					stateCur = State.ExitTransitioning;
					StateExit_TransitionStarts(animator, stateInfo, layerIndex);
				}

				stateCur = State.NotPaying;
				StateExit_TransitionEnds(animator, stateInfo, layerIndex);
			}
		}

		public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			//Debug.Log("(" + Time.frameCount + ") " + debugName + " Update");
			if (stateCur == State.EnteredTransitioning && (
				!animator.IsInTransition(layerIndex) ||
				animator.GetAnimatorTransitionInfo(layerIndex).fullPathHash != transitionHash)) {
				StateEnter_TransitionEnds(animator, stateInfo, layerIndex);
				stateCur = State.Updating;
			}

			StateUpdate(animator, stateInfo, layerIndex);

			if (stateCur == State.Updating && animator.IsInTransition(layerIndex)) {
				stateCur = State.ExitTransitioning;
				StateExit_TransitionStarts(animator, stateInfo, layerIndex);
			}
		}
	}
}
