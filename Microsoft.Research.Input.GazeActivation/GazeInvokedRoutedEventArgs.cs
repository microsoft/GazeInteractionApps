using Windows.UI.Xaml;

namespace Microsoft.Research.Input.Gaze
{
    public sealed class GazeInvokedRoutedEventArgs : RoutedEventArgs
    {
        public bool Handled { get; set; }
    }
}
