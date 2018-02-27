using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace Microsoft.Research.Input.Gaze
{
    class GazeStats
    {
        GazeStats(uint maxHistoryLen)
        {
            _maxHistoryLen = maxHistoryLen;
            _history = new List<Point>();
        }

        void Reset()
        {
            _sumX = 0;
            _sumY = 0;
            _sumSquaredX = 0;
            _sumSquaredY = 0;
            _history.Clear();
        }

        void Update(float x, float y)
        {
            Point pt=new Point(x, y);
            _history.Add(pt);

            if (_history.Count > _maxHistoryLen)
            {
                var oldest = _history[0];
                _history.RemoveAt(0);

                _sumX -= oldest.X;
                _sumY -= oldest.Y;
                _sumSquaredX -= oldest.X * oldest.X;
                _sumSquaredY -= oldest.Y * oldest.Y;
            }
            _sumX += x;
            _sumY += y;
            _sumSquaredX += x * x;
            _sumSquaredY += y * y;
        }

        Point Mean
        {
            get
            {
                var count = _history.Count;
                return new Point((float)_sumX / count, (float)_sumY / count);
            }
        }

        //
        // StdDev = sqrt(Variance) = sqrt(E[X^2] – (E[X])^2)
        //
        Point StandardDeviation
        {
            get
            {
                var count = _history.Count;
                if (count < _maxHistoryLen)
                {
                    return new Point(0.0f, 0.0f);
                }
                double meanX = _sumX / count;
                double meanY = _sumY / count;
                float stddevX = (float)Math.Sqrt((_sumSquaredX / count) - (meanX * meanX));
                float stddevY = (float)Math.Sqrt((_sumSquaredY / count) - (meanY * meanY));
                return new Point(stddevX, stddevY);
            }
        }

        uint _maxHistoryLen;
        double _sumX;
        double _sumY;
        double _sumSquaredX;
        double _sumSquaredY;
        List<Point> _history;
    }
}
