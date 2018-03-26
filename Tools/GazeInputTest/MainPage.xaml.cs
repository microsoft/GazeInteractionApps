//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Research.Input.Gaze;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GazeInputTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GazePointer _gazePointer;

        public MainPage()
        {
            this.InitializeComponent();

            // Since this is an input test, don't make it dependent on shared settings
            _gazePointer = new GazePointer(this);
            _gazePointer.OnGazeInputEvent += OnGazeInputEvent;
            _gazePointer.OnGazePointerEvent += OnGazePointerEvent;

            ShowCursor.IsChecked = _gazePointer.IsCursorVisible;
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            Dwell.Content = ea.PointerState.ToString();
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }

        private void OnGazeInputEvent(GazePointer sender, GazeEventArgs ea)
        {
            Coordinates.Text = ea.Location.ToString();
        }

        private void Dwell_Click(object sender, RoutedEventArgs e)
        {
            Dwell.Content = "Clicked";
        }

        private void ShowCursor_Toggle(object sender, RoutedEventArgs e)
        {
            if (ShowCursor.IsChecked.HasValue)
            {
                _gazePointer.IsCursorVisible = ShowCursor.IsChecked.Value;
            }
        }
    }
}
