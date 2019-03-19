//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
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
            
            new SolidColorBrush(Colors.Transparent),

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

        private string _answerString;
        public string AnswerString
        {
            get { return _answerString; }
            set
            {
                SetField<string>(ref _answerString, value, "AnswerString");
            }
        }

        private Brush _backgroundColor;
        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set { SetField<Brush>(ref _backgroundColor, value, "BackgroundColor"); }
        }

        private int _refIndex;
        public int RefIndex
        {
            get { return _refIndex; }
            set
            {
                SetField<int>(ref _refIndex, value, "IntVal");
            }
        }

        private Double _cellSize;
        public Double CellSize
        {
            get { return _cellSize; }
            set
            {
                SetField<Double>(ref _cellSize, value, "CellSize");
            }
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
        private int _SlideSpeed = 200;
        private int _AddSpeed = 500;
        private int _SpawnSpeed = 25;        
        private int _maxCells;
        private Random _random = new Random();

        CompositionScopedBatch _slideBatchAnimation;
        CompositionScopedBatch _addAdjacentBatchAnimation;
        static bool _busyAnimating = false;

        private bool _gameOver;
        public bool GameOver
        {
            get { return _gameOver; }
            set { SetField<bool>(ref _gameOver, value, "GameOver"); }
        }


        private int _boardSize;
        public int BoardSize
        {
            get { return _boardSize; }
            set { SetField<int>(ref _boardSize, value, "BoardSize"); }
        }

        private int _highScore;
        public int HighScore
        {
            get { return _highScore; }
            set
            {
                var appSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                appSettings.Values["highscore"] = value.ToString();
                SetField<int>(ref _highScore, value, "HighScore");
            }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set
            {
                if (value > HighScore)
                {
                    HighScore = value;
                }
                SetField<int>(ref _score, value, "Score");
            }
        }

        private Double _cellSpace = 75;
        public Double CellSpace
        {
            get { return _cellSpace; }
            set
            {
                SetField<Double>(ref _cellSpace, value, "CellSpace");
                UpdateCellSizes();
            }
        }

        private void UpdateCellSizes()
        {
            double borderSpace = _boardSize * 4; // cells get default margin of 4
            double cellSize = (this.CellSpace - borderSpace) / _boardSize;

            foreach (Cell cell in Cells)
            {
                cell.CellSize = cellSize;
            }
        }

        public List<Cell> Cells { get; set; }
        private List<FrameworkElement> Buttons { get; set; }

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

        public void LoadButtonsList(GridView GameBoardGrid)
        {
            Buttons = new List<FrameworkElement>();            
            var controls = (GameBoardGrid.ItemsPanelRoot as ItemsWrapGrid).Children;
            foreach (FrameworkElement b in controls)
            {
                Buttons.Add(b);
                var newButtonVisual = ElementCompositionPreview.GetElementVisual(b);              
            }
        }

        public void Reset()
        {
            var appSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            string storedHighscore = appSettings.Values["highscore"] as string;
            if (storedHighscore != null)
            {
                HighScore = int.Parse(storedHighscore);
            }
            else
            {
                HighScore = 0;
            }

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
                    if (cur.IntVal == 0)
                    {
                        return false;
                    }

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
                Cells[i].RefIndex = i;
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

            ///Animate the appearance of the new cell value
            ///
            if (Buttons != null)
            {
                var uiElement = Buttons[blankCells[index].RefIndex];
                var newCellVisual = ElementCompositionPreview.GetElementVisual(uiElement);
                var compositor = newCellVisual.Compositor;

                newCellVisual.CenterPoint = new System.Numerics.Vector3((float)uiElement.ActualWidth / 2, (float)uiElement.ActualHeight / 2, 0f);

                var scaleAnimation = compositor.CreateSpringVector3Animation();
                scaleAnimation.InitialValue = new System.Numerics.Vector3(0.5f, 0.5f, 0f);
                scaleAnimation.FinalValue = new System.Numerics.Vector3(1.0f, 1.0f, 0f);

                scaleAnimation.DampingRatio = 0.09f;
                scaleAnimation.Period = TimeSpan.FromMilliseconds(_SpawnSpeed);

                newCellVisual.StartAnimation(nameof(newCellVisual.Scale), scaleAnimation);

                Canvas.SetZIndex(uiElement, GetHighestButtonIndex() + 1);
            }

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
                ///Working backwards from the opposite edge that you are sliding in to the edge that you are sliding from
                //find first cell that is not empty and set j to the cell number
                //if the first cell at the edge you are sliding from is not empty then j will come out as cur
                //if all the cells are empty j will come out as the last cell or the end
                while ((j != end) && (Cells[j].IntVal == 0))
                {
                    j += delta;
                }
                
                //if the first cell is not empty it can't slide anywhere
                ///for any other cell 
                if ((j != cur) && (Cells[j].IntVal != 0))
                {
                    ///Animate cell movement                                        
                    ///Appear to slide the visual of the FromCell to the position of the ToCell                    
                    ///But, the ToCell gets immediately populated so in actuality
                    ///Move the ToCell to the FromCell position immediately
                    ///Then animate the relocated ToCell back to its original position
                    ///The Fromcell doesn't need to move
                    ///The empty position of the ToCell will be taken care of by the duplicate background board cells

                    var slideToCellVisual = ElementCompositionPreview.GetElementVisual(Buttons[cur]);
                    var compositor = slideToCellVisual.Compositor;
                    var slideFromCellVisual = ElementCompositionPreview.GetElementVisual(Buttons[j]);

                    var easing = compositor.CreateLinearEasingFunction();

                    ///slideAnimation
                    ///                    
                    var initialToOffset = slideToCellVisual.Offset;
                    var initialFromOffset = slideFromCellVisual.Offset;



                    var slideAnimation = compositor.CreateVector3KeyFrameAnimation();
                    slideAnimation.InsertKeyFrame(0f, initialFromOffset);
                    slideAnimation.InsertKeyFrame(1f, initialToOffset, easing);
                    slideAnimation.Duration = TimeSpan.FromMilliseconds(_SlideSpeed);

                    slideToCellVisual.StartAnimation(nameof(slideToCellVisual.Offset), slideAnimation);

                    Canvas.SetZIndex(Buttons[cur], GetHighestButtonIndex() + 1);

                    Cells[cur].IntVal = Cells[j].IntVal;
                    Cells[j].IntVal = 0;
                    slideSuccess = true;
                }
                cur += delta;
            }
            return slideSuccess;
        }

        private int GetHighestButtonIndex()
        {
            int highZ = int.MinValue;
            foreach (FrameworkElement e in Buttons)
            {
                var z = Canvas.GetZIndex(e);
                if (z > highZ)
                {
                    highZ = z;
                }
            }
            return highZ;
        }

        private bool AddAdjacent(int start, int end, int delta)
        {
            bool addSuccess = false;
            int cur = start;
            for (int i = 0; i < _boardSize - 1; i++)
            {
                if ((Cells[cur + delta].IntVal != 0) && (Cells[cur].IntVal > 0) && (Cells[cur].IntVal == Cells[cur + delta].IntVal))
                {

                    ///Animate cell movement
                    var slideToCellVisual = ElementCompositionPreview.GetElementVisual(Buttons[cur]);
                    var compositor = slideToCellVisual.Compositor;
                    var slideFromCellVisual = ElementCompositionPreview.GetElementVisual(Buttons[cur + delta]);

                    //very brittle way to get visuals for the text fields inside the cell template
                    var lvp = VisualTreeHelper.GetChild(Buttons[cur], 0);
                    var border = VisualTreeHelper.GetChild(lvp, 0);
                    var grid = VisualTreeHelper.GetChild(border, 0);
                    var cellText = VisualTreeHelper.GetChild(grid, 0);
                    var answerText = VisualTreeHelper.GetChild(grid, 1);

                    var answerTextVisual = ElementCompositionPreview.GetElementVisual(answerText as FrameworkElement);
                    var cellTextVisual = ElementCompositionPreview.GetElementVisual(cellText as FrameworkElement);
                   
                    var easing = compositor.CreateLinearEasingFunction();

                    ///Scale the ToCell to breifly be twice the size and then back down to regular size
                    ///
                    slideToCellVisual.CenterPoint = new System.Numerics.Vector3(slideToCellVisual.Size.X / 2, slideToCellVisual.Size.Y / 2, 0f);
                    var scaleUpAnimation = compositor.CreateVector3KeyFrameAnimation();
                    scaleUpAnimation.InsertKeyFrame(0f, new System.Numerics.Vector3(1.0f, 1.0f, 0f), easing);
                    scaleUpAnimation.InsertKeyFrame(0.5f, new System.Numerics.Vector3(1.5f, 1.5f, 0f), easing);
                    scaleUpAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1.0f, 1.0f, 0f), easing);
                    scaleUpAnimation.Duration = TimeSpan.FromMilliseconds(_AddSpeed);

                    ///Slide the ToCell back from the FromCell position 
                    ///Be sure to return it to its original position afterwards
                    var initialFromOffset = new Vector3(slideFromCellVisual.Offset.X, slideFromCellVisual.Offset.Y, slideFromCellVisual.Offset.Z);
                    var initialToOffset = new Vector3(slideToCellVisual.Offset.X, slideToCellVisual.Offset.Y, slideToCellVisual.Offset.Z);
                    var slideAnimation = compositor.CreateVector3KeyFrameAnimation();
                    slideAnimation.InsertKeyFrame(0, initialFromOffset);
                    slideAnimation.InsertKeyFrame(1, initialToOffset, easing);
                    slideAnimation.Duration = TimeSpan.FromMilliseconds(_SlideSpeed / 2);

                    //show cell text
                    var showCellTextAnimation = compositor.CreateScalarKeyFrameAnimation();
                    showCellTextAnimation.InsertKeyFrame(0, 0f);
                    showCellTextAnimation.InsertKeyFrame(1, 1f);
                    showCellTextAnimation.Duration = TimeSpan.FromMilliseconds(_AddSpeed * 2);

                    //Show answer
                    var showAnswerAnimation = compositor.CreateScalarKeyFrameAnimation();
                    showAnswerAnimation.InsertKeyFrame(0, 1f);
                    showAnswerAnimation.InsertKeyFrame(1, 0.0f);
                    showAnswerAnimation.Duration = TimeSpan.FromMilliseconds(_AddSpeed * 3);

                    var scaleAnswerAnimation = compositor.CreateVector3KeyFrameAnimation();
                    scaleAnswerAnimation.InsertKeyFrame(0f, new System.Numerics.Vector3(1.0f, 1.0f, 0f), easing);
                    scaleAnswerAnimation.InsertKeyFrame(0.5f, new System.Numerics.Vector3(1.5f, 1.5f, 0f), easing);
                    scaleAnswerAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1.0f, 1.0f, 0f), easing);
                    scaleAnswerAnimation.Duration = TimeSpan.FromMilliseconds(_AddSpeed);

                    slideToCellVisual.StartAnimation(nameof(slideToCellVisual.Offset), slideAnimation);

                    _slideBatchAnimation.Suspend();
                    _addAdjacentBatchAnimation.Suspend();
                    slideToCellVisual.StartAnimation(nameof(slideToCellVisual.Scale), scaleUpAnimation);
                    answerTextVisual.StartAnimation(nameof(answerTextVisual.Opacity), showAnswerAnimation);                    
                    answerTextVisual.StartAnimation(nameof(answerTextVisual.Scale), scaleAnswerAnimation);
                    cellTextVisual.StartAnimation(nameof(cellTextVisual.Opacity), showCellTextAnimation);
                    _addAdjacentBatchAnimation.Resume();
                    _slideBatchAnimation.Resume();

                    Canvas.SetZIndex(Buttons[cur], GetHighestButtonIndex() + 1);

                    Cells[cur].AnswerString = Cells[cur].IntVal.ToString() + " + " + Cells[cur + delta].IntVal.ToString();

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
            if (_busyAnimating)
            {
                return;
            }

            if (GameOver)
            {
                return;
            }

            _busyAnimating = true;
            var compositor = ElementCompositionPreview.GetElementVisual(Window.Current.Content).Compositor;
            if (_slideBatchAnimation != null)
            {
                _slideBatchAnimation.Completed -= SlideBatchAnimation_Completed;
                _slideBatchAnimation.Dispose();
            }

            _slideBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _slideBatchAnimation.Completed += SlideBatchAnimation_Completed;

            var firstslideBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            firstslideBatchAnimation.Completed += FirstslideBatchAnimation_Completed;
            firstslideBatchAnimation.Comment = start + "," + end + "," + delta + "," + increment;

            bool change = false;
            for (int i = 0; i < _boardSize; i++)
            {
                change = SlideRowOrCol(start, end, delta) || change;
                start += increment;
                end += increment;
            }
            firstslideBatchAnimation.Comment = firstslideBatchAnimation.Comment + "," + change;
            firstslideBatchAnimation.End();
        }

        private void FirstslideBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            string[] parms = (sender as CompositionScopedBatch).Comment.Split(',');

            int start = int.Parse(parms[0]);
            int end = int.Parse(parms[1]);
            int delta = int.Parse(parms[2]);
            int increment = int.Parse(parms[3]);
            bool change = bool.Parse(parms[4]);

            var compositor = ElementCompositionPreview.GetElementVisual(Window.Current.Content).Compositor;
            _addAdjacentBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _addAdjacentBatchAnimation.Completed += AddAdjacentBatchAnimation_Completed;
            _addAdjacentBatchAnimation.Comment = start + "," + end + "," + delta + "," + increment;

            for (int i = 0; i < _boardSize; i++)
            {
                change = AddAdjacent(start, end, delta) || change;
                start += increment;
                end += increment;
            }

            _addAdjacentBatchAnimation.Comment = _addAdjacentBatchAnimation.Comment + "," + change;
            _addAdjacentBatchAnimation.End();

            if (change)
            {
                OnPropertyChanged("Score");
            }            
        }

        private void AddAdjacentBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            string[] parms = (sender as CompositionScopedBatch).Comment.Split(',');

            int start = int.Parse(parms[0]);
            int end = int.Parse(parms[1]);
            int delta = int.Parse(parms[2]);
            int increment = int.Parse(parms[3]);
            bool change = bool.Parse(parms[4]);

            for (int i = 0; i < _boardSize; i++)
            {
                change = SlideRowOrCol(start, end, delta) || change;
                start += increment;
                end += increment;
            }

            _slideBatchAnimation.Comment = change.ToString();
            _slideBatchAnimation.End();

            var frame = (Frame)Window.Current.Content;
            var mainPage = (MainPage)frame.Content;
        }

        private void SlideBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            string[] parms = (sender as CompositionScopedBatch).Comment.Split(',');

            bool change = bool.Parse(parms[0]);
            bool wasNewTileGenerated = false;

            if (change)
            {
                wasNewTileGenerated = GenerateNextTile();
            }

            if ((!wasNewTileGenerated) && IsBoardFull())
            {
                GameOver = true;
                OnPropertyChanged("GameOver");
            }

            _busyAnimating = false;
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

        private SolidColorBrush _solidTileForegroundBrush;       

        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            VersionTextBlock.Text = GetAppVersion();

            Board = new Board(4);

            CoreWindow.GetForCurrentThread().KeyDown += new Windows.Foundation.TypedEventHandler<CoreWindow, KeyEventArgs>(delegate (CoreWindow sender, KeyEventArgs args)
            {
                GazeInput.GetGazePointer(this).Click();
            });

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) =>
            {
                GazeInput.LoadSettings(sharedSettings);
            });
        }

        private void OnNewGame(object sender, RoutedEventArgs e)
        {
            Board.Reset();
            Board.LoadButtonsList(GameBoardGrid);            
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

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private static T FindFirstChildOfType<T>(DependencyObject startNode)
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                if ((current.GetType()).Equals(typeof(T)))
                {
                    return (T)Convert.ChangeType(current, typeof(T));
                }
                return FindFirstChildOfType<T>(current);
            }
            return default(T);
        }

        private void TurnOffGridViewClipping()
        {
            var scrollContentPresenter = FindFirstChildOfType<ScrollContentPresenter>(GameBoardGrid);
            scrollContentPresenter.Clip = null;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsWrapGrid itemsWrapGrid = GameBoardGrid.ItemsPanelRoot as ItemsWrapGrid;
            itemsWrapGrid.MaximumRowsOrColumns = Board.BoardSize;
            DataContext = Board;

            TurnOffGridViewClipping();
            Board.LoadButtonsList(GameBoardGrid);
            Board.Reset();            

            var boardVisual = ElementCompositionPreview.GetElementVisual(GameBoardGrid);
            boardVisual.BorderMode = CompositionBorderMode.Soft;

            var controls = (GameBoardGrid.ItemsPanelRoot as ItemsWrapGrid).Children;
            foreach (FrameworkElement b in controls)
            {                
                var newButtonVisual = ElementCompositionPreview.GetElementVisual(b);             
            }

            _solidTileForegroundBrush = (SolidColorBrush)this.Resources["TileForeground"];

            GazeInput.DwellFeedbackProgressBrush = _solidTileForegroundBrush;
            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);

        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void GameBoardGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Double narrowEdge = GameBoardGrid.ActualHeight ;
            if (GameBoardGrid.ActualWidth  < narrowEdge)
            {
                narrowEdge = GameBoardGrid.ActualWidth ;
            }
            Board.CellSpace = narrowEdge;
        }       
    }
}
