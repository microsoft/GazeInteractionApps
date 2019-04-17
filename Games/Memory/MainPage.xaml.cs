//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

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

namespace Memory
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
                GazeInput.GetGazePointer(this).Click();
            });

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                GazeInput.LoadSettings(sharedSettings);
            });

            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);
        }

        private void OnStartGame(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tileCount = int.Parse(button.Tag.ToString());

            Frame.Navigate(typeof(GamePage), tileCount);
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void OnHowToPlayButton(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;

            HelpScreen1.Visibility = Visibility.Visible;
            HelpScreen2.Visibility = Visibility.Collapsed;
            HelpScreen3.Visibility = Visibility.Collapsed;
            HelpScreen4.Visibility = Visibility.Collapsed;
            HelpScreen5.Visibility = Visibility.Collapsed;
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
            else if (HelpScreen2.Visibility == Visibility.Visible)
            {
                HelpScreen2.Visibility = Visibility.Collapsed;
                HelpScreen3.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = true;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if (HelpScreen3.Visibility == Visibility.Visible)
            {
                HelpScreen3.Visibility = Visibility.Collapsed;
                HelpScreen4.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = true;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if (HelpScreen4.Visibility == Visibility.Visible)
            {
                HelpScreen4.Visibility = Visibility.Collapsed;
                HelpScreen5.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = false;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if (HelpScreen5.Visibility == Visibility.Visible)
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

            else if (HelpScreen4.Visibility == Visibility.Visible)
            {
                HelpScreen4.Visibility = Visibility.Collapsed;
                HelpScreen3.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = true;
                HelpNavRightButton.IsEnabled = true;
            }
            else if (HelpScreen5.Visibility == Visibility.Visible)
            {
                HelpScreen5.Visibility = Visibility.Collapsed;
                HelpScreen4.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = true;
                HelpNavRightButton.IsEnabled = true;
            }
        }

        private async void PrivacyViewScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            PrivacyWebView.InvokeScriptAsync("eval", new string[] { "window.scrollBy(0,-" + PrivacyWebView.ActualHeight / 2 + ") " });
        }

        private void PrivacyViewScrollDownButton_Click(object sender, RoutedEventArgs e)
        {
            PrivacyWebView.InvokeScriptAsync("eval", new string[] { "window.scrollBy(0," + PrivacyWebView.ActualHeight / 2 + ") " });
        }

        private void PrivacyViewContinueButton_Click(object sender, RoutedEventArgs e)
        {
            PrivacyViewGrid.Visibility = Visibility.Collapsed;
        }

        private void PrivacyHyperlink_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.Transparent);
            WebViewLoadingText.Visibility = Visibility.Visible;
            PrivacyWebView.Navigate(new System.Uri("https://go.microsoft.com/fwlink/?LinkId=521839"));
            PrivacyViewGrid.Visibility = Visibility.Visible;
        }

        private void UseTermsHyperlink_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.Transparent);
            WebViewLoadingText.Visibility = Visibility.Visible;
            PrivacyWebView.Navigate(new System.Uri("https://www.microsoft.com/en-us/servicesagreement/default.aspx"));
            PrivacyViewGrid.Visibility = Visibility.Visible;
        }

        private void PrivacyWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;
            WebViewLoadingText.Visibility = Visibility.Collapsed;
        }

        private void DismissButton(object sender, RoutedEventArgs e)
        {
            HelpDialogGrid.Visibility = Visibility.Collapsed;
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
