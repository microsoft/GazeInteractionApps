//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Storage;
using Microsoft.Research.Input.Gaze;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace EyeControlToolkitSettings
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private AppServiceConnection _appServiceConnection;
        private BackgroundTaskDeferral _appServiceDeferral;
        public static GazeSettings _gazeSettings;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            _gazeSettings = GazeSettings.Instance;

            GazeSettingsFromLocalSettings(_gazeSettings);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            IBackgroundTaskInstance taskInstance = args.TaskInstance;
            AppServiceTriggerDetails appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            _appServiceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnAppServicesCanceled;
            _appServiceConnection = appService.AppServiceConnection;
            _appServiceConnection.RequestReceived += OnAppServiceRequestReceived;
            _appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
        }

        private async void OnAppServiceRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            AppServiceDeferral messageDeferral = args.GetDeferral();

            await args.Request.SendResponseAsync(ValueSetFromGazeSettings(GazeSettings.Instance));
            messageDeferral.Complete();
        }

        private void OnAppServicesCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _appServiceDeferral.Complete();
        }

        private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _appServiceDeferral.Complete();
        }

        private void GazeSettingsFromLocalSettings(GazeSettings gazeSettings)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["OneEuroFilter_Beta"] != null) { _gazeSettings.OneEuroFilter_Beta = (float)localSettings.Values["OneEuroFilter_Beta"]; }
            if (localSettings.Values["OneEuroFilter_Cutoff"] != null) { _gazeSettings.OneEuroFilter_Cutoff = (float)localSettings.Values["OneEuroFilter_Cutoff"]; }
            if (localSettings.Values["OneEuroFilter_Velocity_Cutoff"] != null) { _gazeSettings.OneEuroFilter_Velocity_Cutoff = (float)localSettings.Values["OneEuroFilter_Velocity_Cutoff"]; }

            if (localSettings.Values["GazePointer_Fixation_Delay"] != null) { _gazeSettings.GazePointer_Fixation_Delay = (int)localSettings.Values["GazePointer_Fixation_Delay"]; }
            if (localSettings.Values["GazePointer_Dwell_Delay"] != null) { _gazeSettings.GazePointer_Dwell_Delay = (int)localSettings.Values["GazePointer_Dwell_Delay"]; }
            if (localSettings.Values["GazePointer_Repeat_Delay"] != null) { _gazeSettings.GazePointer_Repeat_Delay = (int)localSettings.Values["GazePointer_Repeat_Delay"]; }
            if (localSettings.Values["GazePointer_Enter_Exit_Delay"] != null) { _gazeSettings.GazePointer_Enter_Exit_Delay = (int)localSettings.Values["GazePointer_Enter_Exit_Delay"]; }
            if (localSettings.Values["GazePointer_Max_History_Duration"] != null) { _gazeSettings.GazePointer_Max_History_Duration = (int)localSettings.Values["GazePointer_Max_History_Duration"]; }
            if (localSettings.Values["GazePointer_Max_Single_Sample_Duration"] != null) { _gazeSettings.GazePointer_Max_Single_Sample_Duration = (int)localSettings.Values["GazePointer_Max_Single_Sample_Duration"]; }
            if (localSettings.Values["GazePointer_Gaze_Idle_Time"] != null) { _gazeSettings.GazePointer_Gaze_Idle_Time = (int)localSettings.Values["GazePointer_Gaze_Idle_Time"]; }

            if (localSettings.Values["GazeCursor_Cursor_Radius"] != null) { _gazeSettings.GazeCursor_Cursor_Radius = (int)localSettings.Values["GazeCursor_Cursor_Radius"]; }
            if (localSettings.Values["GazeCursor_Cursor_Visibility"] != null) { _gazeSettings.GazeCursor_Cursor_Visibility = (bool)localSettings.Values["GazeCursor_Cursor_Visibility"]; }
        }

        private void LocalSettingsFromGazeSettings(GazeSettings gazeSettings)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["OneEuroFilter_Beta"] = _gazeSettings.OneEuroFilter_Beta;
            localSettings.Values["OneEuroFilter_Cutoff"] = _gazeSettings.OneEuroFilter_Cutoff;
            localSettings.Values["OneEuroFilter_Velocity_Cutoff"] = _gazeSettings.OneEuroFilter_Velocity_Cutoff;

            localSettings.Values["GazePointer_Fixation_Delay"] = _gazeSettings.GazePointer_Fixation_Delay;
            localSettings.Values["GazePointer_Dwell_Delay"] = _gazeSettings.GazePointer_Dwell_Delay;
            localSettings.Values["GazePointer_Repeat_Delay"] = _gazeSettings.GazePointer_Repeat_Delay;
            localSettings.Values["GazePointer_Enter_Exit_Delay"] = _gazeSettings.GazePointer_Enter_Exit_Delay;
            localSettings.Values["GazePointer_Max_History_Duration"] = _gazeSettings.GazePointer_Max_History_Duration;
            localSettings.Values["GazePointer_Max_Single_Sample_Duration"] = _gazeSettings.GazePointer_Max_Single_Sample_Duration;
            localSettings.Values["GazePointer_Gaze_Idle_Time"] = _gazeSettings.GazePointer_Gaze_Idle_Time;

            localSettings.Values["GazeCursor_Cursor_Radius"] = _gazeSettings.GazeCursor_Cursor_Radius;
            localSettings.Values["GazeCursor_Cursor_Visibility"] = _gazeSettings.GazeCursor_Cursor_Visibility;
        }

        private ValueSet ValueSetFromGazeSettings(GazeSettings gazeSettings)
        {
            var result = new ValueSet
            {
                { "OneEuroFilter_Beta", gazeSettings.OneEuroFilter_Beta },
                { "OneEuroFilter_Cutoff", gazeSettings.OneEuroFilter_Cutoff },
                { "OneEuroFilter_Velocity_Cutoff", gazeSettings.OneEuroFilter_Velocity_Cutoff },

                { "GazePointer_Fixation_Delay", gazeSettings.GazePointer_Fixation_Delay },
                { "GazePointer_Dwell_Delay", gazeSettings.GazePointer_Dwell_Delay },
                { "GazePointer_Repeat_Delay", gazeSettings.GazePointer_Repeat_Delay },
                { "GazePointer_Enter_Exit_Delay", gazeSettings.GazePointer_Enter_Exit_Delay },
                { "GazePointer_Max_History_Duration", gazeSettings.GazePointer_Max_History_Duration },
                { "GazePointer_Max_Single_Sample_Duration", gazeSettings.GazePointer_Max_Single_Sample_Duration },
                { "GazePointer_Gaze_Idle_Time", gazeSettings.GazePointer_Gaze_Idle_Time },

                { "GazeCursor_Cursor_Radius", gazeSettings.GazeCursor_Cursor_Radius },
                { "GazeCursor_Cursor_Visibility", gazeSettings.GazeCursor_Cursor_Visibility }
            };

            return result;
        }
    }
}
