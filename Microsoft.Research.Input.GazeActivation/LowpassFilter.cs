using Windows.Foundation;

namespace Microsoft.Research.Input.Gaze
{
    class LowpassFilter
    {
        internal LowpassFilter()
        {
            Previous = new Point(0, 0);
        }

        internal LowpassFilter(Point initial)
        {
            Previous = initial;
        }

        internal Point Previous { get; set; }

        internal Point Update(Point point, Point alpha)
        {
            Point pt;
            pt.X = (alpha.X * point.X) + ((1 - alpha.X) * Previous.X);
            pt.Y = (alpha.Y * point.Y) + ((1 - alpha.Y) * Previous.Y);
            Previous = pt;
            return Previous;
        }
    }
}
