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
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Automation;
using Microsoft.Services.Store.Engagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Maze
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        const int MIN_ROWS = 3;
        int _numRows;
        int _numCols;

        int _numMoves;    
        Image _mazeRunner;
        Image _mazeEnd;
        Image _mazeComplete;

        bool _isMazeSolved;
        bool _usedSolver;
        MazeCreator.Core.ICreator _mazeCreator;
        MazeCreator.Core.Maze _maze;

        bool _interactionPaused = false;

        DispatcherTimer _solutionTimer;
        Direction[] _solution;

        int _curRow;
        int _curCol;
        Button _curButton;
        Button _targetButton;
        int _solutionCurIndex;

        int _cellSize;
        double _cellHeight;
        int _borderThickness = 2;
        Thickness _newBorderThickness;
        Thickness _newSolidBorderThickness;
        Border _newBorder;
        Button _newButton;
        Style _newButtonStyle;
        Cell _targetCell;

        SolidColorBrush _toolButtonBrush;
        SolidColorBrush _borderBrush;
        SolidColorBrush _pausedButtonBrush = new SolidColorBrush(Colors.Black);

        int _currentMazeCell;
        List<Point> _mazeCells;
        List<Point> _breadCrumbs;

        DispatcherTimer _cellCreationTimer;
        DispatcherTimer _openCellTimer;

        SpriteVisual _mazeRunnerVisual;
        Compositor _compositor;

        bool _animationIsRunning = false;
        CompositionScopedBatch _batch;
        CompositionScopedBatch _endBatch;

        CompositionSurfaceBrush _breadCrumbBrush;

        enum TravelSpeed
        {
            Walk,
            Run,
            Jump
        }

        public GamePage()
        {
            this.InitializeComponent();

            _toolButtonBrush = (SolidColorBrush)this.Resources["ToolBarButtonBackground"];
            _borderBrush = (SolidColorBrush)this.Resources["BorderBrush"];

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            VersionTextBlock.Text = GetAppVersion();

            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            LoadedImageSurface loadedSurface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/BorderDot.png"), new Size(_cellSize, _cellSize));
            _breadCrumbBrush = _compositor.CreateSurfaceBrush();
            _breadCrumbBrush.Surface = loadedSurface;

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

            Loaded += GamePage_Loaded;

            CoreWindow.GetForCurrentThread().KeyDown += CoredWindow_KeyDown;
          
            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                GazeInput.LoadSettings(sharedSettings);
            });
        }

        private void CoredWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!args.KeyStatus.WasKeyDown)
            {
                GazeInput.GetGazePointer(this).Click();
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

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            getRootScrollViewer().Focus(FocusState.Programmatic);

            ResetMaze();
            //BuildMaze();
            //LoadMazeRunnerVisual();
            //AnimateMazeRunnerVisualToCell(0, 0, TravelSpeed.Jump, 0);
        }

        private void ResetMaze()
        {
            PlayAgainText.Visibility = Visibility.Collapsed;
            BuildMaze();
            LoadMazeRunnerVisual();
            AnimateMazeRunnerVisualToCell(0, 0, TravelSpeed.Jump, 0);
            _usedSolver = false;
            _numMoves = 0;
            MoveCountTextBlock.Text = _numMoves.ToString();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            switch (e.Parameter.ToString())
            {
                case "3":
                    _numRows = 3;
                    //_numCols = 4;                    
                    break;

                case "4":
                    _numRows = 4;
                    //_numCols = 6;                    
                    break;

                case "5":
                    _numRows = 5;
                    //_numCols = 6;                    
                    break;

                case "6":
                    _numRows = 6;
                    //_numCols = 11;                    
                    break;

                default:
                    _numRows = 4;
                    //_numCols = 4;                    
                    break;
            }            
        }

        private void OnNewMaze(object sender, RoutedEventArgs e)
        {
            ResetMaze();
            //BuildMaze();
            //LoadMazeRunnerVisual();
            //AnimateMazeRunnerVisualToCell(0, 0, TravelSpeed.Jump, 0);
        }

        private void OnBiggerMaze(object sender, RoutedEventArgs e)
        {
            _numRows++;
            _numCols++;
            BuildMaze();
            LoadMazeRunnerVisual();
            AnimateMazeRunnerVisualToCell(0, 0, TravelSpeed.Jump, 0);
        }

        private void OnSmallerMaze(object sender, RoutedEventArgs e)
        {
            _numCols = (_numCols > 3) ? _numCols - 1 : _numCols;
            _numRows = (_numRows > 3) ? _numRows - 1 : _numRows;
            BuildMaze();
            LoadMazeRunnerVisual();
            AnimateMazeRunnerVisualToCell(0, 0, TravelSpeed.Jump, 0);
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

                //_curButton.Content = _mazeComplete;
                HideMazeRunnerVisual(0);
                return;
            }
            else
            {
                _curButton.Content = null;
            }            

            var pos = _solution.ElementAt(_solutionCurIndex);
           
            //If cell has visualchild remove it            
            ElementCompositionPreview.SetElementChildVisual(cell, null);
           

            if (pos == Direction.Left)
            {
                _curCol--;
                AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Run, 0);
            }
            if (pos == Direction.Right)
            {
                _curCol++;
                AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Run, 0);
            }
            if (pos == Direction.Up)
            {
                _curRow--;
                AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Run, 0);
            }
            if (pos == Direction.Down)
            {
                _curRow++;
                AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Run, 0);
            }
            _solutionCurIndex++;

            Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
            _curButton = curBorder.Child as Button;

            if (_solutionCurIndex < _solution.Count())
            {
                _curButton.Content = currentContent;
            }
        }

        private void ClearSolution()
        {
            int stepCol = _numCols - 1;
            int stepRow = _numRows - 1;

            for (int step = _solution.Count() - 1; step > -1; step--)
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
            if (_cellCreationTimer.IsEnabled || _openCellTimer.IsEnabled || _interactionPaused || _animationIsRunning)
            {
                return;
            }

            if (_isMazeSolved)
            {
                //Reset to resolve
                ClearSolution();
                AnimateMazeRunnerVisualToCell(0, 0, TravelSpeed.Jump, 0);
                _mazeRunnerVisual.IsVisible = true;
                _mazeRunnerVisual.Opacity = 1;
                Border endBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _numRows - 1 && Grid.GetColumn(b) == _numCols - 1) as Border;
                Button endButton = endBorder.Child as Button;
                endButton.Content = _mazeEnd;
                _curRow = 0;
                _curCol = 0;
                Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
                _curButton = curBorder.Child as Button;
                _isMazeSolved = false;
            }
            var solver = MazeCreator.Solver.Create();
            _solution = solver.Solve(_maze, new Position(_curRow, _curCol), new Position(_numRows - 1, _numCols - 1));
            _usedSolver = true;
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

            int remainingHeight = (int)(this.ActualHeight - Toolbar.ActualHeight) - (int)(MazeBorder.Margin.Bottom + MazeBorder.Margin.Top);
            _cellSize = remainingHeight / _numRows;
            _numCols = (int)((this.ActualWidth - (MazeBorder.Margin.Left + MazeBorder.Margin.Right)) / _cellSize);

            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log($"BuildMaze-ETD:{GazeInput.IsDeviceAvailable}-{_numRows},{_numCols}");

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

            MazeTargetsGrid.Children.Clear();
            MazeTargetsGrid.RowDefinitions.Clear();
            MazeTargetsGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < _numRows; i++)
            {
                MazeGrid.RowDefinitions.Add(new RowDefinition());
                MazeTargetsGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < _numCols; i++)
            {
                MazeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                MazeTargetsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            MazeGrid.Width = _cellSize * _numCols;
            MazeTargetsGrid.Width = _cellSize * _numCols;

            _mazeCells.Clear();
            for (int i = 0; i < _numRows; i++)
            {
                for (int j = 0; j < _numCols; j++)
                {
                    _mazeCells.Add(new Point(j, i));
                    AddTargetToMaze(i, j);
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

                if (_currentMazeCell == _mazeCells.Count() - 1)
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

        void AddTargetToMaze(int row, int col)
        {
            double targetSizeFactor = 16;
            double targetSize = (_cellSize/ targetSizeFactor);

            Ellipse elipse = new Ellipse();
            elipse.Width = targetSize;
            elipse.Height = targetSize;
            elipse.Fill = _borderBrush;
            elipse.Opacity = 0.60;
         
            Grid.SetRow(elipse, row);
            Grid.SetColumn(elipse, col);

            MazeTargetsGrid.Children.Add(elipse);
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
            for (int i = 0; i < _numCols; i++)
            {

                OpenCellBorderAndAddButton();

                if (_currentMazeCell == _mazeCells.Count() - 1)
                {
                    Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == 0 && Grid.GetColumn(b) == 0) as Border;
                    _curButton = curBorder.Child as Button;

                    Border targetBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _numRows - 1 && Grid.GetColumn(b) == _numCols - 1) as Border;
                    _targetButton = targetBorder.Child as Button;
                    _targetButton.Padding = new Thickness(2);
                    _targetButton.Content = _mazeEnd;                   
                    _openCellTimer.Stop();
                    return;
                }
                _currentMazeCell += 1;
            }
        }

        void OpenCellBorderAndAddButton()
        {
            int pageHeaderButtonCount = 4;
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
            _newButton.SetValue(AutomationProperties.NameProperty, $"Room {_mazeCells[_currentMazeCell].Y} {_mazeCells[_currentMazeCell].X}");
            _newButton.TabIndex = (pageHeaderButtonCount-1) + (int)((_mazeCells[_currentMazeCell].Y * _numCols) + _mazeCells[_currentMazeCell].X);
            _newButton.Click += OnMazeCellClick;
            _newButton.Width = _cellSize;
            _newButton.Height = _cellSize;
            _newButton.Style = _newButtonStyle;
            _newBorder.Child = _newButton;
        }

        private int TryMoveRunner(int row, int col)
        {
            int crumbNum = 0;
            if (row == _curRow)
            {
                if (_curCol < col)
                {
                    while ((!_maze[_curRow, _curCol].HasRightWall) && (_curCol < col))
                    {
                        crumbNum++;
                        DropBreadCrumb(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                        _curCol++;
                        AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                    }
                    return crumbNum;
                }
                else if (_curCol > col)
                {
                    while ((!_maze[_curRow, _curCol].HasLeftWall) && (_curCol > col))
                    {
                        crumbNum++;
                        DropBreadCrumb(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                        _curCol--;
                        AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                    }
                    return crumbNum;
                }
            }
            else if (col == _curCol)
            {
                if (_curRow < row)
                {
                    while ((!_maze[_curRow, _curCol].HasBottomWall) && (_curRow < row))
                    {
                        crumbNum++;
                        DropBreadCrumb(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                        _curRow++;
                        AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                    }
                    return crumbNum;
                }
                else if (_curRow > row)
                {
                    while ((!_maze[_curRow, _curCol].HasTopWall) && (_curRow > row))
                    {
                        crumbNum++;
                        DropBreadCrumb(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                        _curRow--;
                        AnimateMazeRunnerVisualToCell(_curCol, _curRow, TravelSpeed.Walk, crumbNum);
                    }
                    return crumbNum;
                }
            }
            return 0;
        }

        private void DropBreadCrumb(int col, int row, TravelSpeed travelSpeed, int crumbCount)
        {
           
            var visual = _compositor.CreateSpriteVisual();
            visual.Size = new Vector2(_cellSize, (float)_cellHeight);
            visual.Brush = _breadCrumbBrush;
            visual.Opacity = 0;

            var cell = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
            ElementCompositionPreview.SetElementChildVisual(cell, visual);

            var animation = _compositor.CreateScalarKeyFrameAnimation();
            var easing = _compositor.CreateLinearEasingFunction();

            animation.InsertKeyFrame(1.0f, 1, easing);

            switch (travelSpeed)
            {
                case TravelSpeed.Walk:
                    animation.Duration = TimeSpan.FromMilliseconds(500);
                    animation.DelayTime = TimeSpan.FromMilliseconds(500 * crumbCount);
                    break;

                case TravelSpeed.Run:
                    animation.Duration = TimeSpan.FromSeconds(0.1 * crumbCount);
                    animation.DelayTime = TimeSpan.FromMilliseconds(0.1 * crumbCount);
                    break;

                case TravelSpeed.Jump:
                    animation.Duration = TimeSpan.FromMilliseconds(1 * crumbCount);
                    animation.DelayTime = TimeSpan.FromMilliseconds(1 * crumbCount);
                    break;
            }
            visual.StartAnimation(nameof(visual.Opacity), animation);

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
            if (_animationIsRunning || _interactionPaused) return;

            if (_isMazeSolved || _solutionTimer.IsEnabled || _cellCreationTimer.IsEnabled || _openCellTimer.IsEnabled)
            {
                return;
            }

            var button = sender as Button;

            var col = Grid.GetColumn(button.Parent as FrameworkElement);
            var row = Grid.GetRow(button.Parent as FrameworkElement);

            var crumbNum = TryMoveRunner(row, col);
          
            var curContent = _curButton.Content;
            _curButton.Content = null;
            Border curBorder = MazeGrid.Children.Cast<FrameworkElement>().First(b => Grid.GetRow(b) == _curRow && Grid.GetColumn(b) == _curCol) as Border;
            _curButton = curBorder.Child as Button;

            if (!((row == _numRows - 1) && (col == _numCols - 1)))
            {
                _curButton.Content = curContent;
            }

            //if ((_curRow == _numRows - 1) && (_curCol == _numCols - 1))
            //{
            //    _curButton.Content = _mazeComplete;
            //    HideMazeRunnerVisual(crumbNum);
            //    string message = $"Congratulations!! You have solved the maze!";
            //    DialogText.Text = message;
            //    DialogGrid.Visibility = Visibility.Visible;
            //    _isMazeSolved = true;
            //}

            _numMoves++;
            MoveCountTextBlock.Text = _numMoves.ToString();
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;            
            ResetMaze();
            SetTabsForPageView();
        }

        private void LoadMazeRunnerVisual()
        {
            if (_mazeRunnerVisual != null) _mazeRunnerVisual.Dispose();

            _cellHeight = MazeGrid.ActualHeight / _numRows;
            _mazeRunnerVisual = _compositor.CreateSpriteVisual();            
            _mazeRunnerVisual.Size = new Vector2(_cellSize, (float)_cellHeight);

            LoadedImageSurface loadedSurface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/luna.png"), new Size(_cellSize, _cellSize));
            CompositionSurfaceBrush surfaceBrush = _compositor.CreateSurfaceBrush();
            surfaceBrush.Surface = loadedSurface;
            _mazeRunnerVisual.Brush = surfaceBrush;

            ElementCompositionPreview.SetElementChildVisual(MazeGrid, _mazeRunnerVisual);
            _mazeRunnerVisual.IsVisible = true;
            _mazeRunnerVisual.Opacity = 1;
        }

        private void HideMazeRunnerVisual(int crumbCount)
        {
            var animation = _compositor.CreateScalarKeyFrameAnimation();
            var easing = _compositor.CreateLinearEasingFunction();

            animation.InsertKeyFrame(1.0f, 0, easing);
            animation.Duration = TimeSpan.FromMilliseconds(50);
            if (crumbCount > 1) crumbCount -= 1;
            animation.DelayTime = TimeSpan.FromMilliseconds(500 * crumbCount);

            if ((_curRow == _numRows - 1) && (_curCol == _numCols - 1))
            {
                animation.DelayTime =TimeSpan.FromMilliseconds(500);

                if (_endBatch != null)
                {
                    _endBatch.Dispose();
                }
                _endBatch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
                scaleAnimation.InsertKeyFrame(1f, new Vector3( 0.50f, 0.50f, 0f));
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);
                _mazeRunnerVisual.CenterPoint = new System.Numerics.Vector3((float)_mazeRunnerVisual.Size.X / 2, (float)_mazeRunnerVisual.Size.Y , 0f);
                _mazeRunnerVisual.StartAnimation(nameof(_mazeRunnerVisual.Scale), scaleAnimation);
                
                _endBatch.End();
                _endBatch.Completed += _endBatch_Completed;
                
            }


            _mazeRunnerVisual.StartAnimation(nameof(_mazeRunnerVisual.Opacity), animation);
        }

        private void _endBatch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            _curButton.Content = _mazeComplete;
        }

        private void AnimateMazeRunnerVisualToCell(int col, int row, TravelSpeed travelSpeed, int crumbCount)
        {
            //if ((row == _numRows - 1) && (col == _numCols - 1))
            //{
            //    return;
            //}

            _animationIsRunning = true;

            if (_batch != null)
            {
                _batch.Dispose();
            }
            _batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            var animation = _compositor.CreateVector3KeyFrameAnimation();
            var easing = _compositor.CreateLinearEasingFunction();
            
            var destination = new Vector3((float)(col * _cellSize), (float)(row * _cellHeight), 0.0F);

            animation.InsertKeyFrame(1.0f, destination, easing);

            switch (travelSpeed)
            {
                case TravelSpeed.Walk:
                    animation.Duration = TimeSpan.FromMilliseconds(500 * crumbCount);
                    break;

                case TravelSpeed.Run:
                    animation.Duration = TimeSpan.FromSeconds(0.1);
                    break;

                case TravelSpeed.Jump:
                    animation.Duration = TimeSpan.FromMilliseconds(1);
                    break;
            }
            _mazeRunnerVisual.StartAnimation(nameof(_mazeRunnerVisual.Offset), animation);
            _batch.Comment = crumbCount.ToString();
            _batch.End();
            _batch.Completed += _batch_Completed;
        }

        private async void _batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            int crumbNum = int.Parse((sender as CompositionScopedBatch).Comment);
            _animationIsRunning = false;

            if ((_curRow == _numRows - 1) && (_curCol == _numCols - 1))
            {
                string message;
                string congratsMessage="";

                //_mazeRunnerVisual.Opacity = 0;

                _isMazeSolved = true;
                //_curButton.Content = _mazeComplete;

                HideMazeRunnerVisual(crumbNum);

                StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();               

                await Task.Delay(500 + (500 * crumbNum));
                if (_usedSolver)
                {
                    var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    message = resourceLoader.GetString("SolvedWithHelpMessageString");
                    //message = $"With a little help you have solved the maze!";
                    //EndAnimation.Source = new BitmapImage(new Uri("ms-appx:///Assets/Luna_animated-Slow.gif"));
                    //EndAnimation.Source = null;
                    logger.Log($"EndOfMaze{_numRows}-ETD:{GazeInput.IsDeviceAvailable}-solve");
                }
                else
                {
                    var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();                    
                    congratsMessage = resourceLoader.GetString("CongratsMessageString");
                    //congratsMessage = "Congratualtions!!";

                    //message = $"You have solved the maze in {_numMoves} moves!";
                    message = $"{resourceLoader.GetString("CongratsMessageStringMoveCountPrefix")}{_numMoves}{resourceLoader.GetString("CongratsMessageStringMoveCountPostfix")}";

                    //EndAnimation.Source = new BitmapImage(new Uri("ms-appx:///Assets/Luna_animated-Fast.gif"));
                    logger.Log($"EndOfMaze{_numRows}-ETD:{GazeInput.IsDeviceAvailable}-m#{_numMoves}");
                }
                GazeInput.DwellFeedbackProgressBrush = _borderBrush;

                DialogText.Text = message;
                CongratsText.Text = congratsMessage;

                if (_numRows >= 6)
                {
                    BoardSizeText.Text = _numRows.ToString();
                    IncreaseBoardSizePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    IncreaseBoardSizePanel.Visibility = Visibility.Collapsed;
                }

                DialogGrid.Visibility = Visibility.Visible;
                SetTabsForDialogView();
                CloseDialogButton.Focus(FocusState.Pointer);
            }
        }

        private void IncreaseBoardSize_Click(object sender, RoutedEventArgs e)
        {
            int minCellSize = 20;
            if (((int)(this.ActualHeight - Toolbar.ActualHeight) - (int)(MazeBorder.Margin.Bottom + MazeBorder.Margin.Top)) / (_numRows + 1) > minCellSize)
            {
                _numRows += 1;
            }            
            BoardSizeText.Text = _numRows.ToString();
        }

        private async void OnBack(object sender, RoutedEventArgs e)
        {
            
            await Task.Delay(400);
            Frame.Navigate(typeof(MainPage));
        }

        private void OnPause(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (_interactionPaused)
            {
                PauseButtonText.Text = "\uE769";
                PauseButtonBorder.Background = _toolButtonBrush;
                GazeInput.SetInteraction(MazeGrid, Interaction.Enabled);
                GazeInput.SetInteraction(SolveButton, Interaction.Enabled);
                
                _interactionPaused = false;
                if (_isMazeSolved)
                {
                    ResetMaze();
                }
            }
            else
            {
                PauseButtonText.Text = "\uE768";
                PauseButtonBorder.Background = _pausedButtonBrush;
                GazeInput.SetInteraction(MazeGrid, Interaction.Disabled);
                GazeInput.SetInteraction(SolveButton, Interaction.Disabled);

                if (e != null)
                {
                    //User initiated pause, not code
                    StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                    logger.Log($"Paused-ETD:{GazeInput.IsDeviceAvailable}");
                }
                _interactionPaused = true;
            }
        }

        private async void DialogButton2_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;
            ResetMaze();
            await Task.Delay(400);
            Frame.Navigate(typeof(MainPage));
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void DismissButton(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            DialogGrid.Visibility = Visibility.Collapsed;
           
            PlayAgainText.Visibility = Visibility.Visible;
            OnPause(PauseButton, null);
            SetTabsForPageView();
            PauseButton.Focus(FocusState.Pointer);
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
            SolveButton.IsTabStop = false;
            ExitButton.IsTabStop = false;

            for (int row = 0; row < _numRows; row++)
            {
                for (int col = 0; col < _numCols; col++)
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
            SolveButton.IsTabStop = true;            
            ExitButton.IsTabStop = true;
            for (int row = 0; row < _numRows; row++)
            {
                for (int col = 0; col < _numCols; col++)
                {
                    string buttonName = $"button_{row}_{col}";
                    Button button = FindName(buttonName) as Button;
                    if (button != null)
                    {
                        button.IsTabStop = true;
                    }                    
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= CoredWindow_KeyDown;
        }
    }
}
