//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Research.Input.Gaze;
using Windows.UI.Xaml.Controls;

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
            LoadSettingsToUserControls();
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
            GazeSettingsHelpers.GazeSettingsFromLocalSettings(App._gazeSettings);

            LoadSettingsToUserControls();
        }

        private void SetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SaveSettingsFromUserControls();

            GazeSettingsHelpers.LocalSettingsFromGazeSettings(App._gazeSettings);
        }

        private void LoadSettingsToUserControls()
        {
            OneEuroFilter_Beta_Textbox.Text = App._gazeSettings.OneEuroFilter_Beta.ToString();
            OneEuroFilter_Cutoff_Textbox.Text = App._gazeSettings.OneEuroFilter_Cutoff.ToString();
            OneEuroFilter_Velocity_Cutoff_Textbox.Text = App._gazeSettings.OneEuroFilter_Velocity_Cutoff.ToString();

            GazePointer_Fixation_Delay_Textbox.Text = App._gazeSettings.GazePointer_Fixation_Delay.ToString();
            GazePointer_Dwell_Delay_Textbox.Text = App._gazeSettings.GazePointer_Dwell_Delay.ToString();
            GazePointer_Repeat_Delay_Textbox.Text = App._gazeSettings.GazePointer_Repeat_Delay.ToString();
            GazePointer_Enter_Exit_Delay_Textbox.Text = App._gazeSettings.GazePointer_Enter_Exit_Delay.ToString();
            GazePointer_Max_History_Duration_Textbox.Text = App._gazeSettings.GazePointer_Max_History_Duration.ToString();
            GazePointer_Max_Single_Sample_Duration_Textbox.Text = App._gazeSettings.GazePointer_Max_Single_Sample_Duration.ToString();
            GazePointer_Gaze_Idle_Time_Textbox.Text = App._gazeSettings.GazePointer_Gaze_Idle_Time.ToString();

            GazeCursor_Cursor_Radius_Textbox.Text = App._gazeSettings.GazeCursor_Cursor_Radius.ToString();
            GazeCursor_Cursor_Visibility_Textbox.Text = App._gazeSettings.GazeCursor_Cursor_Visibility.ToString();
        }

        private void SaveSettingsFromUserControls()
        {
            App._gazeSettings.OneEuroFilter_Beta = float.Parse(OneEuroFilter_Beta_Textbox.Text);
            App._gazeSettings.OneEuroFilter_Cutoff = float.Parse(OneEuroFilter_Cutoff_Textbox.Text);
            App._gazeSettings.OneEuroFilter_Velocity_Cutoff = float.Parse(OneEuroFilter_Velocity_Cutoff_Textbox.Text);

            App._gazeSettings.GazePointer_Fixation_Delay = int.Parse(GazePointer_Fixation_Delay_Textbox.Text);
            App._gazeSettings.GazePointer_Dwell_Delay = int.Parse(GazePointer_Dwell_Delay_Textbox.Text);
            App._gazeSettings.GazePointer_Repeat_Delay = int.Parse(GazePointer_Repeat_Delay_Textbox.Text);
            App._gazeSettings.GazePointer_Enter_Exit_Delay = int.Parse(GazePointer_Enter_Exit_Delay_Textbox.Text);
            App._gazeSettings.GazePointer_Max_History_Duration = int.Parse(GazePointer_Max_History_Duration_Textbox.Text);
            App._gazeSettings.GazePointer_Max_Single_Sample_Duration = int.Parse(GazePointer_Max_Single_Sample_Duration_Textbox.Text);
            App._gazeSettings.GazePointer_Gaze_Idle_Time = int.Parse(GazePointer_Gaze_Idle_Time_Textbox.Text);

            App._gazeSettings.GazeCursor_Cursor_Radius = int.Parse(GazeCursor_Cursor_Radius_Textbox.Text);
            App._gazeSettings.GazeCursor_Cursor_Visibility = bool.Parse(GazeCursor_Cursor_Visibility_Textbox.Text);
        }
    }
}
