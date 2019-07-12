//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace Fifteen
{
    public sealed partial class GamePage : Page
    {
        int _boardSize = 4; 
        
        Button[,] _buttons;
        int _blankRow;
        int _blankCol;
        int _numMoves;
        bool _interactionPaused = false;
        
        SolidColorBrush _solidTileBrush;
        SolidColorBrush _blankTileBrush = new SolidColorBrush(Colors.Transparent);
        SolidColorBrush _toolButtonBrush;
        SolidColorBrush _pausedButtonBrush = new SolidColorBrush(Colors.Black);

        CompositionScopedBatch _slideBatchAnimation;

        DispatcherTimer WaitForCompositionTimer;

        bool _gameOver = false;

        bool _animationActive = false;

        public GamePage()
        {
            InitializeComponent();

            _solidTileBrush = (SolidColorBrush)this.Resources["TileBackground"];
            _toolButtonBrush = (SolidColorBrush)this.Resources["ToolBarButtonBackground"]; 

            WaitForCompositionTimer = new DispatcherTimer();
            WaitForCompositionTimer.Tick += WaitForCompositionTimer_Tick;
            WaitForCompositionTimer.Interval = TimeSpan.FromMilliseconds(50);

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                var gazePointer = GazeInput.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);
            });

            Loaded += GamePage_Loaded;
        }

        private void WaitForCompositionTimer_Tick(object sender, object e)
        {
            if (IsButtonCompositionReady())
            {
                Button blankBtn;
                while (IsSolved())
                {
                    ResetBoard();
                }
                blankBtn = _buttons[_blankRow, _blankCol];
                blankBtn.Background = _blankTileBrush;
                blankBtn.Visibility = Visibility.Visible;
                MoveCountTextBlock.Text = "0";
                WaitForCompositionTimer.Stop();
            }
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeButtonArray();
            WaitForCompositionTimer.Start();
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);                
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _boardSize = (int)e.Parameter;
        }

        void InitializeButtonArray()
        {
            GazeInput.SetInteraction(GameGrid, Interaction.Disabled);

            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            for (int row = 0; row < _boardSize; row++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            _buttons = new Button[_boardSize, _boardSize];

            for (int row = 0; row < _boardSize; row++)
            {
                for (int col = 0; col < _boardSize; col++)
                {
                    var button = new Button();
                    button.Background = _solidTileBrush;                    
                    button.Name = "button" + "_" + col + "_" + row;
                    if (!(row == _boardSize - 1 && col == _boardSize - 1))
                    {
                        button.Content = ((row * _boardSize) + col + 1).ToString();
                    }
                    else
                    {
                        button.Content = "";
                        button.Background = _blankTileBrush;
                    }
                    button.Tag = (row * _boardSize) + col;
                    button.Click += OnButtonClick;
                    button.Style = Resources["ButtonStyle"] as Style;

                    _buttons[row, col] = button; ;

                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);
                    GameGrid.Children.Add(button);
                }
            }

            GameGrid.UpdateLayout();
        }

        void ResetBoard()
        {
            PlayAgainText.Visibility = Visibility.Collapsed;
            _gameOver = false;
            _numMoves = 0;
            MoveCountTextBlock.Text = _numMoves.ToString();
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    _buttons[i, j].Content = ((i * _boardSize) + j + 1).ToString();
                }
            }

            _buttons[_boardSize - 1, _boardSize - 1].Content = "";
            _blankRow = _boardSize - 1;
            _blankCol = _boardSize - 1;

            int shuffleCount = 500;
            Random rnd = new Random();
            while (shuffleCount > 0)
            {
                bool changeRow = rnd.Next(0, 2) == 0;
                bool decrement = rnd.Next(0, 2) == 0;

                int row = _blankRow;
                int col = _blankCol;
                if (changeRow)
                {
                    row = decrement ? row - 1 : row + 1;
                }
                else
                {
                    col = decrement ? col - 1 : col + 1;
                }

                if ((row < 0) || (row >= _boardSize) || (col < 0) || (col >= _boardSize))
                {
                    continue;
                }

                if (SwapBlank(row, col))
                {
                    shuffleCount--;
                }                
            }
            GazeInput.SetInteraction(GameGrid, Interaction.Enabled);            
        }

        private bool IsButtonCompositionReady()
        {
            for (int row = 0; row < _boardSize; row++)
            {
                for (int col = 0; col < _boardSize; col++)
                {
                    var btnVisual = ElementCompositionPreview.GetElementVisual(_buttons[row, col]);
                    if (btnVisual.Offset.X == 0 && btnVisual.Offset.Y == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        bool SwapBlank(int row, int col)
        {
            //Prevent tile slides once puzzle is solved
            if (DialogGrid.Visibility == Visibility.Visible || _gameOver)
            {
                return false;
            }

            if (!((((row == _blankRow - 1) || (row == _blankRow + 1)) && (col == _blankCol)) ||
                 (((col == _blankCol - 1) || (col == _blankCol + 1)) && (row == _blankRow))))
            {
                return false;
            }
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.Transparent);

            _animationActive = true;
            GazeInput.SetInteraction(GameGrid, Interaction.Disabled);
            //Slide button visual
            Button btn = _buttons[row, col];
            Button blankBtn = _buttons[_blankRow, _blankCol];

            //Get Visuals for the selected button that is going to appear to slide and for the blank button
            var btnVisual = ElementCompositionPreview.GetElementVisual(btn);
            var compositor = btnVisual.Compositor;
            var blankBtnVisual = ElementCompositionPreview.GetElementVisual(blankBtn);

            var easing = compositor.CreateLinearEasingFunction();

            if (_slideBatchAnimation != null)
            {
                _slideBatchAnimation.Completed -= SlideBatchAnimation_Completed;
                _slideBatchAnimation.Dispose();
            }

            _slideBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _slideBatchAnimation.Completed += SlideBatchAnimation_Completed;

            //Create an animation to first move the blank button with its updated contents to
            //instantly appear in the position position of the selected button
            //then slide that button back into its original position
            var slideAnimation = compositor.CreateVector3KeyFrameAnimation();
            slideAnimation.InsertKeyFrame(0f, btnVisual.Offset);
            slideAnimation.InsertKeyFrame(1f, blankBtnVisual.Offset, easing);
            slideAnimation.Duration = TimeSpan.FromMilliseconds(500);
            
            //Apply the slide animation to the blank button            
            blankBtnVisual.StartAnimation(nameof(btnVisual.Offset), slideAnimation);
            
            //Pulse after slide if sliding to correct position
            if (((_blankRow * _boardSize) + _blankCol + 1).ToString() == btn.Content.ToString())
            {               
                var springSpeed = 50;

                blankBtnVisual.CenterPoint = new System.Numerics.Vector3((float)blankBtn.ActualWidth / 2, (float)blankBtn.ActualHeight / 2, 0f);

                var scaleAnimation = compositor.CreateSpringVector3Animation();
                scaleAnimation.InitialValue = new System.Numerics.Vector3(0.9f, 0.9f, 0f);
                scaleAnimation.FinalValue = new System.Numerics.Vector3(1.0f, 1.0f, 0f);
                scaleAnimation.DampingRatio = 0.4f;
                scaleAnimation.Period = TimeSpan.FromMilliseconds(springSpeed);
                scaleAnimation.DelayTime = TimeSpan.FromMilliseconds(500);

                blankBtnVisual.StartAnimation(nameof(blankBtnVisual.Scale), scaleAnimation);
            }

            _slideBatchAnimation.End();

            //Swap content of the selected button with the blank button and clear the selected button
            _buttons[_blankRow, _blankCol].Content = _buttons[row, col].Content;
            _buttons[row, col].Content = "";
            _blankRow = row;
            _blankCol = col;


            //Note there is some redunancy in the following settings that corrects the UI at board load as well as tile slide 
            //Force selected button to the bottom and the blank button to the top
            Canvas.SetZIndex(btn, -_boardSize);
            Canvas.SetZIndex(blankBtn, 0);

            //Update the background colors of the two buttons to reflect their new condition
            btn.Background = _blankTileBrush;
            blankBtn.Background = _solidTileBrush;

            //Update the visibility to collapse the selected button that is now blank
            btn.Visibility = Visibility.Collapsed;
            blankBtn.Visibility = Visibility.Visible;

            //Disable eye control for the new empty button so that there are no inappropriate dwell indicators
            GazeInput.SetInteraction(blankBtn, Interaction.Inherited);          
            GazeInput.SetInteraction(btn, Interaction.Disabled);

            return true;
        }


        private void SlideBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {           
            CheckCompletionAsync();            
            if (!_gameOver)
            {
                _animationActive = false;
                GazeInput.SetInteraction(GameGrid, Interaction.Enabled);
                GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            }            
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (_animationActive || _interactionPaused) return;

            var button = sender as Button;
            int cellNumber = int.Parse(button.Tag.ToString());
            int row = cellNumber / _boardSize;
            int col = cellNumber % _boardSize;

            if (SwapBlank(row, col))
            {
                _numMoves++;
                MoveCountTextBlock.Text = _numMoves.ToString();
            }
        }

        bool IsSolved()
        {
            int row=0;
            int col=0;
            for (int i = 0; i < _boardSize * _boardSize - 1; i++)
            {
                row = i / _boardSize;
                col = i % _boardSize;
                if (_buttons[row, col].Content.ToString() != (i + 1).ToString())
                {
                    return false;
                }
            }
            for (int i = 0; i < _boardSize * _boardSize - 1; i++)
            {
                row = i / _boardSize;
                col = i % _boardSize;
                PulseButton(_buttons[row, col]);
            }            
            return true;
        }

        void PulseButton(Button lastButton)
        {                              
            var btn1Visual = ElementCompositionPreview.GetElementVisual(lastButton);
            var compositor = btn1Visual.Compositor;
            var springSpeed = 50;

            btn1Visual.CenterPoint = new System.Numerics.Vector3((float)lastButton.ActualWidth / 2, (float)lastButton.ActualHeight / 2, 0f);

            var scaleAnimation = compositor.CreateSpringVector3Animation();
            scaleAnimation.InitialValue = new System.Numerics.Vector3(0.9f, 0.9f, 0f);
            scaleAnimation.FinalValue = new System.Numerics.Vector3(1.0f, 1.0f, 0f);
            scaleAnimation.DampingRatio = 0.4f;
            scaleAnimation.Period = TimeSpan.FromMilliseconds(springSpeed);

            btn1Visual.StartAnimation(nameof(btn1Visual.Scale), scaleAnimation);
        }

        async void CheckCompletionAsync()
        {
            if (!IsSolved())
            {
                return;
            }
            _gameOver = true;

            await Task.Delay(1000);

            string message = $"You solved the puzzle in {_numMoves} moves!";
            DialogText.Text = message;
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;
            DialogGrid.Visibility = Visibility.Visible;
            SetTabsForDialogView();
            CloseDialogButton.Focus(FocusState.Programmatic);
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;
            while (IsSolved())
            {
                ResetBoard();
            }
            SetTabsForPageView();
        }

        private void DialogButton2_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;
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

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void OnBack(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void OnPause(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (_interactionPaused)
            {
                PauseButtonText.Text = "\uE769";
                PauseButtonBorder.Background = _toolButtonBrush;
                GazeInput.SetInteraction(GameGrid, Interaction.Enabled);
                _interactionPaused = false;
                if (_gameOver)
                {
                    while (IsSolved())
                    {
                        ResetBoard();
                    }
                }
            }
            else
            {
                PauseButtonText.Text = "\uE768";
                PauseButtonBorder.Background = _pausedButtonBrush;
                GazeInput.SetInteraction(GameGrid, Interaction.Disabled);
                _interactionPaused = true;                
            }
        }

        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            GameGrid.Children.Remove(PlayAgainButton);
            RootGrid.Children.Add(PlayAgainButton);
            PlayAgainButton.Visibility = Visibility.Collapsed;
            while (IsSolved())
            {
                ResetBoard();
            }
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

            for (int row = 0; row < _boardSize; row++)
            {
                for (int col = 0; col < _boardSize; col++)
                {
                    _buttons[row, col].IsTabStop = false;
                }
            }
        }

        private void SetTabsForPageView()
        {
            BackButton.IsTabStop = true;
            PauseButton.IsTabStop = true;
            ExitButton.IsTabStop = true;
            for (int row = 0; row < _boardSize; row++)
            {
                for (int col = 0; col < _boardSize; col++)
                {
                    _buttons[row, col].IsTabStop = true;                     
                }
            }
        }
    }
}
