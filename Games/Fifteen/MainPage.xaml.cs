using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fifteen
{
    public sealed partial class MainPage : Page
    {
        SolidColorBrush _solidTileBrush;

        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            _solidTileBrush = (SolidColorBrush)this.Resources["TileBackground"];

            VersionTextBlock.Text = GetAppVersion();

            CoreWindow.GetForCurrentThread().KeyDown += new Windows.Foundation.TypedEventHandler<CoreWindow, KeyEventArgs>(delegate (CoreWindow sender, KeyEventArgs args) {
                GazeInput.GetGazePointer(null).Click();
            });

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                GazeInput.LoadSettings(sharedSettings);
            });

            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);
            
        }

        private void OnBoardSizeSelected(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var boardSize = int.Parse(button.Tag.ToString());
            Frame.Navigate(typeof(GamePage), boardSize);
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void OnHowToPlayButton(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;

            HelpScreen1.Visibility = Visibility.Visible;
            HelpScreen2.Visibility = Visibility.Collapsed;            
            HelpScreen3.Visibility = Visibility.Collapsed;
            HelpNavLeftButton.IsEnabled = false;
            HelpNavRightButton.IsEnabled = true;
            
            HelpDialogGrid.Visibility = Visibility.Visible;
        }

        private void OnHelpNavRight(object sender, RoutedEventArgs e)
        {
            if (HelpScreen1.Visibility == Visibility.Visible)
            {
                HelpScreen1.Visibility = Visibility.Collapsed;
                HelpScreen2.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = true;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if(HelpScreen2.Visibility == Visibility.Visible)
            {
                HelpScreen2.Visibility = Visibility.Collapsed;
                HelpScreen3.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = false;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if (HelpScreen3.Visibility == Visibility.Visible)
            {
                HelpNavRightButton.IsEnabled = false;
                HelpNavLeftButton.IsEnabled = true;
            }
        }

        private void OnHelpNavLeft(object sender, RoutedEventArgs e)
        {
            if (HelpScreen1.Visibility == Visibility.Visible)
            {
                HelpNavLeftButton.IsEnabled = false;
                HelpNavRightButton.IsEnabled = true;
            }
            else if (HelpScreen2.Visibility == Visibility.Visible)
            {
                HelpScreen2.Visibility = Visibility.Collapsed;
                HelpScreen1.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = false;
                HelpNavRightButton.IsEnabled = true;
            }
            else if (HelpScreen3.Visibility == Visibility.Visible)
            {
                HelpScreen3.Visibility = Visibility.Collapsed;
                HelpScreen2.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = true;
                HelpNavRightButton.IsEnabled = true;
            }
        }

        private void DismissButton(object sender, RoutedEventArgs e)
        {
            HelpDialogGrid.Visibility = Visibility.Collapsed;
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
        }
    }
}
