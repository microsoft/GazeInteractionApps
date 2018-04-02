//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Toolkit.UWP.Input.Gaze;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

namespace EyeControlToolkitSettings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GazePointer _gazePointer;
        public GazeSettings GazeSettings = new GazeSettings();

        public MainPage()
        {
            InitializeComponent();

            var localSettings = new ValueSet();
            GazeSettings.ValueSetFromLocalSettings(localSettings);

            _gazePointer = new GazePointer(this);
            _gazePointer.LoadSettings(localSettings);
            _gazePointer.OnGazePointerEvent += OnGazePointerEvent;

            GazeSettings.GazePointer = _gazePointer;
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }

        private void OneEuroFilter_Beta_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilter_Beta -= 1f;
        }

        private void OneEuroFilter_Beta_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilter_Beta += 1f;
        }

        private void OneEuroFilter_Cutoff_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilter_Cutoff -= 0.01f;
        }

        private void OneEuroFilter_Cutoff_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilter_Cutoff += 0.01f;
        }

        private void OneEuroFilter_VelocityCutoff_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilter_VelocityCutoff -= 0.1f;
        }

        private void OneEuroFilter_VelocityCutoff_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilter_VelocityCutoff += 0.1f;
        }

        private void GazePointer_FixationDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_FixationDelay -= 50000;
        }

        private void GazePointer_FixationDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_FixationDelay += 50000;
        }

        private void GazePointer_DwellDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_DwellDelay -= 50000;
        }

        private void GazePointer_DwellDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_DwellDelay -= 50000;
        }

        private void GazePointer_RepeatDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointer_RepeatDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointer_EnterExitDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_EnterExitDelay -= 50000;
        }

        private void GazePointer_EnterExitDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_EnterExitDelay += 50000;
        }

        private void GazePointer_MaxHistoryDuration_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointer_MaxHistoryDuration_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointer_MaxSingleSampleDuration_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointer_MaxSingleSampleDuration_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointer_GazeIdleTime_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_GazeIdleTime -= 10000;
        }

        private void GazePointer_GazeIdleTime_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointer_GazeIdleTime += 10000;
        }

        private void GazeCursor_CursorRadius_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazeCursor_CursorRadius -= 5;
        }

        private void GazeCursor_CursorRadius_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazeCursor_CursorRadius += 5;
        }
    }
}
