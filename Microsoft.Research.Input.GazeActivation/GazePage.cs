namespace Microsoft.Research.Input.Gaze
{
    public sealed class GazePage
    {
        public event GazePointerEvent GazePointerEvent;

        internal void RaiseGazePointerEvent(GazePointer sender, GazePointerEventArgs args)
        {
            GazePointerEvent?.Invoke(sender, args);
        }
    }
}
