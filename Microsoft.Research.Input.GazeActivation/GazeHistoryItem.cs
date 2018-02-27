using Windows.UI.Xaml;

namespace Microsoft.Research.Input.Gaze
{
    class GazeHistoryItem
    {
        internal UIElement HitTarget { get; set; }
        internal long Timestamp { get; set; }
        internal int Duration { get; set; }
    }
}
