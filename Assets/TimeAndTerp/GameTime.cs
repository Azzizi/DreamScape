using System;
using UnityEngine;

namespace FIL.Utilities
{
	public class GameTime : MonoBehaviour
	{
		private static GameTime instance;

		private float prevRealTimeSeconds = 0f;
		private float realTimeDelta;

		public bool enableTimers = true;
		public bool enableInterpolators = true;

		/// <summary>
		/// Returns the Instance for this Singleton. Creates an instance(and associated GameObject) if one does not already exist.
		/// NOTE: Attaching this script to a GameObject by hand is therefore OPTIONAL.
		/// </summary>
		public static GameTime Instance
		{
			get	{ return instance ?? (new GameObject("GameTime Global")).AddComponent<GameTime>(); }
			private set { instance = value; }
		}

		/// <summary>
		/// The real(unscaled) time in seconds since the last frame.
		/// NOTE: On the first frame after the instance is created, this will return the game(scaled) time.
		/// </summary>
		public static float RealTimeDelta { get { return Instance.realTimeDelta; } private set { Instance.realTimeDelta = value; } }

		/// <summary>
		/// The game(scaled) time in seconds since the last frame.
		/// </summary>
		public static float GameTimeDelta { get { return Time.deltaTime; } }
		
		/// <summary>
		/// When true, Timers will receive Update() calls from Unity.
		/// </summary>
		public static bool EnableTimers { get { return Instance.enableTimers; } set { Instance.enableTimers = value; } }

		/// <summary>
		/// When true, Interpolators will receive Update() calls from Unity.
		/// </summary>
		public static bool EnableInterpolators { get { return Instance.enableInterpolators; } set { Instance.enableInterpolators = value; } }

		/// <summary>
		/// Returns/sets Unity's delta timescale, and adjusts the physics timestep to maintain a steady simulation speed.
		/// </summary>
		public static float TimeScale
		{
			get { return Time.timeScale; }
			set
			{
				Time.timeScale = value;
				Time.fixedDeltaTime = .20f * value;
			}
		}

		void Awake()
		{
			if (instance == null)
				instance = this;

			else if (instance != this)
				Destroy(this);
		}

		void Update()
		{
			// We use realtimeSinceStartup differentials for our Real Time Delta, as RTSS is not affected by timescale
			// WARNING: This may return zero time between frames on some platforms-- RTSS behaves in a platform-dependent fashion due to system calls.

			if (prevRealTimeSeconds == 0f)
				RealTimeDelta = Time.deltaTime;
			else
				RealTimeDelta = Time.realtimeSinceStartup - prevRealTimeSeconds;

			prevRealTimeSeconds = Time.realtimeSinceStartup;

			#region Timer and Interpolator Updates

			if (enableInterpolators)
				Interpolator.Update();

			if (enableTimers)
				Timer.Update();

			#endregion
		}
	}
}