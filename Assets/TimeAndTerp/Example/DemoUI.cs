using UnityEngine;
using FIL.Utilities;

public class DemoUI : MonoBehaviour
{

	string[] scaleSelectStrings = { "Linear", "Quadratic", "Cubic", "Quartic", "Quintic", "Exponential", "Circular", "Spring" };
	string[] dirSelectStrings = { "In", "Out", "InOut"};
	int scaleSelectNum = 0;
	int dirSelectNum = 0;
	float timeSliderValue = 1.0f;

	bool direction = true;
	float GametimeDelay = 0.5f;
	float RealtimeDelay = 0.5f;
	InterpolatorScaleDelegate scaling = InterpolatorScales.Linear;			// these delegates control the easing functions, and are stored in the InterpolatorScales class
	InterpolatorEaseDirection easeDir = InterpolatorEaseDirection.InOut;	// this enumeration controls the direction the easing is applied in

	int running = 0;	// used to block the UI from creating adddional, conflicting Timer/Interpolator pairs while the spheres are in motion
	float StartX;
	float EndX;

	public Transform realtimeSphere;
	public Transform gametimeSphere;

	/// <summary>
	/// Our UI method for the demo scene. Nothing particularly interesting in here-- just stock UnityGUI
	/// </summary>
	void OnGUI()
	{
		GUI.Label(new Rect(25, 25, 100, 25), "Interpolator Type");
		scaleSelectNum = GUI.SelectionGrid(new Rect(25, 50, 100, 200), scaleSelectNum, scaleSelectStrings, 1);

		GUI.Label(new Rect(150, 25, 100, 25), "Ease Direction");
		dirSelectNum = GUI.SelectionGrid(new Rect(150, 50, 85, 200), dirSelectNum, dirSelectStrings, 1);

		GUI.Label(new Rect(250, 25, 200, 25), "Timescale: " + timeSliderValue.ToString());
		timeSliderValue = GUI.HorizontalSlider(new Rect(250, 50, 200, 25), timeSliderValue, 0.001f, 2.0f);

		if (GameTime.TimeScale != timeSliderValue)
			GameTime.TimeScale = timeSliderValue;

		GUI.Label(new Rect(250, 75, 150, 25), "Game Time Sphere");
		GUI.Label(new Rect(300, 100, 150, 25), "Delay (s): " + GametimeDelay.ToString());
		GametimeDelay = GUI.HorizontalSlider(new Rect(300, 125, 150, 25), GametimeDelay, 0.5f, 4.0f);

		GUI.Label(new Rect(250, 150, 150, 25), "Real Time Sphere");
		GUI.Label(new Rect(300, 175, 150, 25), "Delay (s): " + RealtimeDelay.ToString());
		RealtimeDelay = GUI.HorizontalSlider(new Rect(300, 200, 150, 25), RealtimeDelay, 0.5f, 4.0f);

		if (running <= 0)
		{
			if (GUI.Button(new Rect(350, 225, 100, 25), "Go"))
				StartSpheres();
		}
		else
			GUI.Button(new Rect(350, 225, 100, 25), "Running...");
	}

	/// <summary>
	/// Processes the UI data and starts our timers and Interpolator chain running for each sphere.
	/// </summary>
	void StartSpheres()
	{
		#region UI processing
		switch (scaleSelectNum)
		{
			case 0:
				scaling = InterpolatorScales.Linear;
				break;
			case 1:
				scaling = InterpolatorScales.Quadratic;
				break;
			case 2:
				scaling = InterpolatorScales.Cubic;
				break;
			case 3:
				scaling = InterpolatorScales.Quartic;
				break;
			case 4:
				scaling = InterpolatorScales.Quintic;
				break;
			case 5:
				scaling = InterpolatorScales.Exponential;
				break;
			case 6:
				scaling = InterpolatorScales.Circular;
				break;
			case 7:
				scaling = InterpolatorScales.Spring;
				break;
			default:
				scaling = InterpolatorScales.Linear;
				break;
		}

		switch (dirSelectNum)
		{
			case 0:
				easeDir = InterpolatorEaseDirection.In;
				break;
			case 1:
				easeDir = InterpolatorEaseDirection.Out;
				break;
			case 2:
				easeDir = InterpolatorEaseDirection.InOut;
				break;
			default:
				easeDir = InterpolatorEaseDirection.InOut;
				break;
		}

		#endregion

		if (direction)
		{
			StartX = -5f;
			EndX = 5f;
		}
		else
		{
			StartX = 5f;
			EndX = -5f;
		}

		running = 2;

		// Here's the most important part of the method-- creating the chain. There are two ways Timers and Interpolators are
		// typically used. The most straightforward (but more involved) creation is shown here. We've chosen to create our 
		// "realtime" sphere using methods declared elsewhere that perform the event actions. In this case, when the Timer fires,
		// it will call the RealtimeTick method. We have also set the Timer to use real (unscaled) time, which means it will
		// always fire at the given rate (RealtimeDelay) regardless of Unity's timescale setting. Finally, we have told the Timer
		// That we want it to fire exactly once and then expire, rather than issue repeated ticks. This creation method is
		// preferred in the case of many Timers/Interpolators that share a common action, as it's easier to reuse the code.
		Timer.Create(RealtimeDelay, RealtimeTick, true, false);
		
		// A more advanced, and (generally) common use of both Timers and Interpolators, using lambda expressions in place of 
		// defined methods(see the MSDN article on lambda expressions if necessary). These allow you to write actions directly
		// inline with the Timer/Interoplator creation and is the most flexible way to use them. Here we start a Timer, and for
		// the tick action, we pass a lambda expression that creates our interpolator. We then pass the Interpolator lambdas for
		// both the step and completion actions. This is functionally equivalent to the above method, without requiring that we
		// write a permanent method to call. This form of creation is preferred for one-off Timers/Interpolators that will not be
		// reused in multiple places in the code. NOTE: be careful when doing this, as lambdas can access variables in scope
		// directly outside of themselves and Time and Terp yeilds effects over several frames. Accessing varables local to the 
		// function you call Create() in can have unpredictable results. Try to stick to persistant objects/feilds/etc unless 
		// you definitely know what you're doing.
		Timer.Create(GametimeDelay,
			(Timer gametimeTimer) =>
			{
				Interpolator.Create(StartX, EndX, 2.0f, false, scaling, easeDir,
					(Interpolator gametimeInterp) =>
					{
						Vector3 temp = gametimeSphere.position;
						temp.x = gametimeInterp.Value;
						gametimeSphere.position = temp;
					},
					(Interpolator gametimeInterp) =>
					{
						running--;

						if (running <= 0)
							direction = !direction;
					});
			},
			false,
			false);
	}

	/// <summary>
	/// The method called when our realtime sphere's timer fires. NOTE: it just happens that we've chosen to fire
	/// the tick event this way for the realtime timer. It is NOT a requirement. We could have done both timers
	/// either way.
	/// </summary>
	/// <param name="realtimeTimer">The specific Timer instance calling this tick method</param>
	void RealtimeTick(Timer realtimeTimer)
	{
		// When the timer goes off, create a new Interpolator to move the sphere, using our defined step and
		// completed events.
		Interpolator.Create(StartX, EndX, 2.0f, true, scaling, easeDir, RealtimeStep, OnRealtimeCompleted);
	}

	/// <summary>
	/// The method that is called each time the realtime sphere Interpolator's value updates. NOTE: it just happens 
	/// that we've chosen to fire the step event this way for the realtime sphere. It is NOT a requirement. We could 
	/// have done both either way. 
	/// </summary>
	/// <param name="realtimeInterp">The specific Interpolator instance calling this step method</param>
	void RealtimeStep(Interpolator realtimeInterp)
	{
		// do the job of moving the sphere based on the interpolator's value
		Vector3 temp = realtimeSphere.position;
		temp.x = realtimeInterp.Value;
		realtimeSphere.position = temp;
	}

	/// <summary>
	/// The method that is called when the realtime sphere Interpolator reaches the target value. NOTE: it just happens 
	/// that we've chosen to fire the event this way for the realtime sphere. It is NOT a requirement. We could have 
	/// done both either way. 
	/// </summary>
	/// <param name="realtimeInterp">The specific Interpolator instance calling this step method</param>
	void OnRealtimeCompleted(Interpolator realtimeInterp)
	{
		// perform cleanup operations when the Interpolator's done. Note
		// that we don't have to do anything with the Interpolator-- Just
		// the final bits of work we need to accomplish for ourselves at
		// the end.
		running--;

		if (running <= 0)
			direction = !direction;
	}
}
