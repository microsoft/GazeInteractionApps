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
        Image _mazeComplete;
        SolidColorBrush _borderBrush;

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

        int _cellSize;
        int _borderThickness = 2;

        int _currentMazeCell;        
        List<Point> _mazeCells;

        DispatcherTimer _cellCreationTimer;
        DispatcherTimer _openCellTimer;

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

            _mazeComplete = new Image
            {
                Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/dogInHouse.png")),
                VerticalAlignment = VerticalAlignment.Center
            };

            _borderBrush = (SolidColorBrush)this.Resources["BorderBrush"];

            _mazeCreator = Creator.GetCreator();

            _solutionTimer = new DispatcherTimer();
            _solutionTimer.Interval = TimeSpan.FromSeconds(0.1);
            _solutionTimer.Tick += OnSolutionTimerTick;

            _cellCreationTimer = new DispatcherTimer();
            _cellCreationTimer.Interval = TimeSpan.FromMilliseconds(50);
            _cellCreationTimer.Tick += OnCellCreationTimer_Tick;

            _openCellTimer = new DispatcherTimer();
            _openCellTimer.Interval = TimeSpan.FromMilliseconds(50);
            _openCellTimer.Tick += OnOpenCellTimer_Tick;

            _mazeCells = new List<Point>();

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
            var cell = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;            

            cell.Background = brush;

            var currentContent = _curButton.Content;

            if (_solutionCurIndex >= _solution.Count())
            {
                _solutionTimer.Stop();
                _isMazeSolved = true;

                                            
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

            Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
            _curButton = curBorder.Child as Button;            
            _curButton.Content = currentContent;
        }

        private void OnSolveMaze(object sender, RoutedEventArgs e)
        {
            if (_cellCreationTimer.IsEnabled || _openCellTimer.IsEnabled)
            {
                return;
            }

            var solver = MazeCreator.Solver.Create();
            _solution = solver.Solve(_maze, new Position(_curRow, _curCol), new Position(_numRows - 1, _numCols - 1));

            _solutionCurIndex = 0;
            _solutionTimer.Start();
        }

        private void BuildMaze()
        {
            _solutionTimer.Stop();
            _cellCreationTimer.Stop();
            _openCellTimer.Stop();

            int remainingHeight = (int)(this.ActualHeight - Toolbar.ActualHeight);
            _cellSize = remainingHeight / _numRows;
            _numCols = (int)(this.ActualWidth / _cellSize);

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

            MazeGrid.Width = _cellSize * _numCols;                        

            _mazeCells.Clear();
            for (int i = 0; i < _numRows; i++)
            {
                for (int j = 0; j < _numCols; j++)
                {
                    _mazeCells.Add(new Point(j, i));
                }
            }
            
            _mazeCells = ShuffleMazeCells(_mazeCells);

            _currentMazeCell = 0;

            _cellCreationTimer.Start();           
        }

        private List<Point> ShuffleMazeCells(List<Point> mazeCells)
        {
            int n = mazeCells.Count();
            Random rnd = new Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                Point value = mazeCells[k];
                mazeCells[k] = mazeCells[n];
                mazeCells[n] = value;
            }
            return mazeCells;
        }
        
        private void OnCellCreationTimer_Tick(object sender, object e)
        {            
            AddButtonToMaze();

            if (_currentMazeCell == _mazeCells.Count() -1)
            {               
                _cellCreationTimer.Stop();

                _mazeCells = ShuffleMazeCells(_mazeCells);

                _currentMazeCell = 0;

                _openCellTimer.Start();
                return;
            }            
            _currentMazeCell += 1;            
        }

        void AddButtonToMaze()
        {
            var button = new Button();            

            button.Name = $"button_{_mazeCells[_currentMazeCell].Y}_{_mazeCells[_currentMazeCell].X}";
            button.Click += OnMazeCellClick;
            button.Width = _cellSize;
            button.Height = _cellSize;
            button.Style = (Style)this.Resources["MazeCellStyle"];

            var border = new Border();
            border.BorderBrush = _borderBrush;
            border.Child = button;

            var thickness = new Thickness(_borderThickness);                       
            border.BorderThickness = thickness;

            Grid.SetRow(border, (int)_mazeCells[_currentMazeCell].Y);
            Grid.SetColumn(border, (int)_mazeCells[_currentMazeCell].X);

            MazeGrid.Children.Add(border);                       
        }

        private void OnOpenCellTimer_Tick(object sender, object e)
        {
            OpenCellInMaze();

            if (_currentMazeCell == _mazeCells.Count() - 1)
            {
                Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == 0 && Grid.GetColumn(b) == 0) as Border;
                _curButton = curBorder.Child as Button;
                _curButton.Content = _mazeRunner;

                Border targetBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _numRows - 1 && Grid.GetColumn(b) == _numCols - 1) as Border;
                _targetButton = targetBorder.Child as Button;
                _targetButton.Content = _mazeEnd;

                _openCellTimer.Stop();
                return;
            }
            _currentMazeCell += 1;
        }

        void OpenCellInMaze()
        {
            var border = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _mazeCells[_currentMazeCell].Y && Grid.GetColumn(b) == _mazeCells[_currentMazeCell].X) as Border;            
            
            var thickness = new Thickness();
            var cell = _maze[(int)_mazeCells[_currentMazeCell].Y, (int)_mazeCells[_currentMazeCell].X];
            thickness.Left = (cell.HasLeftWall) ? _borderThickness : 0;
            thickness.Top = (cell.HasTopWall) ? _borderThickness : 0;
            thickness.Right = (cell.HasRightWall) ? _borderThickness : 0;
            thickness.Bottom = (cell.HasBottomWall) ? _borderThickness : 0;
            border.BorderThickness = thickness;
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
            if (_isMazeSolved || _solutionTimer.IsEnabled || _cellCreationTimer.IsEnabled || _openCellTimer.IsEnabled)
            {
                return;
            }

            var button = sender as Button;            

            var col = Grid.GetColumn(button.Parent as FrameworkElement);
            var row = Grid.GetRow(button.Parent as FrameworkElement);

            TryMoveRunner(row, col);

            var curContent = _curButton.Content;
            _curButton.Content = null;
            Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
            _curButton = curBorder.Child as Button;            
            _curButton.Content = curContent;

            if ((_curRow == _numRows - 1) && (_curCol == _numCols - 1))
            {
                _curButton.Content = _mazeComplete;
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
