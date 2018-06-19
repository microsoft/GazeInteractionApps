//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;

namespace ControlTest
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            CursorVisible.IsChecked = GazeInput.GetIsCursorVisible(this);

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                var gazePointer = GazeInput.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);

                CursorVisible.IsChecked = GazeInput.GetIsCursorVisible(this);
            });
        }

        #region Default Invokable Controls Handlers
        uint _button1ClickCount = 0;
        uint _togglebutton1ClickCount = 0;

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_Button1.Text = $"Clicks = {++_button1ClickCount}";
        }

        private void ToggleButton1_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock_ToggleButton1.Text = $"Checks = {++_togglebutton1ClickCount}";
        }
        #endregion

        #region Timing Button Handlers
        uint _TimingButton200msClickCount = 0;
        uint _TimingButton400msClickCount = 0;
        uint _TimingButton800msClickCount = 0;
        uint _TimingButton1600msClickCount = 0;
        uint _TimingButton2400msClickCount = 0;

        private void TimingButton200ms_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_TimingButton200ms.Text = $"Clicks = {++_TimingButton200msClickCount}";
        }

        private void TimingButton400ms_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_TimingButton400ms.Text = $"Clicks = {++_TimingButton400msClickCount}";
        }

        private void TimingButton800ms_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_TimingButton800ms.Text = $"Clicks = {++_TimingButton800msClickCount}";
        }

        private void TimingButton1600ms_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_TimingButton1600ms.Text = $"Clicks = {++_TimingButton1600msClickCount}";
        }

        private void TimingButton2400ms_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_TimingButton2400ms.Text = $"Clicks = {++_TimingButton2400msClickCount}";
        }
        #endregion

        #region Repeat Button Handlers
        uint _Repeat0ButtonClickCount = 0;
        uint _Repeat1ButtonClickCount = 0;
        uint _Repeat2ButtonClickCount = 0;
        uint _Repeat9ButtonClickCount = 0;

        private void Button0Repeat_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_Button0Repeat.Text = $"Clicks = {++_Repeat0ButtonClickCount}";
        }

        private void Button1Repeat_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_Button1Repeat.Text = $"Clicks = {++_Repeat1ButtonClickCount}";
        }

        private void Button2Repeat_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_Button2Repeat.Text = $"Clicks = {++_Repeat2ButtonClickCount}";
        }

        private void Button9Repeat_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_Button9Repeat.Text = $"Clicks = {++_Repeat9ButtonClickCount}";
        }
        #endregion

        #region Invoke Handing Handlers
        uint _DefaultInvokeClickCount = 0;
        uint _GazeInvokeCount = 0;
        uint _GazeInvokeOrClick_GazeCount = 0;
        uint _GazeInvokeOrClick_ClickCount = 0;

        private void ButtonDefaultInvoke_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_ButtonDefaultInvoke.Text = $"Gaze OR Click = {++_DefaultInvokeClickCount}";
        }
        private void GazeInvoke_Only_Invoked(object sender, DwellInvokedRoutedEventArgs e)
        {
            TextBlock_ButtonGazeInvoke.Text = $"Gazes = {++_GazeInvokeCount}";
            e.Handled = true;
        }

        private void ButtonGazeInvokeOrClick_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_ButtonGazeInvokeOrClick.Text = $"Gaze = {_GazeInvokeOrClick_GazeCount} \r\nClick = {++_GazeInvokeOrClick_ClickCount}";
        }

        private void GazeInvokeOrClick_Invoked(object sender, DwellInvokedRoutedEventArgs e)
        {
            TextBlock_ButtonGazeInvokeOrClick.Text = $"Gaze = {++_GazeInvokeOrClick_GazeCount} \r\nClick = {_GazeInvokeOrClick_ClickCount}";
            e.Handled = true;
        }
        #endregion

        #region Cursor Settings
        private void CursorVisible_Checked(object sender, RoutedEventArgs e)
        {
            GazeInput.SetIsCursorVisible(this, true);
        }

        private void CursorVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            GazeInput.SetIsCursorVisible(this, false);
        }

        private void CursorRadius5_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.SetCursorRadius(this, 5);
        }

        private void CursorRadius10_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.SetCursorRadius(this, 10);
        }

        private void CursorRadius20_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.SetCursorRadius(this, 20);
        }

        private void CursorRadius50_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.SetCursorRadius(this, 50);
        }
        #endregion
    }
}
