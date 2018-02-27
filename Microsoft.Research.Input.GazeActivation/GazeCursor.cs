using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Microsoft.Research.Input.Gaze
{
    class GazeCursor
    {
        const int DEFAULT_CURSOR_RADIUS = 5;
        const bool DEFAULT_CURSOR_VISIBILITY = true;

        static GazeCursor cursor = new GazeCursor();

        internal static GazeCursor Instance
        {
            get
            {
                return cursor;
            }
        }

        internal int CursorRadius
        {
            get { return _cursorRadius; }
            set
            {
                _cursorRadius = value;
                if (_gazeCursor != null)
                {
                    _gazeCursor.Width = 2 * _cursorRadius;
                    _gazeCursor.Height = 2 * _cursorRadius;
                }
            }
        }

        internal bool IsCursorVisible
        {
            get { return _isCursorVisible; }
            set
            {
                _isCursorVisible = value;
                if (_gazePopup != null)
                {
                    _gazePopup.IsOpen = _isCursorVisible;
                }
            }
        }

        internal Point Position
        {
            get
            {
                return _cursorPosition;
            }
            set
            {
                _cursorPosition = value;
                _gazeCursor.Margin = new Thickness(value.X, value.Y, 0, 0);
            }
        }

        Point PositionOriginal
        {
            get
            {
                return _originalCursorPosition;
            }

            set
            {
                _originalCursorPosition = value;
                _origSignalCursor.Margin = new Thickness(value.X, value.Y, 0, 0);
            }
        }

        GazeCursor()
        {
            _cursorRadius = DEFAULT_CURSOR_RADIUS;
            _isCursorVisible = DEFAULT_CURSOR_VISIBILITY;

            _gazePopup = new Popup();
            _gazeCanvas = new Canvas();

            _gazeCursor = new Ellipse();
            _gazeCursor.Fill = new SolidColorBrush(Colors.IndianRed);
            _gazeCursor.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            _gazeCursor.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            _gazeCursor.Width = 2 * CursorRadius;
            _gazeCursor.Height = 2 * CursorRadius;

            _origSignalCursor = new Ellipse();
            _origSignalCursor.Fill = new SolidColorBrush(Colors.Green);
            _origSignalCursor.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            _origSignalCursor.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            _origSignalCursor.Width = 2 * CursorRadius;
            _origSignalCursor.Height = 2 * CursorRadius;

            _gazeRect = new Rectangle();
            _gazeRect.IsHitTestVisible = false;

            _gazeCanvas.Children.Add(_gazeCursor);
            _gazeCanvas.Children.Add(_gazeRect);

            // TODO: Reenable this once GazeCursor is refactored correctly
            //_gazeCanvas.Children.Append(_origSignalCursor);

            _gazePopup.Child = _gazeCanvas;
            _gazePopup.IsOpen = IsCursorVisible;
        }

        Popup _gazePopup;
        Canvas _gazeCanvas;
        Ellipse _gazeCursor;
        Ellipse _origSignalCursor;
        Rectangle _gazeRect;
        Point _cursorPosition;
        Point _originalCursorPosition;
        int _cursorRadius = DEFAULT_CURSOR_RADIUS;
        bool _isCursorVisible = DEFAULT_CURSOR_VISIBILITY;
    }
}
