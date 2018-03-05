//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

using Microsoft.Research.Input.Gaze;

namespace SeeSaw
{
    class TrackViewer
    {
        const int DEFAULT_ELLIPSE_RADIUS = 5;

        Popup _popup;
        Canvas _canvas;
        List<GazeEventArgs> _gazeEvents;
        List<Ellipse> _ellipses;
        List<Line> _lines;

        SolidColorBrush _ellipseBrush;


        bool _showPoints;
        public bool ShowPoints
        {
            get
            {
                return _showPoints;
            }
            set
            {
                _showPoints = value;
                UpdateTrackViewer();
            }
        }

        bool _showTracks;
        public bool ShowTracks
        {
            get
            {
                return _showTracks;
            }
            set
            {
                _showTracks = value;
                UpdateTrackViewer();
            }
        }
        
        public TrackViewer()
        {
            _popup = new Popup();
            _canvas = new Canvas();
            _gazeEvents = new List<GazeEventArgs>();
            _ellipses = new List<Ellipse>();
            _lines = new List<Line>();

            _ellipseBrush = new SolidColorBrush(Windows.UI.Colors.IndianRed);
            Reset();

            _popup.Child = _canvas;
            _popup.IsOpen = _showPoints || _showTracks;
        }

        public void Reset()
        {
            _gazeEvents.Clear();
            _ellipses.Clear();
            _lines.Clear();
            _popup.IsOpen = false;
        }

        void UpdateTrackViewer()
        {
            _canvas.Children.Clear();
            _popup.IsOpen = _showPoints || _showTracks;

            if (!_showPoints)
            {
                return;
            }

            foreach (var ellipse in _ellipses)
            {
                _canvas.Children.Add(ellipse);
            }

            if (_showTracks)
            {
                foreach (var line in _lines)
                {
                    _canvas.Children.Add(line);
                }
            }
        }

        public void AddEvent(GazeEventArgs ea)
        {
            _gazeEvents.Add(ea);

            var ellipse = new Ellipse();
            ellipse.Fill = _ellipseBrush;
            ellipse.VerticalAlignment = VerticalAlignment.Top;
            ellipse.HorizontalAlignment = HorizontalAlignment.Left;
            ellipse.Width = 2 * DEFAULT_ELLIPSE_RADIUS;
            ellipse.Height = 2 * DEFAULT_ELLIPSE_RADIUS;
            ellipse.Margin = new Thickness(ea.Location.X - DEFAULT_ELLIPSE_RADIUS, ea.Location.Y - DEFAULT_ELLIPSE_RADIUS, 0, 0);

            _ellipses.Add(ellipse);

            var count = _gazeEvents.Count;

            if (count > 2)
            {
                var line = new Line();
                line.X1 = _gazeEvents[count - 2].Location.X;
                line.Y1 = _gazeEvents[count - 2].Location.Y;
                line.X2 = _gazeEvents[count - 1].Location.X;
                line.Y2 = _gazeEvents[count - 1].Location.Y;
                line.Stroke = _ellipseBrush;
                line.StrokeThickness = 2;
                _lines.Add(line);
            }
        }
    }
}
