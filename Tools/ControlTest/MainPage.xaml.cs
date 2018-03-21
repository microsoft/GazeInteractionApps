//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Research.Input.Gaze;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ControlTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GazePointer _gazePointer;
        uint _button1ClickCount = 0;
        uint _togglebutton1ClickCount = 0;

        public MainPage()
        {
            this.InitializeComponent();

            GazeSettings.RetrieveSharedSettings(GazeSettings.Instance);

            _gazePointer = new GazePointer(this);
            _gazePointer.OnGazePointerEvent += OnGazePointerEvent;
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
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


    }
}
