//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Memory
{
    public sealed partial class GamePage : Page
    {
        const byte MIN_CHAR = 0x21;        
        const byte MAX_CHAR = 0xff;

        Random _rnd;
        Button _firstButton;
        Button _secondButton;
        DispatcherTimer _flashTimer;
        int _remaining;
        int _numMoves;
        bool _usePictures;

        bool _interactionPaused = false;

        bool _animationActive = false;
        bool _reverseAnimationActive = false;

        bool _gameOver = false;

        SolidColorBrush _solidTileBrush;        
        SolidColorBrush _toolButtonBrush;
        SolidColorBrush _pausedButtonBrush = new SolidColorBrush(Colors.Black);

        int _boardRows = 6;
        int _boardColumns = 11;

        CompositionScopedBatch _reverseFlipBatchAnimation;
        CompositionScopedBatch _flipBatchAnimation;
        CompositionScopedBatch _resetBatchAnimation;

        public GamePage()
        {
            InitializeComponent();

            _solidTileBrush = (SolidColorBrush)this.Resources["TileBackground"];
            _toolButtonBrush = (SolidColorBrush)this.Resources["ToolBarButtonBackground"];

            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);

            _rnd = new Random();
            _flashTimer = new DispatcherTimer();
            _flashTimer.Interval = new TimeSpan(0, 0, 1);
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

            switch (e.Parameter.ToString())
            {
                case "16":                    
                    _boardRows = 4;
                    _boardColumns = 4;
                    break;

                case "24":
                    _boardRows = 4;
                    _boardColumns = 6;
                    break;

                case "36":
                    _boardRows = 6;
                    _boardColumns = 6;
                    break;

                case "66":
                    _boardRows = 6;
                    _boardColumns = 11;
                    break;

                default:
                    _boardRows = 4;
                    _boardColumns = 4;
                    break;
            }
        }

        private ScrollViewer getRootScrollViewer()
        {
            DependencyObject el = this;
            while (el != null && !(el is ScrollViewer))
            {
                el = VisualTreeHelper.GetParent(el);
            }

            return (ScrollViewer)el;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            getRootScrollViewer().Focus(FocusState.Programmatic);

            ArrangeBoardLayout();
            ResetBoard();
        }

        private void ArrangeBoardLayout()
        {
            int pageHeaderButtonCount = 3;
            buttonMatrix.Children.Clear();
            buttonMatrix.RowDefinitions.Clear();
            buttonMatrix.ColumnDefinitions.Clear();

            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log($"NewBoard-ETD:{GazeInput.IsDeviceAvailable}-{_boardRows},{_boardColumns}");

            for (int row = 0; row < _boardRows; row++)
            {
                buttonMatrix.RowDefinitions.Add(new RowDefinition());                
            }

            for (int column = 0; column < _boardColumns; column++)
            {
                buttonMatrix.ColumnDefinitions.Add(new ColumnDefinition());

            }

            for (int row = 0; row < _boardRows; row++)
            {
                for (int col = 0; col < _boardColumns; col++)
                {
                    var button = new Button();
                    button.Name = $"button_{row}_{col}";
                    button.SetValue(AutomationProperties.NameProperty, $"Card {row} {col}");
                    button.TabIndex = (pageHeaderButtonCount - 1) + (int)((row * _boardColumns) + col);                    
                    button.Click += OnButtonClick;
                    button.Style = Resources["ButtonStyle"] as Style;
                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);
                    buttonMatrix.Children.Add(button);
                    button.Content = CreateTargetEllipse();
                }
            }

            buttonMatrix.UpdateLayout();
        }

        private Ellipse CreateTargetEllipse()
        {
            var target = new Ellipse();
            target.Width = 10;
            target.Height = 10;
            target.HorizontalAlignment = HorizontalAlignment.Center;
            target.VerticalAlignment = VerticalAlignment.Center;
            target.Fill = _toolButtonBrush;

            return target;
        }

        private void AdjustFontSizes()
        {            
            var buttons = GetButtonList();
            var shortEdge = buttons[0].ActualHeight;
            if (shortEdge > buttons[0].ActualWidth)
            {
                shortEdge = buttons[0].ActualWidth;
            }

            var tb = new TextBlock();
            tb.FontFamily = buttons[0].FontFamily;
            tb.FontSize = shortEdge * 0.8;
            tb.Text = "g";

            tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var height = tb.DesiredSize.Height;            

            foreach (Button button in buttons)
            {
                button.FontSize = height;
            }
        }

        private void RepositionButtons()
        {
            var buttons = GetButtonList();
            foreach (Button button in buttons)
            {
                var btn1Visual = ElementCompositionPreview.GetElementVisual(button);                
            }
        }

        private void OnFlashTimerTick(object sender, object e)
        {
            _reverseAnimationActive = true;
           
            //Flip button visual
            var btn1Visual = ElementCompositionPreview.GetElementVisual(_firstButton);
            var btn2Visual = ElementCompositionPreview.GetElementVisual(_secondButton);
            var compositor = btn1Visual.Compositor;

            //Get a visual for the content
            var btn1Content = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_firstButton, 0), 0);
            var btn1ContentVisual = ElementCompositionPreview.GetElementVisual(btn1Content as FrameworkElement);
            var btn2Content = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_secondButton, 0), 0);
            var btn2ContentVisual = ElementCompositionPreview.GetElementVisual(btn2Content as FrameworkElement);

            var easing = compositor.CreateLinearEasingFunction();
            
            if (_reverseFlipBatchAnimation != null)
            {
                _reverseFlipBatchAnimation.Completed -= ReverseFlipBatchAnimation_Completed;
                _reverseFlipBatchAnimation.Dispose();
            }

            _reverseFlipBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _reverseFlipBatchAnimation.Completed += ReverseFlipBatchAnimation_Completed;

            ScalarKeyFrameAnimation flipAnimation = compositor.CreateScalarKeyFrameAnimation();
            flipAnimation.InsertKeyFrame(0.000001f, 0);
            flipAnimation.InsertKeyFrame(0.999999f, -180, easing);
            flipAnimation.InsertKeyFrame(1f, 0);
            flipAnimation.Duration = TimeSpan.FromMilliseconds(400);
            flipAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            flipAnimation.IterationCount = 1;            
            btn1Visual.CenterPoint = new Vector3((float)(0.5 * _firstButton.ActualWidth), (float)(0.5f * _firstButton.ActualHeight), 0f);
            btn1Visual.RotationAxis = new Vector3(0.0f, 1f, 0f);
            btn2Visual.CenterPoint = new Vector3((float)(0.5 * _secondButton.ActualWidth), (float)(0.5f * _secondButton.ActualHeight), 0f);
            btn2Visual.RotationAxis = new Vector3(0.0f, 1f, 0f);

            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(0.0f, 1);
            appearAnimation.InsertKeyFrame(0.399999f, 1);
            appearAnimation.InsertKeyFrame(0.4f, 0);
            appearAnimation.InsertKeyFrame(1f, 0);            
            appearAnimation.Duration = TimeSpan.FromMilliseconds(400);
            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;

            btn1Visual.StartAnimation(nameof(btn1Visual.RotationAngleInDegrees), flipAnimation);
            btn2Visual.StartAnimation(nameof(btn2Visual.RotationAngleInDegrees), flipAnimation);
            btn1ContentVisual.StartAnimation(nameof(btn1ContentVisual.Opacity), appearAnimation);
            btn2ContentVisual.StartAnimation(nameof(btn2ContentVisual.Opacity), appearAnimation);
            _reverseFlipBatchAnimation.End();
           
            _flashTimer.Stop();
            GazeInput.SetInteraction(buttonMatrix, Interaction.Enabled);
        }

        void MakeButtonContentOpaque(Button thisButton)
        {
            var thisVisual = ElementCompositionPreview.GetElementVisual(thisButton);            
            var compositor = thisVisual.Compositor;

            //Get a visual for the content
            var thisContent = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(thisButton, 0), 0);
            var thisContentVisual = ElementCompositionPreview.GetElementVisual(thisContent as FrameworkElement);
          
            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(1f, 1);
            appearAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;

            thisContentVisual.StartAnimation(nameof(thisContentVisual.Opacity), appearAnimation);            
        }

        private void ReverseFlipBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {             
            _firstButton.Content = CreateTargetEllipse();
            _secondButton.Content = CreateTargetEllipse();
            MakeButtonContentOpaque(_firstButton);
            MakeButtonContentOpaque(_secondButton);
            _firstButton = null;
            _secondButton = null;
            _reverseAnimationActive = false;
        }

        private async void FlipCardFaceUp(Button btn)
        {
            //Flip button visual
            var btnVisual = ElementCompositionPreview.GetElementVisual(btn);
            var compositor = btnVisual.Compositor;

            //Get a visual for the content
            var btnContent = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(btn, 0), 0);
            var btnContentVisual = ElementCompositionPreview.GetElementVisual(btnContent as FrameworkElement);

            var easing = compositor.CreateLinearEasingFunction();

            if (_flipBatchAnimation != null)
            {
                _flipBatchAnimation.Completed -= FlipBatchAnimation_Completed;
                _flipBatchAnimation.Dispose();
            }

            _flipBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _flipBatchAnimation.Completed += FlipBatchAnimation_Completed;

            ScalarKeyFrameAnimation flipAnimation = compositor.CreateScalarKeyFrameAnimation();
            flipAnimation.InsertKeyFrame(0.000001f, 180);
            flipAnimation.InsertKeyFrame(1f, 0, easing);
            flipAnimation.Duration = TimeSpan.FromMilliseconds(800);
            flipAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            flipAnimation.IterationCount = 1;
            btnVisual.CenterPoint = new Vector3((float)(0.5 * btn.ActualWidth), (float)(0.5f * btn.ActualHeight), 0f);
            btnVisual.RotationAxis = new Vector3(0.0f, 1f, 0f);

            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(0.0f, 0);
            appearAnimation.InsertKeyFrame(0.599999f, 0);
            appearAnimation.InsertKeyFrame(0.6f, 0.5f);
            appearAnimation.InsertKeyFrame(1f, 1);
            appearAnimation.Duration = TimeSpan.FromMilliseconds(800);
            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;

            btnVisual.StartAnimation(nameof(btnVisual.RotationAngleInDegrees), flipAnimation);
            btnContentVisual.StartAnimation(nameof(btnContentVisual.Opacity), appearAnimation);
            _flipBatchAnimation.End();

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
        }

        private void FlipCardFaceDown(Button card)
        {
            if (card.Content == null  || card.Content is Ellipse) return;            

            //Flip button visual
            var btn1Visual = ElementCompositionPreview.GetElementVisual(card);            
            var compositor = btn1Visual.Compositor;

            //Get a visual for the content
            var btn1Content = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(card, 0), 0);
            var btn1ContentVisual = ElementCompositionPreview.GetElementVisual(btn1Content as FrameworkElement);

            var easing = compositor.CreateLinearEasingFunction();

            TimeSpan rndDelay = TimeSpan.FromMilliseconds(_rnd.NextDouble() * 200);

            ScalarKeyFrameAnimation flipAnimation = compositor.CreateScalarKeyFrameAnimation();
            flipAnimation.InsertKeyFrame(0.000001f, 0);
            flipAnimation.InsertKeyFrame(0.999999f, 180, easing);
            flipAnimation.InsertKeyFrame(1f, 0);
            flipAnimation.Duration = TimeSpan.FromMilliseconds(400);
            flipAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            flipAnimation.IterationCount = 1;            
            btn1Visual.CenterPoint = new Vector3((float)(0.5 * card.ActualWidth), (float)(0.5f * card.ActualHeight), 0.0f);
            btn1Visual.RotationAxis = new Vector3(0.0f, 1f, 0f);
            flipAnimation.DelayTime = rndDelay;

            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(0.0f, 1);
            appearAnimation.InsertKeyFrame(0.399999f, 1);
            appearAnimation.InsertKeyFrame(0.4f, 0);
            appearAnimation.InsertKeyFrame(1f, 0);
            appearAnimation.Duration = TimeSpan.FromMilliseconds(400);
            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;
            appearAnimation.DelayTime = rndDelay;

            btn1Visual.StartAnimation(nameof(btn1Visual.RotationAngleInDegrees), flipAnimation);            
            btn1ContentVisual.StartAnimation(nameof(btn1ContentVisual.Opacity), appearAnimation);                        
        }

        List<Button> ShuffleList(List<Button> list)
        {
            var len = list.Count;
            var repeatCount = _rnd.Next(1, 5);
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                for (var i = 0; i < len; i++)
                {
                    var j = _rnd.Next(0, len);
                    var k = list[i];
                    list[i] = list[j];
                    list[j] = k;
                }
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
            foreach (UIElement button in buttonMatrix.Children)
            {
                if (button is Button)
                {
                    list.Add(button as Button);
                }                
            }
            return list;            
        }
      
        async void ResetBoard()
        {
            PlayAgainText.Visibility = Visibility.Collapsed;
            _gameOver = false;
            _firstButton = null;
            _secondButton = null;
            _numMoves = 0;
            MoveCountTextBlock.Text = _numMoves.ToString();            
            _remaining = _boardRows * _boardColumns;
            var pairs = (_boardRows * _boardColumns) / 2;

            List<string> listContent;
            if (_usePictures)
            {
                try
                {                
                    listContent = await GetPicturesContent(pairs);
                }
                catch
                {
                    listContent = GetSymbolContent(pairs);
                    _usePictures = false;
                }
            }
            else
            {
                listContent = GetSymbolContent(pairs);
            }

            List<Button> listButtons = ShuffleList(GetButtonList());

            var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            if (_resetBatchAnimation != null)
            {
                _resetBatchAnimation.Completed -= ResetBatchAnimation_Completed;
                _resetBatchAnimation.Dispose();
            }

            _resetBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _resetBatchAnimation.Completed += ResetBatchAnimation_Completed; ;

            foreach (Button button in listButtons)
            {
                FlipCardFaceDown(button);
                GazeInput.SetInteraction(button, Interaction.Inherited);                
            }
            _resetBatchAnimation.End();

            for (int i = 0; i < _boardRows * _boardColumns; i += 2)
            {
              
                listButtons[i].Tag = listContent[i / 2];
                listButtons[i + 1].Tag = listContent[i / 2];
            }
        }

        private void ResetBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            List<Button> listButtons = GetButtonList();

            for (int i = 0; i < _boardRows * _boardColumns; i += 2)
            {                              
                listButtons[i].Content = CreateTargetEllipse();
                listButtons[i + 1].Content = CreateTargetEllipse();
                MakeButtonContentOpaque(listButtons[i]);
                MakeButtonContentOpaque(listButtons[i + 1]);
            }
            ApplyPerspective();
            AdjustFontSizes();
        }

        private void ApplyPerspective()
        {            
            //Apply the perspective to the environment

            var pageVisual = ElementCompositionPreview.GetElementVisual(this);            
            Vector2 pageSize = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);

            Matrix4x4 perspective = new Matrix4x4(

                                    1.0f, 0f, 0f, 0,
                                      0f, 1f, 0f, 0,
                                      0f, 0f, 1f, 1/pageSize.X,
                                      0f, 0f, 0f, 1f);

            pageVisual.TransformMatrix =
                            Matrix4x4.CreateTranslation(-pageSize.X / 2, -pageSize.Y / 2, 0f) *  // Translate to origin
                            perspective *                                                        // Apply perspective at origin
                            Matrix4x4.CreateTranslation(pageSize.X / 2, pageSize.Y / 2, 0f);     // Translate back to original position
        }

        private void ClearBoard()
        {
            List<Button> listButtons = GetButtonList();
            foreach (Button button in listButtons)
            {                
                FlipCardFaceDown(button);
            }
        }

        private void RevealBoard()  //For debuging purposes
        {
            List<Button> listButtons = GetButtonList();
            foreach (Button button in listButtons)
            {
                FlipCardFaceUp(button);
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (_animationActive || _reverseAnimationActive || _interactionPaused) return;

            if (_gameOver)
            {                
                return;
            }

            var btn = sender as Button;
            if (!(btn.Content is Ellipse) && btn.Content != null)
            {             
                return;
            }          

            if (_flashTimer.IsEnabled)
            {
                return;
                //OnFlashTimerTick(null, null);  //Unclear about the original idea of this
            }

            _animationActive = true;
            _numMoves++;
            MoveCountTextBlock.Text = _numMoves.ToString();

            if (_firstButton == null)
            {
                _firstButton = btn;
            }
            else
            {
                _secondButton = btn;
            }

            FlipCardFaceUp(btn);

        }

        private void FlipBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            _animationActive = false;

            if (_secondButton == null)
            {
                return;
            }

            if (_secondButton.Tag.ToString() != _firstButton.Tag.ToString())
            {
                GazeInput.SetInteraction(buttonMatrix, Interaction.Disabled);
                _flashTimer.Start();            
            }
            else            
            {
                //Do Match confirmation animation

                PulseButton(_firstButton);
                PulseButton(_secondButton);

           
                _firstButton = null;
                _secondButton = null;
                _remaining -= 2;

                CheckGameCompletion();
            }
        }

        void PulseButton(Button button)
        {
            var btnVisual = ElementCompositionPreview.GetElementVisual(button);
            var compositor = btnVisual.Compositor;
            var springSpeed = 50;

            btnVisual.CenterPoint = new System.Numerics.Vector3((float)button.ActualWidth / 2, (float)button.ActualHeight / 2, 0f);

            var scaleAnimation = compositor.CreateSpringVector3Animation();
            scaleAnimation.InitialValue = new System.Numerics.Vector3(0.9f, 0.9f, 0f);
            scaleAnimation.FinalValue = new System.Numerics.Vector3(1.0f, 1.0f, 0f);
            scaleAnimation.DampingRatio = 0.4f;
            scaleAnimation.Period = TimeSpan.FromMilliseconds(springSpeed);

            btnVisual.StartAnimation(nameof(btnVisual.Scale), scaleAnimation);
        }

        async void  CheckGameCompletion()
        {
            if (_remaining > 0)
            {
                return;
            }
            _gameOver = true;

            //Pulse entire board
            List<Button> listButtons = GetButtonList();
            foreach (Button button in listButtons)
            {
                PulseButton(button);
            }

            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log($"AllCards{_boardRows * _boardColumns}-ETD:{GazeInput.IsDeviceAvailable}-m#{_numMoves}");

            await Task.Delay(1000);

            string message = $"You matched the {_boardRows * _boardColumns} cards in {_numMoves} moves!";
            DialogText.Text = message;
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;
            DialogGrid.Visibility = Visibility.Visible;
            SetTabsForDialogView();
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;
            ResetBoard();
            SetTabsForPageView();
        }

        private async void DialogButton2_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;
            ClearBoard();
            await Task.Delay(400);
            Frame.Navigate(typeof(MainPage));
        }


        private void DismissButton(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;

            //RootGrid.Children.Remove(PlayAgainButton);
            //Grid.SetColumn(PlayAgainButton, Grid.GetColumn(_buttons[_boardSize - 1, _boardSize - 1]));
            //Grid.SetRow(PlayAgainButton, Grid.GetRow(_buttons[_boardSize - 1, _boardSize - 1]));
            //PlayAgainButton.MaxWidth = _buttons[_boardSize - 1, _boardSize - 1].ActualWidth;
            //PlayAgainButton.MaxHeight = _buttons[_boardSize - 1, _boardSize - 1].ActualHeight;
            //GameGrid.Children.Add(PlayAgainButton);
            //PlayAgainButton.Visibility = Visibility.Visible;

            PlayAgainText.Visibility = Visibility.Visible;
            OnPause(PauseButton, null);
            SetTabsForPageView();
            PauseButton.Focus(FocusState.Programmatic);
        }


        private void OnPause(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (_interactionPaused)
            {
                PauseButtonText.Text = "\uE769";
                PauseButtonBorder.Background = _toolButtonBrush;
                GazeInput.SetInteraction(buttonMatrix, Interaction.Enabled);
                _interactionPaused = false;
                if (_gameOver)
                {
                    ResetBoard();
                }
            }
            else
            {
                PauseButtonText.Text = "\uE768";
                PauseButtonBorder.Background = _pausedButtonBrush;
                GazeInput.SetInteraction(buttonMatrix, Interaction.Disabled);

                if (e != null)
                {
                    //User initiated pause, not code
                    StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                    logger.Log($"Paused-ETD:{GazeInput.IsDeviceAvailable}");
                }
                _interactionPaused = true;
            }
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void OnBack(object sender, RoutedEventArgs e)
        {
            ClearBoard();
            await Task.Delay(400);
            Frame.Navigate(typeof(MainPage));
        }
        
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustFontSizes();
            RepositionButtons();
        }

        private void DialogGrid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                DismissButton(null, null);
            }
        }

        private void SetTabsForDialogView()
        {
            BackButton.IsTabStop = false;
            PauseButton.IsTabStop = false;            
            ExitButton.IsTabStop = false;

            for (int row = 0; row < _boardRows; row++)
            {
                for (int col = 0; col < _boardColumns; col++)
                {
                    string buttonName = $"button_{row}_{col}";
                    Button button = FindName(buttonName) as Button;
                    button.IsTabStop = false;
                }
            }
        }

        private void SetTabsForPageView()
        {
            BackButton.IsTabStop = true;
            PauseButton.IsTabStop = true;            
            ExitButton.IsTabStop = true;
            for (int row = 0; row < _boardRows; row++)
            {
                for (int col = 0; col < _boardColumns; col++)
                {
                    string buttonName = $"button_{row}_{col}";
                    Button button = FindName(buttonName) as Button;
                    button.IsTabStop = true;
                }
            }
        }
    }
}

