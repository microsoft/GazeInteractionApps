using Windows.UI.Xaml;

namespace Microsoft.Research.Input.Gaze
{
    public class GazePointerEventArgs
    {
        UIElement HitTarget { get; set; }
        public GazePointerState PointerState { get; set; }
        int ElapsedTime { get; set; }

        internal GazePointerEventArgs(UIElement target, GazePointerState state, int elapsedTime)
        {
            HitTarget = target;
            PointerState = state;
            ElapsedTime = elapsedTime;
        }
    }
}
