using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Memory
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            VersionTextBlock.Text = GetAppVersion();

            CoreWindow.GetForCurrentThread().KeyDown += new Windows.Foundation.TypedEventHandler<CoreWindow, KeyEventArgs>(delegate (CoreWindow sender, KeyEventArgs args) {
                GazeInput.GetGazePointer(this).Click();
            });

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                GazeInput.LoadSettings(sharedSettings);
            });
        }

        private void OnStartGame(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            bool usePictures = int.Parse(button.Tag.ToString()) == 1;
            Frame.Navigate(typeof(GamePage), usePictures);
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

    }
}
