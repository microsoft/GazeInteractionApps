using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Research.Input.Gaze;
using Windows.UI.Popups;

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

        private void ShowCursor_Checked(object sender, RoutedEventArgs e)
        {
            _gazePointer.IsCursorVisible = true;
        }

        private void ShowCursor_Unchecked(object sender, RoutedEventArgs e)
        {
            _gazePointer.IsCursorVisible = false;
        }
    }
}
