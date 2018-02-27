using System;
using Windows.Foundation;

namespace Microsoft.Research.Input.Gaze
{
    class OneEuroFilter : IGazeFilter
    {
        const float EUROFILTER_DEFAULT_BETA = 5.0f;
        const float EUROFILTER_DEFAULT_CUTOFF = 0.1f;
        const float EUROFILTER_DEFAULT_VELOCITY_CUTOFF = 1.0f;
        public OneEuroFilter()
        {
            _lastTimestamp = 0;
            Beta = EUROFILTER_DEFAULT_BETA;
            Cutoff = EUROFILTER_DEFAULT_CUTOFF;
            VelocityCutoff = EUROFILTER_DEFAULT_VELOCITY_CUTOFF;
        }

        public OneEuroFilter(float cutoff, float beta)
        {
            _lastTimestamp = 0;
            Beta = beta;
            Cutoff = cutoff;
            VelocityCutoff = EUROFILTER_DEFAULT_VELOCITY_CUTOFF;
        }

        GazeEventArgs IGazeFilter.Update(GazeEventArgs args)
        {
            if (_lastTimestamp == 0)
            {
                _lastTimestamp = args.Timestamp;
                _pointFilter = new LowpassFilter(args.Location);
                _deltaFilter = new LowpassFilter(new Point());
                return new GazeEventArgs(args.Location, args.Timestamp);
            }

            Point gazePoint = args.Location;

            // Reducing _beta increases lag. Increasing beta decreases lag and improves response time
            // But a really high value of beta also contributes to jitter
            float beta = Beta;

            // This simply represents the cutoff frequency. A lower value reduces jiiter
            // and higher value increases jitter
            float cf = Cutoff;
            Point cutoff = new Point(cf, cf);

            // determine sampling frequency based on last time stamp
            float samplingFrequency = 1000000.0f / Math.Max(1, args.Timestamp - _lastTimestamp);
            _lastTimestamp = args.Timestamp;

            // calculate change in distance...
            Point deltaDistance;
            deltaDistance.X = gazePoint.X - _pointFilter.Previous.X;
            deltaDistance.Y = gazePoint.Y - _pointFilter.Previous.Y;

            // ...and velocity
            Point velocity = new Point(deltaDistance.X * samplingFrequency, deltaDistance.Y * samplingFrequency);

            // find the alpha to use for the velocity filter
            float velocityAlpha = Alpha(samplingFrequency, VelocityCutoff);
            Point velocityAlphaPoint = new Point(velocityAlpha, velocityAlpha);

            // find the filtered velocity
            Point filteredVelocity = _deltaFilter.Update(velocity, velocityAlphaPoint);

            // ignore sign since it will be taken care of by deltaDistance
            filteredVelocity.X = Math.Abs(filteredVelocity.X);
            filteredVelocity.Y = Math.Abs(filteredVelocity.Y);

            // compute new cutoff to use based on velocity
            cutoff.X += beta * filteredVelocity.X;
            cutoff.Y += beta * filteredVelocity.Y;

            // find the new alpha to use to filter the points
            Point distanceAlpha = new Point(Alpha(samplingFrequency, cutoff.X), Alpha(samplingFrequency, cutoff.Y));

            // find the filtered point
            Point filteredPoint = _pointFilter.Update(gazePoint, distanceAlpha);

            // compute the new args
            var fa = new GazeEventArgs(filteredPoint, args.Timestamp);
            return fa;
        }

        float Beta { get; set; }
        float Cutoff { get; set; }
        float VelocityCutoff { get; set; }

        float Alpha(float rate, double cutoff)
        {
            float te = 1.0f / rate;
            float tau = (float)(1.0f / (2 * Math.PI * cutoff));
            float alpha = te / (te + tau);
            return alpha;
        }

        long _lastTimestamp;
        LowpassFilter _pointFilter;
        LowpassFilter _deltaFilter;
    }
}
