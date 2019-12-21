using System;
using UnityEngine;

namespace Ashkatchap.AnimatorEvents {
	public class AnimatorEventSMB : StateMachineBehaviourExtended {
		public TimedEvent[] onStateEnterTransitionStart;
		public TimedEvent[] onStateEnterTransitionEnd;
		public TimedEvent[] onStateExitTransitionStart;
		public TimedEvent[] onStateExitTransitionEnd;
		public TimedEvent[] onNormalizedTimeReached;
		public TimedEvent[] onStateUpdated; // onStateUpdated instead of onStateUpdate because of a naming error. In the future it can be changed to onStateUpdate

		private AnimatorEvent _animatorEvent;
		private AnimatorEvent GetAnimatorEvent(Animator animator) {
			if (_animatorEvent == null) {
				_animatorEvent = animator.GetComponent<AnimatorEvent>();
			}
			return _animatorEvent;
		}

		[Serializable]
		public struct TimedEvent {
			[Range(0, 1)]
			public float normalizedTime;
			[Obsolete]
			public string callback;
			public int callbackId;
			public bool repeat;
			[UnityEngine.Serialization.FormerlySerializedAs("executeOnExitEnds")]
			public bool atLeastOnce;
			public bool neverWhileExit;
			[NonSerialized] public int nextNormalizedTime;
		}

		public override void StateEnter_TransitionStarts(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			FireTimedEvents(animator, onStateEnterTransitionStart);
		}

		public override void StateEnter_TransitionEnds(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			FireTimedEvents(animator, onStateEnterTransitionEnd);
		}

		public override void StateExit_TransitionStarts(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			FireTimedEvents(animator, onStateExitTransitionStart);
		}

		public override void StateExit_TransitionEnds(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			FireTimedEvents(animator, onStateExitTransitionEnd);

			// Finalize events that want to execute on exit, and reset them for later
			for (int i = 0; i < onNormalizedTimeReached.Length; i++) {
				if (onNormalizedTimeReached[i].atLeastOnce && onNormalizedTimeReached[i].nextNormalizedTime == 0) {
					if (onNormalizedTimeReached[i].callbackId == 0) {
						ShowIdWarning();
						onNormalizedTimeReached[i].callbackId = Animator.StringToHash(onNormalizedTimeReached[i].callback);
					}
					GetAnimatorEvent(animator).CallEvent(onNormalizedTimeReached[i].callbackId);
				}
				onNormalizedTimeReached[i].nextNormalizedTime = 0;
			}
		}

		public override void StateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			for (int i = 0; i < onStateUpdated.Length; i++) {
				if (onStateUpdated[i].callbackId == 0) {
					ShowIdWarning();
					onStateUpdated[i].callbackId = Animator.StringToHash(onStateUpdated[i].callback);
				}
				GetAnimatorEvent(animator).CallEvent(onStateUpdated[i].callbackId);
			}
			for (int i = 0; i < onNormalizedTimeReached.Length; i++) {
				if (onNormalizedTimeReached[i].neverWhileExit && currentState == State.ExitTransitioning) continue;
				if (stateInfo.normalizedTime >= onNormalizedTimeReached[i].normalizedTime + onNormalizedTimeReached[i].nextNormalizedTime) {
					if (onNormalizedTimeReached[i].callbackId == 0) {
						ShowIdWarning();
						onNormalizedTimeReached[i].callbackId = Animator.StringToHash(onNormalizedTimeReached[i].callback);
					}
					GetAnimatorEvent(animator).CallEvent(onNormalizedTimeReached[i].callbackId);
					if (onNormalizedTimeReached[i].repeat)
						onNormalizedTimeReached[i].nextNormalizedTime++;
					else
						onNormalizedTimeReached[i].nextNormalizedTime = int.MaxValue;
				}
			}
		}

		private void FireTimedEvents(Animator animator, TimedEvent[] events) {
			for (int i = 0; i < events.Length; i++) {
				if (events[i].callbackId == 0) {
					ShowIdWarning();
					events[i].callbackId = Animator.StringToHash(events[i].callback);
				}
				GetAnimatorEvent(animator).CallEvent(events[i].callbackId);
			}
		}

		private void ShowIdWarning() {
			Debug.LogError("Events now are referenced by an int that works as an identificator instead of a string. There are events that still don't have an id and need to have one for optimum performance. Please inspect all the Animator's states that have an AnimatorEventSMB attached by clicking on them to generate the ids and make this warning stop appearing. In case of not doing this, a performance hit will happen and future releases of this asset may break existing events. [THIS IS A WARNING, but is important enough to be thrown as an error].");
		}
	}
}
