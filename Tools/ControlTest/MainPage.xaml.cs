//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.UWP.Input.Gaze;
using Windows.UI.Popups;
using Windows.Foundation.Collections;
using Windows.Foundation;

namespace ControlTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        uint _button1ClickCount = 0;
        uint _togglebutton1ClickCount = 0;

        uint _TimingButton200msClickCount = 0;
        uint _TimingButton400msClickCount = 0;
        uint _TimingButton800msClickCount = 0;
        uint _TimingButton1600msClickCount = 0;
        uint _TimingButton2400msClickCount = 0;

        public MainPage()
        {
            this.InitializeComponent();

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
                var gazePointer = GazeApi.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);
            });
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_Button1.Text = $"Clicks = {++_button1ClickCount}";
        }

        private void ToggleButton1_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock_ToggleButton1.Text = $"Checks = {++_togglebutton1ClickCount}";
        }

        private void MessageDialog_Click(object sender, RoutedEventArgs e)
        {
            ShowMessageDialog();
        }

        private void ContentDialog_Click(object sender, RoutedEventArgs e)
        {
            ShowContentDialog();
        }

        async void ShowMessageDialog()
        {
            string message = $"Congratulations!! You have a MessageDialog";
            MessageDialog dlg = new MessageDialog(message);
            await dlg.ShowAsync();
        }

        async void ShowContentDialog()
        {
            ContentDialog dlg = new ContentDialog()
            {
                Title = "I am a content Dialog",
                Content = "There is content here.",
                CloseButtonText = "Ok"
            };
            await dlg.ShowAsync();
        }

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
    }
}
