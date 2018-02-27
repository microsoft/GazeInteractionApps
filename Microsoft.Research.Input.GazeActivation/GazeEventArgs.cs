using Windows.Foundation;

namespace Microsoft.Research.Input.Gaze
{
    public class GazeEventArgs
    {
        public Point Location { get; set; }
        public long Timestamp { get; set; }

        internal GazeEventArgs(Point location, long timestamp)
        {
            Location = location;
            Timestamp = timestamp;
        }
    }
}
