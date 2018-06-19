//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Windows.UI.Xaml.Navigation;

namespace Fifteen
{
    public sealed partial class GamePage : Page
    {
        int _boardSize = 4;

        Button[,] _buttons;
        int _blankRow;
        int _blankCol;
        int _numMoves;


        public GamePage()
        {
            this.InitializeComponent();

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
                var gazePointer = GazeInput.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);
            });

            this.Loaded += GamePage_Loaded;
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeButtonArray();
            while(IsSolved())
            {
                ResetBoard();
            }
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
            if (!((((row == _blankRow - 1) || (row == _blankRow + 1)) && (col == _blankCol)) ||
                 (((col == _blankCol - 1) || (col == _blankCol + 1)) && (row == _blankRow))))
            {
                return false;
            }

            _buttons[_blankRow, _blankCol].Content = _buttons[row, col].Content;
            _buttons[row, col].Content = "";
            _blankRow = row;
            _blankCol = col;
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
