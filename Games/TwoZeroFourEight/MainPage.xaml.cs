//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Services.Store.Engagement;
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

            new SolidColorBrush(Color.FromArgb(255,215,253,253)),
            new SolidColorBrush(Color.FromArgb(255,171,235,243)),
            new SolidColorBrush(Color.FromArgb(255,140,208,243)),
            new SolidColorBrush(Color.FromArgb(255,92,191,241)),
            new SolidColorBrush(Color.FromArgb(255,255,250,205)),
            new SolidColorBrush(Color.FromArgb(255,253,242,140)),
            new SolidColorBrush(Color.FromArgb(255,247,214,127)),
            new SolidColorBrush(Color.FromArgb(255,244,184,109)),
            new SolidColorBrush(Color.FromArgb(255,175,248,188)),
            new SolidColorBrush(Color.FromArgb(255,126,237,147)),
            new SolidColorBrush(Color.FromArgb(255,92,208,130)),
            new SolidColorBrush(Color.FromArgb(255,62,185,144)),
            new SolidColorBrush(Color.FromArgb(255,215,217,255)),
            new SolidColorBrush(Color.FromArgb(255,199,141,255)),
            new SolidColorBrush(Color.FromArgb(255,167,154,247)),
            new SolidColorBrush(Color.FromArgb(255,127,127,255)),

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

    public sealed class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            int val = (int)value;

            if (val > 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Cell : NotificationBase
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
                SetField<int>(ref _refIndex, value, "RefIndex");
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

    public sealed class IntegerToStringConverter : IValueConverter
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
        private BoardSpeed _boardSpeed = BoardSpeed.Slow;
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
                StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                logger.Log($"HighScore:{_highScore}-ETD:{GazeInput.IsDeviceAvailable}");
            }
        }

        private int _highTile;
        public int HighTile
        {
            get { return _highTile; }
            set
            {
                var appSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                appSettings.Values["hightile"] = value.ToString();
                SetField<int>(ref _highTile, value, "HighTile");
                StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                logger.Log($"HighTile:{_highTile}-ETD:{GazeInput.IsDeviceAvailable}");
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

        private TimeSpan _slideDwellSpeed = TimeSpan.FromMilliseconds(100);
        public TimeSpan SlideDwellSpeed
        {
            get { return _slideDwellSpeed; }
            set
            {
                SetField<TimeSpan>(ref _slideDwellSpeed, value, "SlideDwellSpeed");
            }
        }

        public enum BoardSpeed
        {
            Slow,
            Fast
        }

        public void SetBoardSpeed(BoardSpeed speed)
        {
            _boardSpeed = speed;

            switch (_boardSpeed)
            {
                case BoardSpeed.Slow:
                    _SlideSpeed = 200;
                    _AddSpeed = 500;
                    _SpawnSpeed = 25;
                    break;

                case BoardSpeed.Fast:
                    _SlideSpeed = 100;
                    _AddSpeed = 100;
                    _SpawnSpeed = 25;

                    break;

                default:
                    _SlideSpeed = 200;
                    _AddSpeed = 500;
                    _SpawnSpeed = 25;
                    break;
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

            string storedHighTile = appSettings.Values["hightile"] as string;
            if (storedHighTile != null)
            {
                HighTile = int.Parse(storedHighTile);
            }
            else
            {
                HighTile = 0;
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

                scaleAnimation.DampingRatio = 0.6f;
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

                    CubicBezierEasingFunction easing = compositor.CreateCubicBezierEasingFunction(new Vector2(.86f, 0.0f), new Vector2(.07f, 1.00f));

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

        private int AddAdjacent(int start, int end, int delta)
        {
            int doubledVal = 0;
            int totalBonus = -1; //If it returns -1 then no additions happened regardless of whether they were bonus worthy
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

                    CubicBezierEasingFunction easing = compositor.CreateCubicBezierEasingFunction(new Vector2(.86f, 0.0f), new Vector2(.07f, 1.00f));

                    ///Scale the ToCell to breifly be twice the size and then back down to regular size
                    ///
                    slideToCellVisual.CenterPoint = new System.Numerics.Vector3(slideToCellVisual.Size.X / 2, slideToCellVisual.Size.Y / 2, 0f);
                    var scaleUpAnimation = compositor.CreateVector3KeyFrameAnimation();
                    scaleUpAnimation.InsertKeyFrame(0f, new System.Numerics.Vector3(1.0f, 1.0f, 0f), easing);
                    scaleUpAnimation.InsertKeyFrame(0.5f, new System.Numerics.Vector3(1.2f, 1.2f, 0f), easing);
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
                    if (_boardSpeed == BoardSpeed.Slow)
                    {
                        answerTextVisual.StartAnimation(nameof(answerTextVisual.Opacity), showAnswerAnimation);
                        answerTextVisual.StartAnimation(nameof(answerTextVisual.Scale), scaleAnswerAnimation);
                        cellTextVisual.StartAnimation(nameof(cellTextVisual.Opacity), showCellTextAnimation);
                    }
                    _addAdjacentBatchAnimation.Resume();
                    _slideBatchAnimation.Resume();

                    Canvas.SetZIndex(Buttons[cur], GetHighestButtonIndex() + 1);

                    if (_boardSpeed == BoardSpeed.Slow)
                    {
                        Cells[cur].AnswerString = Cells[cur].IntVal.ToString() + " + " + Cells[cur + delta].IntVal.ToString();
                    }

                    Cells[cur].IntVal += Cells[cur + delta].IntVal;

                    doubledVal = Cells[cur].IntVal;
                    if (totalBonus > -1)
                    {
                        totalBonus += MilestoneBonus(doubledVal);
                    }
                    else
                    {
                        totalBonus = 0;
                        totalBonus += MilestoneBonus(doubledVal);
                    }

                    if (doubledVal > HighTile)
                    {
                        HighTile = doubledVal;
                    }

                    Cells[cur + delta].IntVal = 0;
                    Score += Cells[cur].IntVal;
                }
                cur += delta;
            }
            return totalBonus;
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
            int totalRotationFactor = 0;
            int bonusRotations = -1;

            var compositor = ElementCompositionPreview.GetElementVisual(Window.Current.Content).Compositor;
            _addAdjacentBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _addAdjacentBatchAnimation.Completed += AddAdjacentBatchAnimation_Completed;
            _addAdjacentBatchAnimation.Comment = start + "," + end + "," + delta + "," + increment;

            for (int i = 0; i < _boardSize; i++)
            {
                bonusRotations = AddAdjacent(start, end, delta);
                if (bonusRotations > 0)
                {
                    totalRotationFactor += bonusRotations;
                }
                change = (bonusRotations > -1) || change;
                start += increment;
                end += increment;
            }

            _addAdjacentBatchAnimation.Comment = _addAdjacentBatchAnimation.Comment + "," + change;
            _addAdjacentBatchAnimation.End();

            MilestoneReward(totalRotationFactor);

            if (change)
            {
                OnPropertyChanged("Score");
            }
        }

        private void MilestoneReward(int totalRotationFactor)
        {
            //Milestone reward             
            if (totalRotationFactor > 0)
            {
                var compositor = ElementCompositionPreview.GetElementVisual(Window.Current.Content).Compositor;

                //Do something interesting             
                var frame = (Frame)Window.Current.Content;
                var mainPage = (MainPage)frame.Content;
                var boardVisual = ElementCompositionPreview.GetElementVisual(mainPage.GameBoardGrid);

                CubicBezierEasingFunction cubicEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(.86f, 0.0f), new Vector2(.07f, 1.00f));

                ScalarKeyFrameAnimation spinAnimation = compositor.CreateScalarKeyFrameAnimation();
                spinAnimation.InsertKeyFrame(0.000001f, 0);
                spinAnimation.InsertKeyFrame(0.999999f, 360 * totalRotationFactor, cubicEasing);
                spinAnimation.InsertKeyFrame(1f, 0);
                spinAnimation.Duration = TimeSpan.FromMilliseconds(3000);
                spinAnimation.IterationBehavior = AnimationIterationBehavior.Count;
                spinAnimation.IterationCount = 1;
                boardVisual.CenterPoint = new Vector3((float)(0.5 * mainPage.GameBoardGrid.ActualWidth), (float)(0.5f * mainPage.GameBoardGrid.ActualHeight), 0f);
                boardVisual.RotationAxis = new Vector3(0.0f, 0.0f, 1f);

                boardVisual.StartAnimation(nameof(boardVisual.RotationAngleInDegrees), spinAnimation);
            }
        }


        private int MilestoneBonus(int doubledValue)
        {
            int bonus = 0;
            int milestone = 2048;  //Give one bonus spin for any adjacent doubling of tiles with this value or exponential spins for higher values

            if (doubledValue >= milestone && doubledValue % milestone == 0)
            {
                bonus = doubledValue / milestone;
            }
            return bonus;
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
        static bool firstLaunch = true;

        public Board Board;

        private SolidColorBrush _solidTileForegroundBrush;
        private SolidColorBrush _solidTileBrush;

        private enum WebViewOpenedAs
        {
            Privacy,
            UseTerms
        }

        private WebViewOpenedAs _webViewOpenedAs;

        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            VersionTextBlock.Text = "Version " + GetAppVersion();

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

            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log($"NewGame-ETD:{GazeInput.IsDeviceAvailable}");
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

                case VirtualKey.F1:
                    Board.SetBoardSpeed(Board.BoardSpeed.Slow);
                    break;

                case VirtualKey.F2:
                    Board.SetBoardSpeed(Board.BoardSpeed.Fast);
                    break;

                case VirtualKey.F3:
                    int newSpeed = Board.SlideDwellSpeed.Milliseconds - 100;
                    if (newSpeed < 100) newSpeed = 100;
                    Board.SlideDwellSpeed = TimeSpan.FromMilliseconds(newSpeed);
                    break;

                case VirtualKey.F4:
                    Board.SlideDwellSpeed = TimeSpan.FromMilliseconds(800);
                    break;

                case VirtualKey.F5:
                    Board.SlideDwellSpeed = TimeSpan.FromMilliseconds(Board.SlideDwellSpeed.Milliseconds + 100);
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

        private ScrollViewer getRootScrollViewer()
        {
            DependencyObject el = this;
            while (el != null && !(el is ScrollViewer))
            {
                el = VisualTreeHelper.GetParent(el);
            }

            return (ScrollViewer)el;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            getRootScrollViewer().Focus(FocusState.Programmatic);

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
            _solidTileBrush = (SolidColorBrush)this.Resources["TileBackground"];

            GazeInput.DwellFeedbackProgressBrush = _solidTileForegroundBrush;
            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);

            if (firstLaunch)
            {
                StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                logger.Log($"Init-ETD:{GazeInput.IsDeviceAvailable}");
                firstLaunch = false;
            }
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void OnHowToPlayButton(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;

            HelpScreen1.Visibility = Visibility.Visible;
            HelpScreen2.Visibility = Visibility.Collapsed;
            HelpScreen3.Visibility = Visibility.Collapsed;
            HelpScreen4.Visibility = Visibility.Collapsed;
            HelpScreen5.Visibility = Visibility.Collapsed;
            HelpScreen6.Visibility = Visibility.Collapsed;
            HelpNavLeftButton.IsEnabled = false;
            HelpNavRightButton.IsEnabled = true;

            HelpDialogGrid.Visibility = Visibility.Visible;
            SetTabsForDialogView();
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            BackToGameButton.Focus(FocusState.Programmatic);
        }

        private void GameBoardGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Double narrowEdge = GameBoardGrid.ActualHeight;
            if (GameBoardGrid.ActualWidth < narrowEdge)
            {
                narrowEdge = GameBoardGrid.ActualWidth;
            }
            Board.CellSpace = narrowEdge;
        }

        private void OnHelpNavRight(object sender, RoutedEventArgs e)
        {
            if (HelpScreen1.Visibility == Visibility.Visible)
            {
                HelpScreen1.Visibility = Visibility.Collapsed;
                HelpScreen2.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = true;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if (HelpScreen2.Visibility == Visibility.Visible)
            {
                HelpScreen2.Visibility = Visibility.Collapsed;
                HelpScreen3.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = true;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if (HelpScreen3.Visibility == Visibility.Visible)
            {
                HelpScreen3.Visibility = Visibility.Collapsed;
                HelpScreen4.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = true;
                HelpNavLeftButton.IsEnabled = true;
            }
            else if (HelpScreen4.Visibility == Visibility.Visible)
            {
                HelpScreen4.Visibility = Visibility.Collapsed;
                HelpScreen5.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = true;
                HelpNavLeftButton.IsEnabled = true;                
            }
            else if (HelpScreen5.Visibility == Visibility.Visible)
            {
                HelpScreen5.Visibility = Visibility.Collapsed;
                HelpScreen6.Visibility = Visibility.Visible;
                HelpNavRightButton.IsEnabled = false;
                HelpNavLeftButton.IsEnabled = true;
           }
            else if (HelpScreen6.Visibility == Visibility.Visible)
            {
                HelpNavRightButton.IsEnabled = false;
                HelpNavLeftButton.IsEnabled = true;
            }
            FixHelp();
        }

        private void OnHelpNavLeft(object sender, RoutedEventArgs e)
        {
            if (HelpScreen1.Visibility == Visibility.Visible)
            {
                HelpNavLeftButton.IsEnabled = false;
                HelpNavRightButton.IsEnabled = true;
            }
            else if (HelpScreen2.Visibility == Visibility.Visible)
            {
                HelpScreen2.Visibility = Visibility.Collapsed;
                HelpScreen1.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = false;
                HelpNavRightButton.IsEnabled = true;
            }
            else if (HelpScreen3.Visibility == Visibility.Visible)
            {
                HelpScreen3.Visibility = Visibility.Collapsed;
                HelpScreen2.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = true;
                HelpNavRightButton.IsEnabled = true;
            }

            else if (HelpScreen4.Visibility == Visibility.Visible)
            {
                HelpScreen4.Visibility = Visibility.Collapsed;
                HelpScreen3.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = true;
                HelpNavRightButton.IsEnabled = true;
            }

            else if (HelpScreen5.Visibility == Visibility.Visible)
            {
                HelpScreen5.Visibility = Visibility.Collapsed;
                HelpScreen4.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = true;
                HelpNavRightButton.IsEnabled = true;
            }
            else if (HelpScreen6.Visibility == Visibility.Visible)
            {
                HelpScreen6.Visibility = Visibility.Collapsed;
                HelpScreen5.Visibility = Visibility.Visible;
                HelpNavLeftButton.IsEnabled = true;
                HelpNavRightButton.IsEnabled = true;
            }
            FixHelp();
        }

        private void DismissButton(object sender, RoutedEventArgs e)
        {
            HelpDialogGrid.Visibility = Visibility.Collapsed;
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.White);
            SetTabsForPageView();
            LogHowToPlayClosed();
        }

        private void LogHowToPlayClosed()
        {
            int currentPage = 0;

            if (HelpScreen1.Visibility == Visibility.Visible)
            {
                currentPage = 1;
            }
            else if (HelpScreen2.Visibility == Visibility.Visible)
            {
                currentPage = 2;
            }
            else if (HelpScreen3.Visibility == Visibility.Visible)
            {
                currentPage = 3;
            }
            else if (HelpScreen4.Visibility == Visibility.Visible)
            {
                currentPage = 4;
            }
            else if (HelpScreen5.Visibility == Visibility.Visible)
            {
                currentPage = 5;
            }
            else if (HelpScreen6.Visibility == Visibility.Visible)
            {
                currentPage = 6;
            }

            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log($"HTP-Pg{currentPage}-ETD:{GazeInput.IsDeviceAvailable}");
        }

        private async void PrivacyViewScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            await PrivacyWebView.InvokeScriptAsync("eval", new string[] { "window.scrollBy(0,-" + PrivacyWebView.ActualHeight / 2 + ") " });
        }

        private async void PrivacyViewScrollDownButton_Click(object sender, RoutedEventArgs e)
        {
            await PrivacyWebView.InvokeScriptAsync("eval", new string[] { "window.scrollBy(0," + PrivacyWebView.ActualHeight / 2 + ") " });
        }

        private void PrivacyViewContinueButton_Click(object sender, RoutedEventArgs e)
        {
            SetTabsForHelpWithClosedWebView();
            PrivacyViewGrid.Visibility = Visibility.Collapsed;
            if (_webViewOpenedAs == WebViewOpenedAs.Privacy)
            {
                PrivacyHyperlink.Focus(FocusState.Programmatic);
            }
            else
            {
                UseTermsHyperlink.Focus(FocusState.Programmatic);
            }
        }

        private void PrivacyHyperlink_Click(object sender, RoutedEventArgs e)
        {
            _webViewOpenedAs = WebViewOpenedAs.Privacy;
            SetTabsForHelpWebView();
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.Transparent);
            WebViewLoadingText.Visibility = Visibility.Visible;
            PrivacyWebView.Navigate(new System.Uri("https://go.microsoft.com/fwlink/?LinkId=521839"));
            PrivacyViewGrid.Visibility = Visibility.Visible;
        }

        private void UseTermsHyperlink_Click(object sender, RoutedEventArgs e)
        {
            _webViewOpenedAs = WebViewOpenedAs.UseTerms;
            SetTabsForHelpWebView();
            GazeInput.DwellFeedbackProgressBrush = new SolidColorBrush(Colors.Transparent);
            WebViewLoadingText.Visibility = Visibility.Visible;
            PrivacyWebView.Navigate(new System.Uri("https://www.microsoft.com/en-us/servicesagreement/default.aspx"));
            PrivacyViewGrid.Visibility = Visibility.Visible;
        }

        private void PrivacyWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;
            WebViewLoadingText.Visibility = Visibility.Collapsed;
        }

        private void HelpPaneGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FixHelp();
        }

        private void FixHelp()
        {
            double helpHeaderSize = 150;
            double helpBottomSize = 140;

            double helpTextBodySize = HelpPaneGrid.ActualHeight - helpHeaderSize - helpBottomSize;

            HelpPaneGrid.UpdateLayout();

            //Check Help 1
            FixHelpTextSize(Help1TextBody, helpTextBodySize);

            //Check Help 2
            FixHelpTextSize(Help2TextBody, helpTextBodySize);

            //Check Help 3
            FixHelpTextSize(Help3TextBody, helpTextBodySize);

            //Check Help 4            
            FixHelpTextSize(Help4TextBody, helpTextBodySize);

            //Check Help 5
            double buttonItem1Size = 50;
            double buttonItem2Size = 50;
            helpTextBodySize = helpTextBodySize - buttonItem1Size - buttonItem2Size;
            FixHelpTextSize(Help5TextBody, helpTextBodySize);

            HelpPaneGrid.UpdateLayout();            
        }

        private void FixHelpTextSize(TextBlock helpTextBody, double helpTextBodySize)
        {
            double defaultFontSize = (double)this.Resources["HelpTextFontSize"];
            double fitFontSize = defaultFontSize;            

            var tb = new TextBlock();
            tb.FontFamily = helpTextBody.FontFamily;
            tb.FontSize = fitFontSize;
            tb.TextWrapping = helpTextBody.TextWrapping;
            tb.Text = helpTextBody.Text;
            tb.Measure(new Size(helpTextBody.ActualWidth, Double.PositiveInfinity));

            if (tb.DesiredSize.Height < helpTextBodySize)
            {
                helpTextBody.FontSize = fitFontSize;
            }
            else
            {
                if (helpTextBody.ActualHeight > 1)
                {
                    do
                    {
                        fitFontSize -= 1;
                        tb.FontSize = fitFontSize;

                        tb.Measure(new Size(helpTextBody.ActualWidth, Double.PositiveInfinity));

                    } while (tb.DesiredSize.Height > helpTextBodySize && fitFontSize > 8);
                }
            }
            helpTextBody.FontSize = fitFontSize;
        }

        private void HelpDialogGrid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                DismissButton(null, null);
            }
        }

        private void SetTabsForDialogView()
        {
            HowToPlayButton.IsTabStop = false;
            ExitButton.IsTabStop = false;
            NewGameButton.IsTabStop = false;
            UpButton.IsTabStop = false;
            DownButton.IsTabStop = false;
            LeftButton.IsTabStop = false;
            RightButton.IsTabStop = false;
        }

        private void SetTabsForPageView()
        {
            HowToPlayButton.IsTabStop = true;
            ExitButton.IsTabStop = true;
            NewGameButton.IsTabStop = true;
            UpButton.IsTabStop = true;
            DownButton.IsTabStop = true;
            LeftButton.IsTabStop = true;
            RightButton.IsTabStop = true;
        }

        private void SetTabsForHelpWebView()
        {
            HelpNavRightButton.IsTabStop = false;
            HelpNavLeftButton.IsTabStop = false;
            BackToGameButton.IsTabStop = false;
            PrivacyHyperlink.IsTabStop = false;
            UseTermsHyperlink.IsTabStop = false;
        }

        private void SetTabsForHelpWithClosedWebView()
        {
            HelpNavRightButton.IsTabStop = true;
            HelpNavLeftButton.IsTabStop = true;
            BackToGameButton.IsTabStop = true;
            PrivacyHyperlink.IsTabStop = true;
            UseTermsHyperlink.IsTabStop = true;
        }      
    }
}
