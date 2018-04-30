using System;
using System.Diagnostics;
using Windows.Devices.Input.Preview;
using Windows.UI.Xaml.Controls;
using static Positioning.GazeHidInputReportHelpers;

namespace Positioning
{
    public sealed partial class MainPage : Page
    {
        private GazeInputSourcePreview gazeInputSourcePreview;
        const int GazeHidPage = 0x12;

        public MainPage()
        {
            this.InitializeComponent();

            gazeInputSourcePreview = GazeInputSourcePreview.GetForCurrentView();
            gazeInputSourcePreview.GazeMoved += GazeInputSourcePreview_GazeMoved;
        }

        private void GazeInputSourcePreview_GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            if (args.CurrentPoint.EyeGazePosition != null)
            {
                Canvas.SetLeft(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.X);
                Canvas.SetTop(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.Y);
            }

            if (args.CurrentPoint.HidInputReport != null)
            {
                var hidReport = args.CurrentPoint.HidInputReport;
                var sourceDevice = args.CurrentPoint.SourceDevice;

                var leftEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_LeftEyePosition);
                var rightEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_RightEyePosition);

                var leftEyePosition = leftEyePositionParser.GetPosition(hidReport);
                var rightEyePosition = rightEyePositionParser.GetPosition(hidReport);

                if (leftEyePosition != null)
                {
                    Debug.WriteLine($"LeftEye  {leftEyePosition.Value.X}, {leftEyePosition.Value.Y}, {leftEyePosition.Value.Z}");

                    var newX = MapRange(150000, 500000, 0, ActualWidth, leftEyePosition.Value.X);
                    var newY = MapRange(0, 100000, 0, ActualHeight, leftEyePosition.Value.Y);
                    var newZ = MapRange(500000, 1300000, 0, 100, leftEyePosition.Value.Z);

                    Debug.WriteLine($"LeftEye* {newX}, {newY}, {newZ}");

                    Canvas.SetLeft(LeftEyePositionEllipse, newX);
                    Canvas.SetTop(LeftEyePositionEllipse, newY);

                }

                if (rightEyePosition != null)
                {
                    Debug.WriteLine($"RightEye {rightEyePosition.Value.X}, {rightEyePosition.Value.Y}, {rightEyePosition.Value.Z}");

                    var newX = MapRange(150000, 500000, 0, ActualWidth, rightEyePosition.Value.X);
                    var newY = MapRange(0, 100000, 0, ActualHeight, rightEyePosition.Value.Y);
                    var newZ = MapRange(500000, 1300000, 0, 100, rightEyePosition.Value.Z);

                    Debug.WriteLine($"RightEye* {newX}, {newY}, {newZ}");

                    Canvas.SetLeft(RightEyePositionEllipse, newX);
                    Canvas.SetTop(RightEyePositionEllipse, newY);
                }
            }
        }

        private static double MapRange(double oldStart, double oldEnd, double newStart, double newEnd, double valueToMap)
        {
            double scalingFactor = (double)(newEnd - newStart) / (oldEnd - oldStart);
            return (double)(newStart + ((valueToMap - oldStart) * scalingFactor));
        }
    }
}
