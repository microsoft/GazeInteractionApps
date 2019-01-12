//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using System.Numerics;
using Windows.UI.Xaml.Media;
using Windows.UI;


namespace Memory
{
    public sealed partial class GamePage : Page
    {
        const byte MIN_CHAR = 0x21;
        const byte MAX_CHAR = 0xE8;

        Random _rnd;
        Button _firstButton;
        Button _secondButton;
        DispatcherTimer _flashTimer;
        int _remaining;
        int _numMoves;
        bool _usePictures;

        public GamePage()
        {
            InitializeComponent();

            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);

            _rnd = new Random();
            _flashTimer = new DispatcherTimer();
            _flashTimer.Interval = new TimeSpan(0, 0, 2);
            _flashTimer.Tick += OnFlashTimerTick;
            _usePictures = false;

            Loaded += MainPage_Loaded;

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
                var gazePointer = GazeInput.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _usePictures = (bool)e.Parameter;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ResetBoard();
        }

        private void OnFlashTimerTick(object sender, object e)
        {
            _firstButton.Content = null;
            _secondButton.Content = null;
            _firstButton = null;
            _secondButton = null;
            _flashTimer.Stop();
        }

        List<Button> ShuffleList(List<Button> list)
        {
            var len = list.Count;
            for (var i = 0; i < len; i++)
            {
                var j = _rnd.Next(0, len);
                var k = list[i];
                list[i] = list[j];
                list[j] = k;
            }
            return list;
        }

        async Task<List<string>> GetPicturesContent(int len)
        {
            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;            
            IReadOnlyList<StorageFile> pictures = await picturesFolder.GetFilesAsync();
            List<string> list = new List<string>();

            for (int i = 0; i < len; i++)
            {
                string filename;
                do
                {
                    filename = pictures[_rnd.Next(pictures.Count)].Path;
                }
                while (list.Contains(filename));

                list.Add(filename.ToString());
            }
            return list;
        }

        List<string> GetSymbolContent(int len)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < len; i++)
            {
                char ch;
                do
                {
                    ch = Convert.ToChar(_rnd.Next(MIN_CHAR, MAX_CHAR));
                }
                while (list.Contains(ch.ToString()));

                list.Add(ch.ToString());
            }
            return list;
        }

        List<Button> GetButtonList()
        {
            List<Button> list = new List<Button>();
            foreach (Button button in buttonMatrix.Children)
            {
                list.Add(button);
            }
            return ShuffleList(list);
        }

        async void ResetBoard()
        {
            _firstButton = null;
            _secondButton = null;
            _numMoves = 0;
            _remaining = 16;

            List<string> listContent;
            if (_usePictures)
            {
                try
                {                
                    listContent = await GetPicturesContent(8);
                }
                catch
                {
                    listContent = GetSymbolContent(8);
                    _usePictures = false;
                }
            }
            else
            {
                listContent = GetSymbolContent(8);
            }

            List<Button> listButtons = GetButtonList();

            for (int i = 0; i < 16; i += 2)
            {
                listButtons[i].Content = null;
                listButtons[i + 1].Content = null;

                listButtons[i].Tag = listContent[i / 2];
                listButtons[i + 1].Tag = listContent[i / 2];
            }
        }

        private async void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Content != null)
            {
                return;
            }
            if (_flashTimer.IsEnabled)
            {
                OnFlashTimerTick(null, null);
            }

            _numMoves++;

            if (_firstButton == null)
            {
                _firstButton = btn;
            }
            else
            {
                _secondButton = btn;
            }

            //Flip button visual
            var btnVisual = ElementCompositionPreview.GetElementVisual(btn);
            var compositor = btnVisual.Compositor;

            //Get a visual for the content
            var btnContent = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(btn, 0), 0);
            var btnContentVisual = ElementCompositionPreview.GetElementVisual(btnContent as FrameworkElement);

            var easing = compositor.CreateLinearEasingFunction();

            ScalarKeyFrameAnimation flipAnimation = compositor.CreateScalarKeyFrameAnimation();
            flipAnimation.InsertKeyFrame(0.000001f, 180);
            flipAnimation.InsertKeyFrame(1f, 0, easing);
            flipAnimation.Duration = TimeSpan.FromMilliseconds(800);
            flipAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            flipAnimation.IterationCount = 1;
            btnVisual.CenterPoint = new Vector3((float)(0.5 * btn.ActualWidth), (float)(0.5f * btn.ActualHeight), (float)(btn.ActualWidth / 4));
            btnVisual.RotationAxis = new Vector3(0.0f, 1f, 0f);

            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(0.0f, 0);
            appearAnimation.InsertKeyFrame(0.499999f, 0);
            appearAnimation.InsertKeyFrame(0.5f, 1);
            appearAnimation.InsertKeyFrame(1f, 1);
            appearAnimation.Duration = TimeSpan.FromMilliseconds(800);
            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;

            btnVisual.StartAnimation(nameof(btnVisual.RotationAngleInDegrees), flipAnimation);
            btnContentVisual.StartAnimation(nameof(btnContentVisual.Opacity), appearAnimation);

            if (_usePictures)
            {
                var file = await StorageFile.GetFileFromPathAsync(btn.Tag.ToString());
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var image = new Image();
                    var bmp = new BitmapImage();
                    await bmp.SetSourceAsync(stream);
                    image.Source = bmp;
                    btn.Content = image;
                }
            }
            else
            {
                btn.Content = btn.Tag.ToString();
            }

            if (_secondButton == null)
            {
                return;
            }

            if (_secondButton.Tag.ToString() != _firstButton.Tag.ToString())
            {
                _flashTimer.Start();
            }
            else
            {
                _firstButton = null;
                _secondButton = null;
                _remaining -= 2;

                CheckGameCompletion();
            }
        }

        void CheckGameCompletion()
        {
            if (_remaining > 0)
            {
                return;
            }

            string message = $"Congratulations!! You solved it in {_numMoves} moves";
            DialogText.Text = message;
            DialogGrid.Visibility = Visibility.Visible;           
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            DialogGrid.Visibility = Visibility.Collapsed;
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void OnBack(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}

