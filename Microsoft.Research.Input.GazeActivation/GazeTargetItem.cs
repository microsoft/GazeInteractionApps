using Windows.UI.Xaml;

namespace Microsoft.Research.Input.Gaze
{
    class GazeTargetItem
    {
        internal int ElapsedTime { get; set; }
        internal int NextStateTime { get; set; }
        internal long LastTimestamp { get; set; }
        internal GazePointerState ElementState { get; set; }
        internal UIElement TargetElement { get; set; }

        internal GazeTargetItem(UIElement target)
        {
            TargetElement = target;
        }

        internal void Reset(int nextStateTime)
        {
            ElementState = GazePointerState.PreEnter;
            ElapsedTime = 0;
            NextStateTime = nextStateTime;
        }
    }
}
