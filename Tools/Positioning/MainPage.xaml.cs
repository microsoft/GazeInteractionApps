//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using Microsoft.Toolkit.Uwp.Input.GazeInteraction.GazeHidParsers;
using System.Drawing;
using System.Text;
using Windows.Devices.Input.Preview;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Positioning
{
    public sealed partial class MainPage : Page
    {
        private GazeInputSourcePreview gazeInputSourcePreview;
        private GazeHidPositionsParser gazeHidPositionsParser;

        private DisplayInformation displayInformation;
        Size screenSize;
        float screenSizeInchesWidth;
        float screenSizeInchesHeight;

        float screenSizeMicrometersWidth;
        float screenSizeMicrometersHeight;

        bool showGaze = true;

        public MainPage()
        {
            InitializeComponent();

            gazeInputSourcePreview      = GazeInputSourcePreview.GetForCurrentView();
            gazeInputSourcePreview.GazeMoved += GazeInputSourcePreview_GazeMoved;

            displayInformation          = DisplayInformation.GetForCurrentView();
            screenSize                  = new Size((int)displayInformation.ScreenWidthInRawPixels,
                                                   (int)displayInformation.ScreenHeightInRawPixels);
            screenSizeInchesWidth       = screenSize.Width / displayInformation.RawDpiX;
            screenSizeInchesHeight      = screenSize.Height / displayInformation.RawDpiY;

            screenSizeMicrometersWidth  = screenSizeInchesWidth * 25400;
            screenSizeMicrometersHeight = screenSizeInchesHeight * 25400;
        }

        private void GazeInputSourcePreview_GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            var sb = new StringBuilder();

            sb.Append("      GazePos (");
            if (args.CurrentPoint.EyeGazePosition != null)
            {
                Canvas.SetLeft(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.X);
                Canvas.SetTop(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.Y);

                sb.Append($"{args.CurrentPoint.EyeGazePosition.Value.X,6}px, {args.CurrentPoint.EyeGazePosition.Value.Y,6}px");

                if (showGaze)
                {
                    GazePositionEllipse.Visibility = Visibility.Visible;
                }
                else
                {
                    GazePositionEllipse.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                GazePositionEllipse.Visibility = Visibility.Collapsed;
            }
            sb.AppendLine(")");

            if (args.CurrentPoint.HidInputReport != null)
            {
                var hidReport = args.CurrentPoint.HidInputReport;
                var sourceDevice = args.CurrentPoint.SourceDevice;

                if (gazeHidPositionsParser == null)
                {
                    gazeHidPositionsParser = new GazeHidPositionsParser(sourceDevice);
                }

                var positions = gazeHidPositionsParser.GetGazeHidPositions(hidReport);

                // The return values for the left and right eye are in micrometers by default
                // They are references such that X and Y origin are the top left hand corner
                // of the calibrated display. The Z origin is the centerpoint of the display
                // (not the tracker). As such, there is a minor difference between the actual
                // sensor-to-eye distance vs the reported distance for left/right eye position.

                UpdateEyeData("Left", positions.LeftEyePosition, LeftEyePositionEllipse, sb);
                UpdateEyeData("Right", positions.RightEyePosition, RightEyePositionEllipse, sb);

                if (positions.RightEyePosition != null && positions.LeftEyePosition != null)
                {
                    // calculate IPD in mm
                    var interPupilaryDistance = (positions.RightEyePosition.X - positions.LeftEyePosition.X) / 1000.0;

                    sb.AppendLine($"          IPD ({interPupilaryDistance,6:F2}mm)");
                }

                if (positions.HeadPosition != null)
                {
                    sb.AppendLine($"HeadPosition ({positions.HeadPosition.X,8:F2}, {positions.HeadPosition.Y,8:F2}, {positions.HeadPosition.Z,8:F2})");
                }

                if (positions.HeadRotation != null)
                {
                    sb.AppendLine($"HeadRotation ({positions.HeadRotation.X,8:F2}, {positions.HeadRotation.Y,8:F2}, {positions.HeadRotation.Z,8:F2})");
                }
            }

            StatusTextBlock.Text = sb.ToString();
        }

        private void UpdateEyeData(string eyeName, GazeHidPosition eyePosition, Windows.UI.Xaml.Shapes.Ellipse eyeEllipse, StringBuilder sb)
        {
            sb.Append($"{eyeName,7}EyePos (");
            if (eyePosition != null)
            {
                sb.Append($"{(eyePosition.X / 1000.0),6:F1}mm, {(eyePosition.Y / 1000.0),6:F1}mm, {(eyePosition.Z / 1000.0),6:F1}mm)");

                if (eyePosition.X >= 0 &&
                    eyePosition.X <= screenSizeMicrometersWidth &&
                    eyePosition.Y >= 0 &&
                    eyePosition.Y <= screenSizeMicrometersHeight)
                {
                    var newX = MapRange(0, screenSizeMicrometersWidth, 0, ActualWidth, eyePosition.X);
                    var newY = MapRange(0, screenSizeMicrometersHeight, 0, ActualHeight, eyePosition.Y);

                    var newZ = string.Empty;
                    if (eyePosition.Z < 400000)
                    {
                        newZ = "Red";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else if (eyePosition.Z < 500000)
                    {
                        newZ = "Yellow";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (eyePosition.Z < 700000)
                    {
                        newZ = "Green";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else if (eyePosition.Z < 800000)
                    {
                        newZ = "Yellow";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else
                    {
                        newZ = "Red";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Red);
                    }

                    sb.Append($" ({newX,6:F0}, {newY,6:F0}, {newZ}");

                    Canvas.SetLeft(eyeEllipse, newX);
                    Canvas.SetTop(eyeEllipse, newY);

                    eyeEllipse.Visibility = Visibility.Visible;
                }
                else
                {
                    eyeEllipse.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                eyeEllipse.Visibility = Visibility.Collapsed;
            }
            sb.AppendLine(")");
        }

        private static double MapRange(double oldStart, double oldEnd, double newStart, double newEnd, double valueToMap)
        {
            double scalingFactor = (newEnd - newStart) / (oldEnd - oldStart);
            return newStart + ((valueToMap - oldStart) * scalingFactor);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var conversionFactorX = (displayInformation.RawDpiX / displayInformation.RawPixelsPerViewPixel) / 25.4;
            var conversionFactorY = (displayInformation.RawDpiY / displayInformation.RawPixelsPerViewPixel) / 25.4;

            D40.Width   = conversionFactorX * 40;
            D40.Height  = conversionFactorY * 40;
        }

        private void DebugInformationCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (DebugInformationCheckbox.IsChecked.HasValue && DebugInformationCheckbox.IsChecked.Value)
            {
                StatusTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                StatusTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void PositioningReticleCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (PositioningReticleCheckbox.IsChecked.HasValue && PositioningReticleCheckbox.IsChecked.Value)
            {
                D40.Visibility = Visibility.Visible;
                D4.Visibility = Visibility.Visible;
            }
            else
            {
                D40.Visibility = Visibility.Collapsed;
                D4.Visibility = Visibility.Collapsed;
            }
        }

        private void EyeGazeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            showGaze = EyeGazeCheckbox.IsChecked.HasValue && EyeGazeCheckbox.IsChecked.Value;
        }
    }
}
