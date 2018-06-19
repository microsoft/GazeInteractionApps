//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System.Collections.ObjectModel;
using Windows.Devices.Input.Preview;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DrawnOut
{
    public class PointPair
    {
        public Point P1 { get; set; }

        public Point P2 { get; set; }

        public PointPair(Point p1, Point p2)
        {
            P1 = p1;
            P2 = p2;
        }
    }

    public sealed partial class MainPage : Page
    {
        private GazeInputSourcePreview gazeInputSourcePreview;
        private Frame rootFrame;
        private Point? oldPoint = null;

        public ObservableCollection<PointPair> GazeHistory { get; set; } = new ObservableCollection<PointPair>();

        public int MaxGazeHistorySize { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            MaxGazeHistorySize = 100000;

            rootFrame = Window.Current.Content as Frame;

            gazeInputSourcePreview = GazeInputSourcePreview.GetForCurrentView();
            gazeInputSourcePreview.GazeMoved += GazeInputSourcePreview_GazeMoved;

            GazeHistoryItemsControl.ItemsSource = GazeHistory;
        }
        
        private void GazeInputSourcePreview_GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            UpdateGazeHistory(args.CurrentPoint);
        }

        private void UpdateGazeHistory(GazePointPreview pt)
        {
            if (pt.EyeGazePosition.HasValue)
            {
                var transform = rootFrame.TransformToVisual(this);
                var newPoint = transform.TransformPoint(pt.EyeGazePosition.Value);

                if (oldPoint.HasValue)
                {
                    GazeHistory.Add(new PointPair(oldPoint.Value, newPoint));

                    if (GazeHistory.Count > MaxGazeHistorySize)
                    {
                        GazeHistory.RemoveAt(0);
                    }
                }

                oldPoint = newPoint;
            }
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                App.Current.Exit();
            }
            else if (e.Key == Windows.System.VirtualKey.Delete ||
                e.Key == Windows.System.VirtualKey.Back)
            {
                GazeHistory.Clear();
            }
        }
    }
}
