//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Toolkit.Uwp.Input.Gaze;
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
        public GazeSettings GazeSettings = new GazeSettings();

        public MainPage()
        {
            InitializeComponent();

            var localSettings = new ValueSet();
            GazeSettings.ValueSetFromLocalSettings(localSettings);

            var gazePointer = GazeApi.GetGazePointer(this);
            gazePointer.LoadSettings(localSettings);

            GazeSettings.GazePointer = gazePointer;
        }

        #region Button Handlers
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
