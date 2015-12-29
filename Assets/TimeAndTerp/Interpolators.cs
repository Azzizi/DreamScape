using System;

namespace FIL.Utilities
{
    /// <summary>
    /// A delegate used by Interpolators to scale their progress and generate their current value.
    /// </summary>
    /// <param name="progress">The current progress of the Interpolator in the range [0, 1].</param>
	/// <param name="direction">The direction in which to apply the easing.</param>
    /// <returns>A value representing the scaled progress used to generate the Interpolator's Value.</returns>
	public delegate float InterpolatorScaleDelegate(float progress, InterpolatorEaseDirection direction);
	
	public enum InterpolatorEaseDirection { In = 0, Out = 1, InOut = 2 }

    public sealed class Interpolator
    {
        private static readonly Pool<Interpolator> Interpolators = new Pool<Interpolator>(10, i => i._valid)
        {
            // Initialize is invoked whenever we get an instance through New()
            Initialize = i =>
            {
				i._valid = true;
                i.Progress = 0f;
            },
            // Deinitialize is invoked whenever an object is reclaimed during CleanUp()
            Deinitialize = i =>
            {
                i.Tag = null;
                i._scale = null;
                i._step = null;
                i._completed = null;
            }
        };

        private Action<Interpolator> _completed;

        private float _range;
        private InterpolatorScaleDelegate _scale;
        private float _speed;
        private Action<Interpolator> _step;
        private bool _valid;

		private InterpolatorEaseDirection _direction;

        /// <summary>
        /// Gets the interpolator's progress in the range of [0, 1].
        /// </summary>
        public float Progress { get; private set; }

        /// <summary>
        /// Gets the interpolator's starting value.
        /// </summary>
        public float Start { get; private set; }

        /// <summary>
        /// Gets the interpolator's ending value.
        /// </summary>
        public float End { get; private set; }

		/// <summary>
		/// Gets the interpolator's current value.
		/// </summary>
		public float Value { get; private set; }

		/// <summary>
		/// Gets whether the Interpolator runs in real or game (scaled) time.
		/// </summary>
		public bool Realtime { get; private set; }

		/// <summary>
        /// Gets or sets some extra data to the timer.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Creates a new Interpolator.
        /// </summary>
        /// <param name="start">The starting value.</param>
        /// <param name="end">The ending value.</param>
        /// <param name="length">The length of time, in seconds, to perform the interpolation.</param>
        /// <param name="step">An optional callback to invoke when the Interpolator is updated.</param>
        /// <param name="completed">An optional callback to invoke when the Interpolator completes.</param>
        /// <returns>The Interpolator instance.</returns>
        public static Interpolator Create(
            float start,
            float end,
            float length,
            Action<Interpolator> step,
            Action<Interpolator> completed)
        {
            return Create(start, end, length, false, InterpolatorScales.Linear, InterpolatorEaseDirection.InOut, step, completed);
        }

        /// <summary>
        /// Creates a new Interpolator.
        /// </summary>
        /// <param name="start">The starting value.</param>
        /// <param name="end">The ending value.</param>
        /// <param name="length">The length of time, in seconds, to perform the interpolation.</param>
		/// <param name="realtime">Whether this Interpolator runs in real time or game time.</param>
        /// <param name="scale">The InterpolatorScaleDelegate that will perform the interpolation.</param>
        /// <param name="step">An optional callback to invoke when the Interpolator is updated.</param>
        /// <param name="completed">An optional callback to invoke when the Interpolator completes.</param>
        /// <returns>The Interpolator instance.</returns>
        public static Interpolator Create(
            float start,
            float end,
            float length,
			bool realtime,
            InterpolatorScaleDelegate scale,
			InterpolatorEaseDirection direction,
            Action<Interpolator> step,
            Action<Interpolator> completed)
        {
            if (length <= 0f)
                throw new ArgumentException("length must be greater than zero");
            if (scale == null)
                throw new ArgumentNullException("scale");

            var i = Interpolators.New();
            i.Start = start;
            i.End = end;
			i.Realtime = realtime;
            i._range = end - start;
            i._step = step;
            i._completed = completed;
            i._scale = scale;
            i._speed = 1f / length;
			i._direction = direction;

			if (GameTime.Instance) { }	// This is a hacky little solution to bootstrap the GameTime instance if necessary

            return i;
        }

        /// <summary>
        /// Stops the Interpolator.
        /// </summary>
        public void Stop()
        {
            _valid = false;
        }

        /// <summary>
        /// Updates all the Interpolators using GameTime values.
        /// </summary>
        public static void Update()
        {
            for (var i = 0; i < Interpolators.ValidCount; i++)
            {
                var p = Interpolators[i];

                // if Stop was called, the interpolator may already be invalid, so we
                // make sure to skip those interpolators.
                if (!p._valid)
                    continue;

                // update the progress, clamping at 1f
				if (p.Realtime)
	                p.Progress = Math.Min(p.Progress + p._speed * GameTime.RealTimeDelta, 1f);
				else
					p.Progress = Math.Min(p.Progress + p._speed * GameTime.GameTimeDelta, 1f);

                // get the scaled progress and use that to generate the value
                var scaledProgress = p._scale(p.Progress, p._direction);
                p.Value = p.Start + p._range * scaledProgress;

                // invoke the step callback
                if (p._step != null)
                    p._step(p);

                // if the progress is 1...
                if (p.Progress != 1f) continue;
                // the interpolator is done
                p._valid = false;

                // invoke the completed callback
                if (p._completed != null)
                    p._completed(p);
            }

            // clean up the invalid interpolators
            Interpolators.CleanUp();
        }
    }


    /// <summary>
    /// A static class that contains predefined scales for Interpolators.
    /// </summary>
    public static class InterpolatorScales
    {
        /// <summary>
        /// A linear interpolator scale. This is used by default by the Interpolator if no other scale is given.
        /// </summary>
        public static readonly InterpolatorScaleDelegate Linear = LinearInterpolation;

        /// <summary>
        /// A quadratic interpolator scale.
        /// </summary>
        public static readonly InterpolatorScaleDelegate Quadratic = QuadraticInterpolation;

        /// <summary>
        /// A cubic interpolator scale.
        /// </summary>
        public static readonly InterpolatorScaleDelegate Cubic = CubicInterpolation;

		/// <summary>
		/// A quartic interpolator scale.
		/// </summary>
		public static readonly InterpolatorScaleDelegate Quartic = QuarticInterpolation;

		/// <summary>
		/// A quintic interpolator scale.
		/// </summary>
		public static readonly InterpolatorScaleDelegate Quintic = QuinticInterpolation;

		/// <summary>
		/// A quintic interpolator scale.
		/// </summary>
		public static readonly InterpolatorScaleDelegate Exponential = ExponentialInterpolation;

		/// <summary>
		/// A circular interpolator scale.
		/// </summary>
		public static readonly InterpolatorScaleDelegate Circular = CircularInterpolation;

		/// <summary>
		/// An interpolator scale that bounces back before/after the start/target position.
		/// </summary>
		public static readonly InterpolatorScaleDelegate Spring = SpringInterpolation;


		private static float LinearInterpolation(float progress, InterpolatorEaseDirection direction)
        {
            return progress;
        }

        private static float QuadraticInterpolation(float progress, InterpolatorEaseDirection direction)
        {
			switch(direction)
			{
				case InterpolatorEaseDirection.In:
					return progress * progress;

				case InterpolatorEaseDirection.Out:
					return -progress * (progress - 2f);

				case InterpolatorEaseDirection.InOut:
					progress *= 2f;
					if (progress < 1f) 
						return 0.5f * progress * progress;
					progress--;
					return -0.5f * (progress * (progress - 2f) - 1f);
				
				default:
					return progress * progress;
			}
		}

		private static float CubicInterpolation(float progress, InterpolatorEaseDirection direction)
        {
			switch (direction)
			{
				case InterpolatorEaseDirection.In:
					return progress * progress * progress;

				case InterpolatorEaseDirection.Out:
					progress--;
					return progress * progress * progress + 1f;

				case InterpolatorEaseDirection.InOut:
					progress *= 2f;
					if (progress < 1f)
						return 0.5f * progress * progress * progress;
					progress -= 2f;
					return 0.5f * (progress * progress * progress + 2f);

				default:
					return progress * progress * progress;
			}
        }

		private static float QuarticInterpolation(float progress, InterpolatorEaseDirection direction)
		{
			switch (direction)
			{
				case InterpolatorEaseDirection.In:
					return progress * progress * progress * progress;

				case InterpolatorEaseDirection.Out:
					progress--;
					return -(progress * progress * progress * progress - 1f);

				case InterpolatorEaseDirection.InOut:
					progress *= 2f;
					if (progress < 1f)
						return 0.5f * progress * progress * progress * progress;
					progress -= 2f;
					return -0.5f * (progress * progress * progress * progress - 2f);

				default:
					return progress * progress * progress * progress;
			}
		}

		private static float QuinticInterpolation(float progress, InterpolatorEaseDirection direction)
        {
			switch (direction)
			{
				case InterpolatorEaseDirection.In:
					return progress * progress * progress * progress * progress;

				case InterpolatorEaseDirection.Out:
					return (progress -= 1f) * progress * progress * progress * progress + 1f;

				case InterpolatorEaseDirection.InOut:
					if ((progress *= 2f) < 1f)
						return .5f * progress * progress * progress * progress * progress;
					return .5f * ((progress -= 2f) * progress * progress * progress * progress + 2f);

				default:
					return progress * progress * progress * progress * progress;
			}
        }

		private static float ExponentialInterpolation(float progress, InterpolatorEaseDirection direction)
		{
			switch (direction)
			{
				case InterpolatorEaseDirection.In:
					return (progress <= 0) ? 0 : (float)Math.Pow(2f, 10f * (progress - 1f));

				case InterpolatorEaseDirection.Out:
					return (progress >= 1f) ? 1f : -(float)Math.Pow(2f, -10f * progress) + 1f;

				case InterpolatorEaseDirection.InOut:
					if ( progress <= 0f)
						return 0f;
					if ( progress >= 1f)
						return 1.0f;
					if ((progress *= 2) < 1f)
						return .5f * (float)Math.Pow(2f, 10f * (progress - 1f));

					return .5f * (-(float)Math.Pow(2f, -10f * --progress) + 2f);

				default:
					return (progress <= 0) ? 0 : (float)Math.Pow(2f, 10f * (progress - 1f));
			}
		}

		private static float CircularInterpolation(float progress, InterpolatorEaseDirection direction)
		{
			switch (direction)
			{
				case InterpolatorEaseDirection.In:
					return -((float)Math.Sqrt(1f - progress * progress) - 1f);

				case InterpolatorEaseDirection.Out:
					return (float)Math.Sqrt(1f - (progress -= 1f) * progress);

				case InterpolatorEaseDirection.InOut:
					if (( progress *= 2 ) < 1)
						return -.5f * ((float)Math.Sqrt(1f - progress * progress) - 1f);

					return .5f * ((float)Math.Sqrt(1f - (progress -= 2f) * progress) + 1f);

				default:
					return -(float)(Math.Sqrt(1f - progress * progress) - 1f);
			}
		}

		private static float SpringInterpolation(float progress, InterpolatorEaseDirection direction)
		{
			switch (direction)
			{
				case InterpolatorEaseDirection.In:
					return progress * progress * (2.70158f * progress - 1.70158f);

				case InterpolatorEaseDirection.Out:
					return (progress -= 1) * progress * (2.70158f * progress + 1.70158f) + 1f;

				case InterpolatorEaseDirection.InOut:
					float s = 1.70158f;
					if ( ( progress *= 2 ) < 1f )
						return .5f * ( progress * progress * (((s *= (1.525f)) + 1f) * progress - s ));
					return .5f * ((progress -= 2f) * progress * (((s *= (1.525f)) + 1f) * progress + s) + 2f);

				default:
					return progress * progress * (2.70158f * progress - 1.70158f);
			}
		}
    }
}