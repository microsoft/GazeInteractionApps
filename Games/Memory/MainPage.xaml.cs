using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Memory
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                var gazePointer = GazeInput.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);

                CoreWindow.GetForCurrentThread().KeyDown += new Windows.Foundation.TypedEventHandler<CoreWindow, KeyEventArgs>(delegate (CoreWindow sender, KeyEventArgs args) {
                    gazePointer.Click();
                });
            });
        }

        private void OnStartGame(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            bool usePictures = int.Parse(button.Tag.ToString()) == 1;
            Frame.Navigate(typeof(GamePage), usePictures);
        }

    }
}
