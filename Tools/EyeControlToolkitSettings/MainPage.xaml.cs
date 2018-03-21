//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Research.Input.Gaze;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EyeControlToolkitSettings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GazePointer _gazePointer;

        public MainPage()
        {
            InitializeComponent();

            _gazePointer = new GazePointer(this);
            _gazePointer.OnGazePointerEvent += OnGazePointerEvent;

            BetaTextbox.Text = App._gazeSettings.OneEuroFilter_Beta.ToString();
            CutoffTextbox.Text = App._gazeSettings.OneEuroFilter_Cutoff.ToString();
            VelocityCutoffTextbox.Text = App._gazeSettings.OneEuroFilter_Velocity_Cutoff.ToString();
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }

        private void GetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            BetaTextbox.Text = App._gazeSettings.OneEuroFilter_Beta.ToString();
            CutoffTextbox.Text = App._gazeSettings.OneEuroFilter_Cutoff.ToString();
            VelocityCutoffTextbox.Text = App._gazeSettings.OneEuroFilter_Velocity_Cutoff.ToString();
        }

        private void SetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App._gazeSettings.OneEuroFilter_Beta = float.Parse(BetaTextbox.Text);
            App._gazeSettings.OneEuroFilter_Cutoff = float.Parse(CutoffTextbox.Text);
            App._gazeSettings.OneEuroFilter_Velocity_Cutoff = float.Parse(VelocityCutoffTextbox.Text);
        }
    }
}
