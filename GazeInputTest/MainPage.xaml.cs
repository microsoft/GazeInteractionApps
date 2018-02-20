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
        public MainPage()
        {
            this.InitializeComponent();

            ShowCursor.IsChecked = GazeApi.GetIsGazeCursorVisible(this);
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            Dwell.Content = ea.PointerState.ToString();
            if (ea.PointerState == GazePointerState.Dwell)
            {
                sender.InvokeTarget(ea.HitTarget);
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
                GazeApi.SetIsGazeCursorVisible(this, ShowCursor.IsChecked.Value);
            }
        }
    }
}
