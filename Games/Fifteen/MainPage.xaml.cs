//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Input.Gaze;
using Windows.Foundation.Collections;
using Windows.Foundation;

namespace Fifteen
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const int BOARD_SIZE = 4;

        Button[,] _buttons;
        int _blankRow;
        int _blankCol;
        int _numMoves;


        public MainPage()
        {
            this.InitializeComponent();

            InitializeButtonArray();
            ResetBoard();

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
                var gazePointer = GazeApi.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);
            });
        }

        void InitializeButtonArray()
        {
            _buttons = new Button[BOARD_SIZE, BOARD_SIZE];
            _buttons[0, 0] = button_0_0;
            _buttons[0, 1] = button_0_1;
            _buttons[0, 2] = button_0_2;
            _buttons[0, 3] = button_0_3;
            _buttons[1, 0] = button_1_0;
            _buttons[1, 1] = button_1_1;
            _buttons[1, 2] = button_1_2;
            _buttons[1, 3] = button_1_3;
            _buttons[2, 0] = button_2_0;
            _buttons[2, 1] = button_2_1;
            _buttons[2, 2] = button_2_2;
            _buttons[2, 3] = button_2_3;
            _buttons[3, 0] = button_3_0;
            _buttons[3, 1] = button_3_1;
            _buttons[3, 2] = button_3_2;
            _buttons[3, 3] = button_3_3;
        }

        void ResetBoard()
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    _buttons[i, j].Content = ((i * BOARD_SIZE) + j + 1).ToString();
                }
            }

            _buttons[BOARD_SIZE - 1, BOARD_SIZE - 1].Content = "";
            _blankRow = BOARD_SIZE - 1;
            _blankCol = BOARD_SIZE - 1;

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

                if ((row < 0) || (row >= BOARD_SIZE) || (col < 0) || (col >= BOARD_SIZE))
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
            int row = cellNumber / BOARD_SIZE;
            int col = cellNumber % BOARD_SIZE;

            if (SwapBlank(row, col))
            {
                _numMoves++;
                CheckCompletion();
            }
        }

        void CheckCompletion()
        {
            for (int i = 0; i < BOARD_SIZE * BOARD_SIZE - 1; i++)
            {
                int row = i / BOARD_SIZE;
                int col = i % BOARD_SIZE;
                if (_buttons[row, col].Content.ToString() != (i + 1).ToString())
                {
                    return;
                }
            }

            string message = $"Congratulations!! You solved it in {_numMoves} moves";
            DialogText.Text = message;
            DialogGrid.Visibility = Visibility.Visible;

            ResetBoard();
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBoard();
            DialogGrid.Visibility = Visibility.Collapsed;
        }
    }
}
