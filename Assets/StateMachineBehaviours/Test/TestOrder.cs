using System.Collections.Generic;
using UnityEngine;

namespace Ashkatchap.AnimatorEvents {
	public class TestOrder : MonoBehaviour {
		public string[] expectedEventsInOrder;

		private Queue<string> toProcess = new Queue<string>();

		private void Awake() {
			foreach (var str in expectedEventsInOrder) {
				toProcess.Enqueue(str);
			}
		}

		public void Test(string ev) {
			if (ev == toProcess.Peek()) {
				Debug.Log("Received expected [" + ev + "]");
				toProcess.Dequeue();
			}
			else {
				Debug.LogError("Expecting [" + toProcess.Dequeue() + "] but received [" + ev + "]");
			}
			if (toProcess.Count == 0) {
				Debug.Log("Test finished");
			}
		}
	}
}
