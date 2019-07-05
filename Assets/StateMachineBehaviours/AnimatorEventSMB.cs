using System;
using UnityEngine;

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
		public string callback;
		public bool repeat;
		[Tooltip("Execute as OnStateExitTransitionEnd if this hasn't been executed yet")]
		public bool executeOnExitEnds;
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

		// Finalize events that want to execute on end, and reset them for later
		for (int i = 0; i < onNormalizedTimeReached.Length; i++) {
			if (onNormalizedTimeReached[i].executeOnExitEnds && onNormalizedTimeReached[i].nextNormalizedTime == 0) {
				GetAnimatorEvent(animator).CallEvent(onNormalizedTimeReached[i].callback);
			}
			onNormalizedTimeReached[i].nextNormalizedTime = 0;
		}
	}

	public override void StateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		for (int i = 0; i < onStateUpdated.Length; i++) {
			GetAnimatorEvent(animator).CallEvent(onStateUpdated[i].callback);
		}
		for (int i = 0; i < onNormalizedTimeReached.Length; i++) {
			if (stateInfo.normalizedTime >= onNormalizedTimeReached[i].normalizedTime + onNormalizedTimeReached[i].nextNormalizedTime) {
				GetAnimatorEvent(animator).CallEvent(onNormalizedTimeReached[i].callback);
				if (onNormalizedTimeReached[i].repeat)
					onNormalizedTimeReached[i].nextNormalizedTime++;
				else
					onNormalizedTimeReached[i].nextNormalizedTime = int.MaxValue;
			}
		}
	}

	private void FireTimedEvents(Animator animator, TimedEvent[] events) {
		for (int i = 0; i < events.Length; i++)
			GetAnimatorEvent(animator).CallEvent(events[i].callback);
	}
}
