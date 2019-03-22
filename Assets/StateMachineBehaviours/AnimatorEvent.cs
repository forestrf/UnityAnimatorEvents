using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimatorEvent : MonoBehaviour {
	public EventElement[] events;
	[Tooltip("Debug.Log calls to events. Only works in the Editor")]
	public bool debug = false;

	// Instead of executing events inmediately, we wait for the animator to play the animation for this frame
	// This will prevent being one frame ahead, with the character's old pose from the previous animation
	private List<EventElement> queuedEvents;
	private Dictionary<string, EventElement> elementsDict;

	public void CallEvent(string eventName) {
#if UNITY_EDITOR
		if (debug) Debug.Log("Event: " + eventName);
#endif
		if (elementsDict.TryGetValue(eventName, out EventElement ev))
			queuedEvents.Add(ev);
		else
			Debug.LogError("Event [" + eventName + "] not found", this);
	}

	void Awake() {
		queuedEvents = new List<EventElement>();
		elementsDict = new Dictionary<string, EventElement>();
		foreach (var elem in events)
			elementsDict.Add(elem.name, elem);

		// Initialize references in SMBs
		foreach (var smb in GetComponent<Animator>().GetBehaviours<AnimatorEventSMB>()) {
			smb.animatorEvent = this;
		}
	}

	private void LateUpdate() {
		for (int i = 0; i < queuedEvents.Count; i++) {
			queuedEvents[i].action.Invoke();
		}
		queuedEvents.Clear();
	}

	[Serializable]
	public class EventElement {
		public string name;
		public UnityEvent action;
	}
}
