using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MazeCreator.Core;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Maze
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const int MIN_ROWS = 3;
        int _numRows;
        int _numCols;

        Image _mazeRunner;
        Image _mazeEnd;

        bool _isMazeSolved;
        MazeCreator.Core.ICreator _mazeCreator;
        MazeCreator.Core.Maze _maze;

        DispatcherTimer _solutionTimer;
        Direction[] _solution;

        int _curRow;
        int _curCol;
        Button _curButton;
        Button _targetButton;
        int _solutionCurIndex;

        public MainPage()
        {
            this.InitializeComponent();
            _numRows = MIN_ROWS;

            _mazeRunner = new Image
            {
                Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/luna.png")),
                VerticalAlignment = VerticalAlignment.Center
            };

            _mazeEnd = new Image
            {
                Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/doghouse.png")),
                VerticalAlignment = VerticalAlignment.Center
            };

            _mazeCreator = Creator.GetCreator();

            _solutionTimer = new DispatcherTimer();
            _solutionTimer.Interval = TimeSpan.FromSeconds(0.1);
            _solutionTimer.Tick += OnSolutionTimerTick;
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            BuildMaze();
        }

        private void OnNewMaze(object sender, RoutedEventArgs e)
        {
            BuildMaze();
        }

        private void OnBiggerMaze(object sender, RoutedEventArgs e)
        {
            _numRows++;
            _numCols++;
            BuildMaze();
        }

        private void OnSmallerMaze(object sender, RoutedEventArgs e)
        {
            _numCols = (_numCols > 3) ? _numCols - 1 : _numCols;
            _numRows = (_numRows > 3) ? _numRows - 1 : _numRows;
            BuildMaze();
        }

        private void OnSolutionTimerTick(object sender, object e)
        {
            var brush = (SolidColorBrush)this.Resources["SolveBrush"];
            var cell = MazeGrid.Children.ElementAt(_curRow * _numCols + _curCol) as Border;
            cell.Background = brush;

            var currentContent = _curButton.Content;

            if (_solutionCurIndex >= _solution.Count())
            {
                _solutionTimer.Stop();
                _isMazeSolved = true;

                Image _mazeComplete = new Image
                {
                    Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/dogInHouse.png")),
                    VerticalAlignment = VerticalAlignment.Center
                };                              
                _curButton.Content = _mazeComplete;

                return;
            }            

           
            _curButton.Content = null;          

            var pos = _solution.ElementAt(_solutionCurIndex);
            if (pos == Direction.Left)
            {
                _curCol--;
            }
            if (pos == Direction.Right)
            {
                _curCol++;
            }
            if (pos == Direction.Up)
            {
                _curRow--;
            }
            if (pos == Direction.Down)
            {
                _curRow++;
            }
            _solutionCurIndex++;

            _curButton = FindName($"button_{_curRow}_{_curCol}") as Button;
            _curButton.Content = currentContent;
        }

        private void OnSolveMaze(object sender, RoutedEventArgs e)
        {
            var solver = MazeCreator.Solver.Create();
            _solution = solver.Solve(_maze, new Position(_curRow, _curCol), new Position(_numRows - 1, _numCols - 1));

            _solutionCurIndex = 0;
            _solutionTimer.Start();
        }

        private void BuildMaze()
        {
            _solutionTimer.Stop();

            int remainingHeight = (int)(this.ActualHeight - Toolbar.ActualHeight);
            int cellSize = remainingHeight / _numRows;
            _numCols = (int)(this.ActualWidth / cellSize);

            _maze = _mazeCreator.Create(_numRows, _numCols);
            _isMazeSolved = false;

            _curRow = _curCol = 0;

            if (_curButton != null)
            {
                _curButton.Content = null;
                _curButton = null;
            }
            if (_targetButton != null)
            {
                _targetButton.Content = null;
                _targetButton = null;
            }
            MazeGrid.Children.Clear();
            MazeGrid.RowDefinitions.Clear();
            MazeGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < _numRows; i++)
            {
                MazeGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < _numCols; i++)
            {
                MazeGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            int borderThickness = 2;
            var brush = (SolidColorBrush)this.Resources["BorderBrush"];

            for (int i = 0; i < _numRows; i++)
            {
                for (int j = 0; j < _numCols; j++)
                {
                    var button = new Button();
                    button.Name = $"button_{i}_{j}";
                    button.Tag = i << 16 | j;
                    button.Click += OnMazeCellClick;
                    button.Width = cellSize;
                    button.Height = cellSize;
                    button.Style = (Style) this.Resources["MazeCellStyle"];

                    var border = new Border();
                    border.BorderBrush = brush;
                    border.Child = button;

                    var thickness = new Thickness();
                    var cell = _maze[i, j];
                    thickness.Left = (cell.HasLeftWall) ? borderThickness : 0;
                    thickness.Top = (cell.HasTopWall) ? borderThickness : 0;
                    thickness.Right = (cell.HasRightWall) ? borderThickness : 0;
                    thickness.Bottom = (cell.HasBottomWall) ? borderThickness : 0;
                    border.BorderThickness = thickness;

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    MazeGrid.Children.Add(border);
                }
            }

            _curButton = FindName("button_0_0") as Button;
            _curButton.Content = _mazeRunner;
            _targetButton = FindName($"button_{_numRows - 1}_{_numCols - 1}") as Button;
            _targetButton.Content = _mazeEnd;
        }

        private void TryMoveRunner(int row, int col)
        {
            if (row == _curRow)
            {
                if (_curCol < col)
                {
                    while ((!_maze[_curRow, _curCol].HasRightWall) && (_curCol < col))
                    {
                        _curCol++;
                    }
                    return;
                }
                else if (_curCol > col)
                {
                    while ((!_maze[_curRow, _curCol].HasLeftWall) && (_curCol > col))
                    {
                        _curCol--;
                    }
                }
            }
            else if (col == _curCol)
            {
                if (_curRow < row)
                {
                    while ((!_maze[_curRow, _curCol].HasBottomWall) && (_curRow < row))
                    {
                        _curRow++;
                    }
                    return;
                }
                else if (_curRow > row)
                {
                    while ((!_maze[_curRow, _curCol].HasTopWall) && (_curRow > row))
                    {
                        _curRow--;
                    }
                }
            }
        }
        private void OnMazeCellClick(object sender, RoutedEventArgs e)
        {
            if (_isMazeSolved || _solutionTimer.IsEnabled)
            {
                return;
            }

            var button = sender as Button;
            var tag = (int)button.Tag;
            var col = tag & 0xFFFF;
            var row = tag >> 16;

            TryMoveRunner(row, col);

            var curContent = _curButton.Content;
            _curButton.Content = null;
            _curButton = FindName($"button_{_curRow}_{_curCol}") as Button;
            _curButton.Content = curContent;

            if ((_curRow == _numRows - 1) && (_curCol == _numCols - 1))
            {
                string message = $"Congratulations!! You have solved the maze!";
                DialogText.Text = message;
                DialogGrid.Visibility = Visibility.Visible;
            }
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            //ResetBoard();
            DialogGrid.Visibility = Visibility.Collapsed;

            OnNewMaze(this, null);
        }
    }
}
