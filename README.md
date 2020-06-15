# Unity Animator Events

This asset provides the ability to call UnityEvents stored in a script added next to an Animator from its AnimatorController's states.

This can be useful for things from easily adding events like footsteps to controlling how a character works by using the Animator like a state machine, blocking and unblocking the character's input, enabling and disabling rootmotion at certain points and much more.

This asset was later rewritten and published to the asset store with more features https://assetstore.unity.com/packages/tools/animation/animator-events-169047

## How to use

First, add the component `AnimatorEvent` to the GameObject that has the Animator that has the AnimatorController in question, and add events to it.

![Adding Events](https://raw.githubusercontent.com/forestrf/UnityAnimatorEvents/master/AssetsForReadme/CreateEvent.gif)

Then, navigate through the AnimatorController and add the script `AnimatorEventSMB` to the states that will call the previously defined events.

![Calling Events](https://raw.githubusercontent.com/forestrf/UnityAnimatorEvents/master/AssetsForReadme/UseEvent.gif)

The events can (only) be triggered by 5 type of triggers:
- Entering a state, just when the transition to it starts
- Entering a state, just when the transition to it ends
- Exiting a state, just when the transition from it starts
- Exiting a state, just when the transition from it ends
- At a specific point in time when playing a state. It can be repeated every time the sate loops. It can also fire the trigger in case the animator leaves the state before the specific point in time.

For the last type of trigger, a slider is provided to help pinpoint the exact time for the trigger to happen by previsualizing the animation. This feature doesn't work with blendtrees, only showing the preview for the first animation without blending it with the others.

You can change the name of an event at any point because it is referenced by an integer ID, not by its name. You can also type the character '/' in the event name to organize the events in a hierarchy like folders, by adding as many `/` as needed.
