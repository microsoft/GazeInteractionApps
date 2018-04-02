//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Toolkit.UWP.Input.Gaze;
using Windows.Foundation.Collections;
using Windows.Storage;
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

        #region Button Handlers
        private void OneEuroFilterBeta_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilterBeta -= 1f;
        }

        private void OneEuroFilterBeta_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilterBeta += 1f;
        }

        private void OneEuroFilterCutoff_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilterCutoff -= 0.01f;
        }

        private void OneEuroFilterCutoff_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilterCutoff += 0.01f;
        }

        private void OneEuroFilterVelocityCutoff_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilterVelocityCutoff -= 0.1f;
        }

        private void OneEuroFilterVelocityCutoff_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.OneEuroFilterVelocityCutoff += 0.1f;
        }

        private void GazePointerFixationDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerFixationDelay -= 50000;
        }

        private void GazePointerFixationDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerFixationDelay += 50000;
        }

        private void GazePointerDwellDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerDwellDelay -= 50000;
        }

        private void GazePointerDwellDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerDwellDelay -= 50000;
        }

        private void GazePointerRepeatDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointerRepeatDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointerEnterExitDelay_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerEnterExitDelay -= 50000;
        }

        private void GazePointerEnterExitDelay_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerEnterExitDelay += 50000;
        }

        private void GazePointerMaxHistoryDuration_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointerMaxHistoryDuration_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointerMaxSingleSampleDuration_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointerMaxSingleSampleDuration_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void GazePointerGazeIdleTime_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerGazeIdleTime -= 10000;
        }

        private void GazePointerGazeIdleTime_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazePointerGazeIdleTime += 10000;
        }

        private void GazeCursorRadius_NudgeDown_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazeCursorRadius -= 5;
        }

        private void GazeCursorRadius_NudgeUp_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GazeSettings.GazeCursorRadius += 5;
        }

        private void ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
            GazeSettings.Reset();
        }
        #endregion
    }
}
