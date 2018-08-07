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
using Windows.UI.Core;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.Media.Core;

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
        Thickness _newBorderThickness;
        Thickness _newSolidBorderThickness;
        Border _newBorder;
        Button _newButton;
        Style _newButtonStyle;
        Cell _targetCell;

        int _currentMazeCell;        
        List<Point> _mazeCells;
        List<Point> _breadCrumbs;

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
                //Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/doghouse.png")),
                Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/LunasHouse.png")),
                VerticalAlignment = VerticalAlignment.Center
            };

            _mazeComplete = new Image
            {
                //Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/dogInHouse.png")),
                Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/LunaInHerHouse.png")),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            _newSolidBorderThickness = new Thickness(_borderThickness);
            _newButtonStyle = (Style)this.Resources["MazeCellStyle"];
            
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
            _breadCrumbs = new List<Point>();

            Loaded += MainPage_Loaded;

            CoreWindow.GetForCurrentThread().KeyDown += new Windows.Foundation.TypedEventHandler<CoreWindow, KeyEventArgs>(delegate (CoreWindow sender, KeyEventArgs args) {
                GazeInput.GetGazePointer(this).Click();
            });

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                GazeInput.LoadSettings(sharedSettings);
            });
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
            var brush = SolveBrush;            
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

        private void ClearSolution()
        {
            int stepCol = _numCols - 1;
            int stepRow = _numRows - 1;

            for (int step = _solution.Count()-1; step >-1; step--)
            {
                var pos = _solution.ElementAt(step);

                if (pos == Direction.Left)
                {
                    stepCol++;
                }
                if (pos == Direction.Right)
                {
                    stepCol--;
                }
                if (pos == Direction.Up)
                {
                    stepRow++;
                }
                if (pos == Direction.Down)
                {
                    stepRow--;
                }
                var cell = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == stepRow && Grid.GetColumn(b) == stepCol) as Border;

                cell.Background = null;
            }                        
        }

        private void OnSolveMaze(object sender, RoutedEventArgs e)
        {
            if (_cellCreationTimer.IsEnabled || _openCellTimer.IsEnabled)
            {
                return;
            }

            if (_isMazeSolved)
            {
                //Reset to resolve
                ClearSolution();
                Border endBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _numRows - 1 && Grid.GetColumn(b) == _numCols - 1) as Border;
                Button endButton = endBorder.Child as Button;
                endButton.Content = _mazeEnd;
                _curRow = 0;
                _curCol = 0;
                Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
                _curButton = curBorder.Child as Button;
                _curButton.Content = _mazeRunner;
                _isMazeSolved = false;
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

            _breadCrumbs.Clear();
            ClearBreadCrumbs();

            int remainingHeight = (int)(this.ActualHeight - Toolbar.ActualHeight)-(int)(MazeBorder.Margin.Bottom + MazeBorder.Margin.Top);            
            _cellSize = remainingHeight / _numRows;
            _numCols = (int)((this.ActualWidth - (MazeBorder.Margin.Left + MazeBorder.Margin.Right)) / _cellSize);

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
            for (int i = 0; i < _numCols; i++)
            {            
                AddBorderToMaze();

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
        }
        

        void AddBorderToMaze()
        {            
            _newBorder = new Border();
            _newBorder.BorderBrush = BorderBrush;            

            _newBorder.BorderThickness = _newSolidBorderThickness;

            Grid.SetRow(_newBorder, (int)_mazeCells[_currentMazeCell].Y);
            Grid.SetColumn(_newBorder, (int)_mazeCells[_currentMazeCell].X);

            MazeGrid.Children.Add(_newBorder);                       
        }

        private void OnOpenCellTimer_Tick(object sender, object e)
        {
            for (int i=0; i < _numCols; i++) { 

                OpenCellBorderAndAddButton();

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
        }

        void OpenCellBorderAndAddButton()
        {
            _newBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _mazeCells[_currentMazeCell].Y && Grid.GetColumn(b) == _mazeCells[_currentMazeCell].X) as Border;            
            
            _newBorderThickness = new Thickness();
            _targetCell = _maze[(int)_mazeCells[_currentMazeCell].Y, (int)_mazeCells[_currentMazeCell].X];
            _newBorderThickness.Left = (_targetCell.HasLeftWall) ? _borderThickness : 0;
            _newBorderThickness.Top = (_targetCell.HasTopWall) ? _borderThickness : 0;
            _newBorderThickness.Right = (_targetCell.HasRightWall) ? _borderThickness : 0;
            _newBorderThickness.Bottom = (_targetCell.HasBottomWall) ? _borderThickness : 0;
            _newBorder.BorderThickness = _newBorderThickness;

            _newButton = new Button();

            _newButton.Name = $"button_{_mazeCells[_currentMazeCell].Y}_{_mazeCells[_currentMazeCell].X}";
            _newButton.Click += OnMazeCellClick;
            _newButton.Width = _cellSize;
            _newButton.Height = _cellSize;
            _newButton.Style = _newButtonStyle;
            _newBorder.Child = _newButton;
        }

        private void TryMoveRunner(int row, int col)
        {
            if (row == _curRow)
            {
                if (_curCol < col)
                {
                    while ((!_maze[_curRow, _curCol].HasRightWall) && (_curCol < col))
                    {
                        DropBreadCrumb();
                        _curCol++;                        
                    }
                    return;
                }
                else if (_curCol > col)
                {
                    while ((!_maze[_curRow, _curCol].HasLeftWall) && (_curCol > col))
                    {
                        DropBreadCrumb();
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
                        DropBreadCrumb();
                        _curRow++;
                    }
                    return;
                }
                else if (_curRow > row)
                {
                    while ((!_maze[_curRow, _curCol].HasTopWall) && (_curRow > row))
                    {
                        DropBreadCrumb();
                        _curRow--;
                    }
                }
            }
        }

        private void DropBreadCrumb()
        {
            _breadCrumbs.Add(new Point(_curCol, _curRow));            
            var brush = BordercrumbBrush;
            var cell = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
            cell.Background = brush;
        }

        private void ClearBreadCrumbs()
        {
            for (int i = 0; i < _breadCrumbs.Count(); i++)
            {
                var cell = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _breadCrumbs[i].Y && Grid.GetColumn(b) == _breadCrumbs[i].X) as Border;
                cell.Background = null;
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
                FireworksVideoPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Fireworks.mp4"));
                FireworksVideoPlayer.MediaPlayer.Play();
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
