using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fifteen
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            CoreWindow.GetForCurrentThread().KeyDown += new Windows.Foundation.TypedEventHandler<CoreWindow, KeyEventArgs>(delegate (CoreWindow sender, KeyEventArgs args) {
                GazeInput.GetGazePointer(null).Click();
            });

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                GazeInput.LoadSettings(sharedSettings);
            });
        }

        private void OnBoardSizeSelected(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var boardSize = int.Parse(button.Tag.ToString());
            Frame.Navigate(typeof(GamePage), boardSize);
        }
    }
}
