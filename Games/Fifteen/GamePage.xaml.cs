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

namespace Fifteen
{
    public sealed partial class GamePage : Page
    {
        int _boardSize = 4;

        Button[,] _buttons;
        int _blankRow;
        int _blankCol;
        int _numMoves;

        SolidColorBrush _solidTileBrush = new SolidColorBrush(Colors.Silver);
        SolidColorBrush _blankTileBrush = new SolidColorBrush(Colors.White);

        public GamePage()
        {
            InitializeComponent();

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
                var gazePointer = GazeInput.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);
            });

            Loaded += GamePage_Loaded;
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeButtonArray();
            while(IsSolved())
            {
                ResetBoard();
            }
            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);
            Button blankBtn = _buttons[_blankRow, _blankCol];
            blankBtn.Background = _blankTileBrush;
            blankBtn.Visibility = Visibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) 
        {
            base.OnNavigatedTo(e);
            _boardSize = (int)e.Parameter;
        }

        void InitializeButtonArray()
        {
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
                    button.Name = "button" + "_" + col + "_" + row;
                    button.Content = ((row * _boardSize) + col + 1).ToString();
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
        }

        bool SwapBlank(int row, int col)
        {
            //Prevent tile slides once puzzle is solved
            if (DialogGrid.Visibility == Visibility.Visible)
            {
                return false;
            }

            if (!((((row == _blankRow - 1) || (row == _blankRow + 1)) && (col == _blankCol)) ||
                 (((col == _blankCol - 1) || (col == _blankCol + 1)) && (row == _blankRow))))
            {
                return false;
            }

            //Slide button visual
            Button btn = _buttons[row, col];
            Button blankBtn = _buttons[_blankRow, _blankCol];

            //Get Visuals for the selected button that is going to appear to slide and for the blank button
            var btnVisual = ElementCompositionPreview.GetElementVisual(btn);            
            var compositor = btnVisual.Compositor;
            var blankBtnVisual = ElementCompositionPreview.GetElementVisual(blankBtn);

            var easing = compositor.CreateLinearEasingFunction();

            //Create an animation to first move the blank button with its updated contents to
            //instantly appear in the position position of the selected button
            //then slide that button back into its original position
            var slideAnimation = compositor.CreateVector3KeyFrameAnimation();
            slideAnimation.InsertKeyFrame(0f, btnVisual.Offset);
            slideAnimation.InsertKeyFrame(1f, blankBtnVisual.Offset, easing);
            slideAnimation.Duration = TimeSpan.FromMilliseconds(500);

            //Apply the slide anitmation to the blank button
            blankBtnVisual.StartAnimation(nameof(btnVisual.Offset), slideAnimation);

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
            GazeInput.SetInteraction(blankBtn, Interaction.Enabled);
            GazeInput.SetInteraction(btn, Interaction.Disabled);

            return true;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int cellNumber = int.Parse(button.Tag.ToString());
            int row = cellNumber / _boardSize;
            int col = cellNumber % _boardSize;

            if (SwapBlank(row, col))
            {
                _numMoves++;
                CheckCompletion();
            }
        }

        bool IsSolved()
        {
            for (int i = 0; i < _boardSize * _boardSize - 1; i++)
            {
                int row = i / _boardSize;
                int col = i % _boardSize;
                if (_buttons[row, col].Content.ToString() != (i + 1).ToString())
                {
                    return false;
                }
            }
            return true;
        }

        void CheckCompletion()
        {
            if (!IsSolved())
            {
                return;
            }

            string message = $"Congratulations!! You solved it in {_numMoves} moves";
            DialogText.Text = message;
            DialogGrid.Visibility = Visibility.Visible;
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            //ResetBoard();
            DialogGrid.Visibility = Visibility.Collapsed;

            Frame.Navigate(typeof(MainPage));
        }
    }
}
