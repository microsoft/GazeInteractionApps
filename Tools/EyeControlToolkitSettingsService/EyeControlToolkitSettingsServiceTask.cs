using System;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace EyeControlToolkitSettingsService
{
    public sealed class EyeControlToolkitSettingsServiceTask : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        AppServiceConnection connection;
        static Random randomNumberGenerator;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //Take a service deferral so the service isn't terminated
            serviceDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += OnTaskCanceled;

            //Initialize the random number generator
            randomNumberGenerator = new Random((int)DateTime.Now.Ticks);

            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            connection = details.AppServiceConnection;

            //Listen for incoming app service requests
            connection.RequestReceived += OnRequestReceived;
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (serviceDeferral != null)
            {
                //Complete the service deferral
                serviceDeferral.Complete();
                serviceDeferral = null;
            }

            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //Get a deferral so we can use an awaitable API to respond to the message
            var messageDeferral = args.GetDeferral();

            try
            {
                var input = args.Request.Message;
                int minValue = (int)input["minvalue"];
                int maxValue = (int)input["maxvalue"];

                //Create the response
                //var result = new ValueSet();
                //result.Add("result", randomNumberGenerator.Next(minValue, maxValue));
                

                //Send the response
                await args.Request.SendResponseAsync(SettingsValues);

            }
            finally
            {
                //Complete the message deferral so the platform knows we're done responding
                messageDeferral.Complete();
            }
        }

        public static ValueSet SettingsValues
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                //Create the response
                var result = new ValueSet
                {
                    { "result", randomNumberGenerator.Next(10, 20) },
                    { "beta", localSettings.Values["beta"] ?? 5.0f },
                    { "cutoff", localSettings.Values["cutoff"] ?? 0.1f },
                    { "velocity_cutoff", localSettings.Values["velocity_cutoff"] ?? 1.0f }
                };

                return result;
            }
            set
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                foreach (var setting in value)
                {
                    localSettings.Values[setting.Key] = setting.Value;
                }
            }
        }
    }
}
