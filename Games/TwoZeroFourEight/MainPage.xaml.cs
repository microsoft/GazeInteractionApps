//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace TwoZeroFourEight
{
    public class NotificationBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public sealed class ValueToBackgroundColorConverter : IValueConverter
    {
        private static Brush[] _backgroundColors = new Brush[]
        {
            new SolidColorBrush(Colors.LightGray),

            new SolidColorBrush(Colors.PaleTurquoise),
            new SolidColorBrush(Colors.LightBlue),
            new SolidColorBrush(Colors.LightSkyBlue),
            new SolidColorBrush(Colors.DeepSkyBlue),

            new SolidColorBrush(Colors.LemonChiffon),
            new SolidColorBrush(Colors.Khaki),
            new SolidColorBrush(Colors.DarkKhaki),
            new SolidColorBrush(Colors.Goldenrod),

            new SolidColorBrush(Colors.PaleGreen),
            new SolidColorBrush(Colors.LightGreen),
            new SolidColorBrush(Colors.YellowGreen),
            new SolidColorBrush(Colors.LimeGreen),

            new SolidColorBrush(Colors.MistyRose),
            new SolidColorBrush(Colors.Pink),
            new SolidColorBrush(Colors.HotPink),
            new SolidColorBrush(Colors.DeepPink),
        };

        public object Convert(object value, Type targetType,
                              object parameter, string culture)
        {
            int index = 0;
            int val = (int)value;
            while (val > 0)
            {
                val >>= 1;
                index++;
            }
            return _backgroundColors[index];
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Cell: NotificationBase
    {
        private int _intVal;
        public int IntVal
        {
            get { return _intVal; }
            set
            {
                SetField<int>(ref _intVal, value, "IntVal");
            }
        }

        private Brush _backgroundColor;
        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set { SetField<Brush>(ref _backgroundColor, value, "BackgroundColor"); }
        }

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

    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, string culture)
        {
            bool val = (bool)value;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Board : NotificationBase
    {
        private int _boardSize;
        private int _maxCells;
        private Random _random = new Random();

        private bool _gameOver;
        public bool GameOver
        {
            get { return _gameOver; }
            set { SetField<bool>(ref _gameOver, value, "GameOver"); }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set { SetField<int>(ref _score, value, "Score"); }
        }

        public List<Cell> Cells { get; set; }

        public Board(int boardSize)
        {
            _boardSize = boardSize;
            _maxCells = _boardSize * _boardSize;

            Cells = new List<Cell>(_maxCells);
            for (int i = 0; i < _maxCells; i++)
            {
                Cells.Add(new Cell());
            }

            Reset();
        }

        public void Reset()
        {
            Score = 0;
            GameOver = false;
            for (int i = 0; i < _maxCells; i++)
            {
                Cells[i].IntVal = 0;
            }
            GenerateNextTile();
        }

        private bool IsBoardFull()
        {
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    var cur = Cells[i * _boardSize + j];
                    var above = (i > 0) ? Cells[((i - 1) * _boardSize) + j] : null;
                    var below = (i < _boardSize - 1) ? Cells[((i + 1) * _boardSize) + j] : null;
                    var left = (j > 0) ? Cells[(i * _boardSize) + j - 1] : null;
                    var right = (j < _boardSize - 1) ? Cells[(i * _boardSize) + j + 1] : null;

                    if ((above != null) && (cur.IntVal == above.IntVal))
                    {
                        return false;
                    }

                    if ((below != null) && (cur.IntVal == below.IntVal))
                    {
                        return false;
                    }

                    if ((left != null) && (cur.IntVal == left.IntVal))
                    {
                        return false;
                    }

                    if ((right != null) && (cur.IntVal == right.IntVal))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool GenerateNextTile()
        {
            List<Cell> blankCells = new List<Cell>();
            for (int i = 0; i < _maxCells; i++)
            {
                if (Cells[i].IntVal == 0)
                {
                    blankCells.Add(Cells[i]);
                }
            }

            if (blankCells.Count == 0)
            {
                return false;
            }

            int index = _random.Next(blankCells.Count);

            // generate a 2 mostly, but generate a 4 about one in 8 times
            int val = (_random.Next(8) > 0) ? 2 : 4;

            blankCells[index].IntVal = val;

            return true;
        }

        private bool SlideRowOrCol(int start, int end, int delta)
        {
            bool slideSuccess = false;
            int cur = start;
            for (int i = 0; i < _boardSize; i++)
            {
                int j = cur;
                while ((j != end) && (Cells[j].IntVal == 0))
                {
                    j += delta;
                }
                if ((j != cur) && (Cells[j].IntVal != 0))
                {
                    Cells[cur].IntVal = Cells[j].IntVal;
                    Cells[j].IntVal = 0;
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
            for (int i = 0; i < _boardSize - 1; i++)
            {
                if ((Cells[cur + delta].IntVal != 0) && (Cells[cur].IntVal > 0) && (Cells[cur].IntVal == Cells[cur + delta].IntVal))
                {
                    Cells[cur].IntVal += Cells[cur + delta].IntVal;
                    Cells[cur + delta].IntVal = 0;
                    Score += Cells[cur].IntVal;
                    addSuccess = true;
                }
                cur += delta;
            }
            return addSuccess;
        }

        private void SlideBoard(int start, int end, int delta, int increment)
        {
            if (GameOver)
            {
                return;
            }

            bool change = false;
            for (int i = 0; i < _boardSize; i++)
            {
                change = SlideRowOrCol(start, end, delta) || change;
                change = AddAdjacent(start, end, delta) || change;
                change = SlideRowOrCol(start, end, delta) || change;
                start += increment;
                end += increment;
            }

            if (change)
            {
                OnPropertyChanged("Score");
            }

            if ((!GenerateNextTile()) && IsBoardFull())
            {
                GameOver = true;
                OnPropertyChanged("GameOver");
            }

        }

        public void SlideLeft()
        {
            SlideBoard(0, _boardSize - 1, 1, _boardSize);
        }

        public void SlideRight()
        {
            SlideBoard(_boardSize - 1, 0, -1, _boardSize);
        }

        public void SlideUp()
        {
            SlideBoard(0, _maxCells - _boardSize, _boardSize, 1);
        }

        public void SlideDown()
        {
            SlideBoard(_maxCells - _boardSize, 0, -_boardSize, 1);
        }
    }

    public sealed partial class MainPage : Page
    {
        public Board Board;

        public MainPage()
        {
            this.InitializeComponent();
            Board = new Board(4);
            DataContext = Board;

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                var gazePointer = GazeInput.GetGazePointer(this);
                gazePointer.LoadSettings(sharedSettings);
            });
        }

        private void OnNewGame(object sender, RoutedEventArgs e)
        {
            Board.Reset();
        }

        private void OnUpClick(object sender, RoutedEventArgs e)
        {
            Board.SlideUp();
        }

        private void OnLeftClick(object sender, RoutedEventArgs e)
        {
            Board.SlideLeft();
        }

        private void OnRightClick(object sender, RoutedEventArgs e)
        {
            Board.SlideRight();
        }

        private void OnDownClick(object sender, RoutedEventArgs e)
        {
            Board.SlideDown();
        }

        private void OnPageKeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Up:
                    Board.SlideUp();
                    break;
                case VirtualKey.Down:
                    Board.SlideDown();
                    break;
                case VirtualKey.Left:
                    Board.SlideLeft();
                    break;
                case VirtualKey.Right:
                    Board.SlideRight();
                    break;
            }
        }
    }
}
