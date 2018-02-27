using System;
using Windows.UI.Xaml;

namespace Microsoft.Research.Input.Gaze
{
    public sealed class GazeElement : DependencyObject
    {
        DependencyProperty HasAttentionProperty = DependencyProperty.Register("HasAttention", typeof(bool), typeof(GazeElement), new PropertyMetadata(false));
        DependencyProperty InvokeProgressProperty = DependencyProperty.Register("InvokeProgress", typeof(double), typeof(GazeElement), new PropertyMetadata(0.0));

        public bool HasAttention { get { return (bool)GetValue(HasAttentionProperty); } set { SetValue(HasAttentionProperty, value); } }
        public double InvokeProgress { get { return (double)GetValue(InvokeProgressProperty); } set { SetValue(InvokeProgressProperty, value); } }

        //public event GazePointerEvent GazePointerEvent;
        public event EventHandler<GazeInvokedRoutedEventArgs> Invoked;

        internal void RaiseInvoked(Object sender, GazeInvokedRoutedEventArgs args)
        {
            Invoked?.Invoke(sender, args);
        }
    }
}
