using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TwoZeroFourEight
{
    public class Cell
    {
        public string Value
        {
            get { return (IntVal != 0) ? IntVal.ToString() : ""; }
        }
        public int IntVal;
    }

    public sealed class IntegerToStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, string culture)
        {
            int val = (int)value;
            return val == 0 ? "" : val.ToString();
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public const int BOARD_SIZE = 4;
        public const int MAX_BOARD_CELLS = BOARD_SIZE * BOARD_SIZE;
        private ObservableCollection<int> _board;
        private Random _random = new Random();

        public int Score { get; set; }
        public Visibility GameOverStatus { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            _board = new ObservableCollection<int>();
            for (int i = 0; i < MAX_BOARD_CELLS; i++)
            {
                //var cell = new Cell { IntVal = 0 };
                _board.Add(0);
            }

            InitializeGame();
        }

        private void InitializeGame()
        {
            Score = 0;
            for (int i = 0; i < MAX_BOARD_CELLS; i++)
            {
                //var cell = new Cell { IntVal = 0 };
                _board[i] = 0;
            }
            GameOverStatus = Visibility.Collapsed;
            GenerateNextTile();
        }

        private bool IsBoardFull()
        {
            foreach (var cell in _board)
            {
                if (cell == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void GenerateNextTile()
        {
            int index;
            do
            {
                index = _random.Next(MAX_BOARD_CELLS);
            } while (_board[index] != 0);

            int val = (_random.Next(2) * 2) + 2;

            _board[index] = val;
        }

        private bool SlideRowOrCol(int start, int end, int delta)
        {
            bool slideSuccess = false;
            int cur = start;
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                int j = cur;
                while ((j != end) && (_board[j] == 0))
                {
                    j += delta;
                }
                if ((j != cur) && (_board[j] != 0))
                {
                    _board[cur] = _board[j];
                    _board[j] = 0;
                    slideSuccess = true;
                }
                cur += delta;
            }
            return slideSuccess;
        }

        private bool AddAdjacent(int start, int end, int delta)
        {
            bool addSuccess = false;
            int cur = start;
            for (int i = 0; i < BOARD_SIZE - 1; i++)
            {
                if ((_board[cur + delta] != 0) && (_board[cur] > 0) && (_board[cur] == _board[cur + delta]))
                {
                    _board[cur] += _board[cur + delta];
                    _board[cur + delta] = 0;
                    Score += _board[cur];
                    addSuccess = true;
                }
                cur += delta;
            }
            return addSuccess;
        }

        private void SlideBoard(int start, int end, int delta, int increment)
        {
            bool change = false;
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                change = SlideRowOrCol(start, end, delta) || change;
                change = AddAdjacent(start, end, delta) || change;
                change = SlideRowOrCol(start, end, delta) || change;
                start += increment;
                end += increment;
            }

            if (IsBoardFull())
            {
                GameOverStatus = Visibility.Visible;
                return;
            }

            if (change)
            {
                GenerateNextTile();
            }
        }

        private void OnNewGame(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        private void OnUpClick(object sender, RoutedEventArgs e)
        {
            SlideBoard(0, MAX_BOARD_CELLS - BOARD_SIZE, BOARD_SIZE, 1);
        }

        private void OnLeftClick(object sender, RoutedEventArgs e)
        {
            SlideBoard(0, BOARD_SIZE - 1, 1, BOARD_SIZE);
        }

        private void OnRightClick(object sender, RoutedEventArgs e)
        {
            SlideBoard(BOARD_SIZE - 1, 0, -1, BOARD_SIZE);
        }

        private void OnDownClick(object sender, RoutedEventArgs e)
        {
            SlideBoard(MAX_BOARD_CELLS - BOARD_SIZE, 0, -BOARD_SIZE, 1);
        }

        private void OnPageKeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Up:
                    OnUpClick(null, null);
                    break;
                case VirtualKey.Down:
                    OnDownClick(null, null);
                    break;
                case VirtualKey.Left:
                    OnLeftClick(null, null);
                    break;
                case VirtualKey.Right:
                    OnRightClick(null, null);
                    break;
            }
        }
    }
}
