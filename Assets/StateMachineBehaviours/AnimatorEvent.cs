using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ashkatchap.AnimatorEvents {
	[RequireComponent(typeof(Animator))]
	public class AnimatorEvent : MonoBehaviour {
		/// <summary>
		/// List of events. This are set in the Editor. To add events in runtime, add them to <see cref="eventsById"/> instead of here.
		/// </summary>
		public EventElement[] events = new EventElement[0];
		[Tooltip("Debug.Log calls to events. Only works in the Editor")]
		public bool debug = false;

		// Instead of executing events inmediately, we wait for the animator to play the animation for this frame
		// This will prevent being one frame ahead, with the character's old pose from the previous animation
		private List<EventElement> queuedEvents = new List<EventElement>();
		/// <summary>
		/// Events stored by its id. The id of an <see cref="EventElement"/> doesn't change after it's allocated.
		/// </summary>
		public readonly Dictionary<int, EventElement> eventsById = new Dictionary<int, EventElement>();
		[HideInInspector]
		public bool isEventsByIdReady { get; private set; }

		public void CallEvent(int id) {
#if UNITY_EDITOR
			if (debug) Debug.Log("Event id: " + id);
#endif
			EventElement ev;
			if (eventsById.TryGetValue(id, out ev))
				queuedEvents.Add(ev);
			else
				Debug.LogError("Event id [" + id + "] not found", this);
		}

		void Awake() {
			foreach (var elem in events)
				eventsById.Add(elem.id, elem);
			isEventsByIdReady = true;
		}

		private void LateUpdate() {
			for (int i = 0; i < queuedEvents.Count; i++) {
				queuedEvents[i].action.Invoke();
			}
			queuedEvents.Clear();
		}

		public EventElement GetEventById(int id) {
			foreach (var elem in events)
				if (elem.id == id)
					return elem;
			return null;
		}

		[Serializable]
		public class EventElement {
			public string name;
			public int id;
			public UnityEvent action;
		}
	}
}
