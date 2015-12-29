using System;

namespace FIL.Utilities
{
	/// <summary>
	/// An object that invokes an action after an amount of time has elapsed and
	/// optionally continues repeating until told to stop.
	/// </summary>
	public sealed class Timer
	{
		private static readonly Pool<Timer> timers = new Pool<Timer>(10, t => t._valid)
		{
			// Initialize is invoked whenever we get an instance through New()
			Initialize = t =>
			{
				t._valid = true;
				t._time = 0f;
			},
			// Deinitialize is invoked whenever an object is reclaimed during CleanUp()
			Deinitialize = t =>
			{
				t._tick = null;
				t.Tag = null;
			}
		};

		private Action<Timer> _tick;
		private float _time;
		private bool _valid;

		/// <summary>
		/// Gets or sets some extra data to the timer.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Gets whether or not this timer repeats.
		/// </summary>
		public bool Repeats { get; private set; }

		/// <summary>
		/// Gets whether or not this timer uses unscaled (real) time or scaled (game) time.
		/// </summary>
		public bool Realtime { get; private set; }

		/// <summary>
		/// Gets the length of time (in seconds) between ticks of the timer.
		/// </summary>
		public float TickLength { get; private set; }

		/// <summary>
		/// Creates a new Timer.
		/// </summary>
		/// <param name="tickLength">The amount of time between the timer's ticks.</param>
		/// <param name="tick">An action to perform when the timer ticks.</param>
		/// <param name="realtime">Should this Timer run in real time or (possibly) scaled game time</param>
		/// <param name="repeats">Whether or not the timer repeats.</param>
		/// <returns>The new Timer object or null if the timer pool is full.</returns>
		public static Timer Create(float tickLength, Action<Timer> tick, bool realtime = false, bool repeats = false)
		{
			if (tickLength <= 0f)
				throw new ArgumentException("tickLength must be greater than zero.");
			if (tick == null)
				throw new ArgumentNullException("tick");

			// get a new timer from the pool
			var t = timers.New();
			t.TickLength = tickLength;
			t.Repeats = repeats;
			t._tick = tick;
			t.Realtime = realtime;

			if (GameTime.Instance)	// This is a hacky little solution to bootstrap the GameTime instance if necessary
			{
			}

			return t;
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public void Stop()
		{
			_valid = false;
		}

		/// <summary>
		/// Updates all the Timers.
		/// </summary>
		public static void Update()
		{
			for (var i = 0; i < timers.ValidCount; i++)
			{
				var t = timers[i];

				// if a timer is stopped manually, it may not
				// be valid at this point so we skip it.
				if (!t._valid)
					continue;

				// update the timer's time
				if (t.Realtime)
					t._time += GameTime.RealTimeDelta;
				else
					t._time += GameTime.GameTimeDelta;

				// if the timer passed its tick length...
				if (t._time < t.TickLength) continue;
				// perform the action
				t._tick(t);

				// subtract the tick length in case we need to repeat
				t._time -= t.TickLength;

				// if the timer doesn't repeat, it is no longer valid
				if (!t.Repeats)
					t._valid = false;
			}

			// clean up any invalid timers
			timers.CleanUp();
		}
	}
}
