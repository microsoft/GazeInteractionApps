//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using Windows.Devices.Input.Preview;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static Positioning.GazeHidInputReportHelpers;

namespace Positioning
{
    public sealed partial class MainPage : Page
    {
        private GazeInputSourcePreview gazeInputSourcePreview;

        private DisplayInformation displayInformation;
        Size screenSize;
        float screenSizeInchesWidth;
        float screenSizeInchesHeight;

        float screenSizeMicrometersWidth;
        float screenSizeMicrometersHeight;

        bool showGaze = true;

        public MainPage()
        {
            this.InitializeComponent();

            gazeInputSourcePreview = GazeInputSourcePreview.GetForCurrentView();
            gazeInputSourcePreview.GazeMoved += GazeInputSourcePreview_GazeMoved;

            displayInformation = DisplayInformation.GetForCurrentView();
            screenSize = new Size((int)displayInformation.ScreenWidthInRawPixels,
                                  (int)displayInformation.ScreenHeightInRawPixels);
            screenSizeInchesWidth = screenSize.Width / displayInformation.RawDpiX;
            screenSizeInchesHeight = screenSize.Height / displayInformation.RawDpiY;

            screenSizeMicrometersWidth = screenSizeInchesWidth * 25400;
            screenSizeMicrometersHeight = screenSizeInchesHeight * 25400;
        }

        private void GazeInputSourcePreview_GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            var sb = new StringBuilder();

            sb.Append("GazePos (");
            if (args.CurrentPoint.EyeGazePosition != null)
            {
                Canvas.SetLeft(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.X);
                Canvas.SetTop(GazePositionEllipse, args.CurrentPoint.EyeGazePosition.Value.Y);

                sb.Append($"{args.CurrentPoint.EyeGazePosition.Value.X}, {args.CurrentPoint.EyeGazePosition.Value.Y}");

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

                var leftEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_LeftEyePosition);
                var rightEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_RightEyePosition);

                var leftEyePosition = leftEyePositionParser.GetPosition(hidReport);
                var rightEyePosition = rightEyePositionParser.GetPosition(hidReport);

                // The return values for the left and right eye are in micrometers by default
                // They are references such that X and Y origin are the top left hand corner 
                // of the calibrated display. The Z origin is the centerpoint of the display
                // (not the tracker). As such, there is a minor difference between the actual
                // sensor-to-eye distance vs the reported distance for left/right eye position.

                sb.Append("LeftEyePos  (");
                if (leftEyePosition != null)
                {
                    sb.Append($"{(leftEyePosition.Value.X / 10000.0):F1}cm, {(leftEyePosition.Value.Y / 10000.0):F1}cm, {(leftEyePosition.Value.Z / 10000.0):F1}cm)");

                    if (leftEyePosition.Value.X >= 0 &&
                        leftEyePosition.Value.X <= screenSizeMicrometersWidth &&
                        leftEyePosition.Value.Y >= 0 &&
                        leftEyePosition.Value.Y <= screenSizeMicrometersHeight)
                    {
                        var newX = MapRange(0, screenSizeMicrometersWidth, 0, ActualWidth, leftEyePosition.Value.X);
                        var newY = MapRange(0, screenSizeMicrometersHeight, 0, ActualHeight, leftEyePosition.Value.Y);

                        var newZ = string.Empty;
                        if (leftEyePosition.Value.Z < 400000)
                        {
                            newZ = "Red";
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }
                        else if (leftEyePosition.Value.Z < 500000)
                        {
                            newZ = "Yellow";
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (leftEyePosition.Value.Z < 700000)
                        {
                            newZ = "Green";
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Green);
                        }
                        else if (leftEyePosition.Value.Z < 800000)
                        {
                            newZ = "Yellow";
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            newZ = "Red";
                            LeftEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }

                        sb.Append($" ({newX:F0}, {newY:F0}, {newZ}");

                        Canvas.SetLeft(LeftEyePositionEllipse, newX);
                        Canvas.SetTop(LeftEyePositionEllipse, newY);

                        LeftEyePositionEllipse.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LeftEyePositionEllipse.Visibility = Visibility.Collapsed;
                    }
                }
                sb.AppendLine(")");

                sb.Append("RightEyePos (");
                if (rightEyePosition != null)
                {
                    sb.Append($"{(rightEyePosition.Value.X / 10000.0):F1}cm, {(rightEyePosition.Value.Y / 10000.0):F1}cm, {(rightEyePosition.Value.Z / 10000.0):F1}cm)");

                    if (rightEyePosition.Value.X >= 0 &&
                        rightEyePosition.Value.X <= screenSizeMicrometersWidth &&
                        rightEyePosition.Value.Y >= 0 &&
                        rightEyePosition.Value.Y <= screenSizeMicrometersHeight)
                    {
                        var newX = MapRange(0, screenSizeMicrometersWidth, 0, ActualWidth, rightEyePosition.Value.X);
                        var newY = MapRange(0, screenSizeMicrometersHeight, 0, ActualHeight, rightEyePosition.Value.Y);

                        var newZ = String.Empty;
                        if (rightEyePosition.Value.Z < 400000)
                        {
                            newZ = "Red";
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }
                        else if (rightEyePosition.Value.Z < 500000)
                        {
                            newZ = "Yellow";
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else if (rightEyePosition.Value.Z < 700000)
                        {
                            newZ = "Green";
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Green);
                        }
                        else if (rightEyePosition.Value.Z < 800000)
                        {
                            newZ = "Yellow";
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            newZ = "Red";
                            RightEyePositionEllipse.Fill = new SolidColorBrush(Colors.Red);
                        }

                        sb.Append($" ({newX:F0}, {newY:F0}, {newZ}");

                        Canvas.SetLeft(RightEyePositionEllipse, newX);
                        Canvas.SetTop(RightEyePositionEllipse, newY);

                        RightEyePositionEllipse.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        RightEyePositionEllipse.Visibility = Visibility.Collapsed;
                    }
                }
                sb.AppendLine(")");

                if (rightEyePosition != null && leftEyePosition != null)
                {
                    // calculate IPD in mm
                    var interPupilaryDistance = (rightEyePosition.Value.X - leftEyePosition.Value.X) / 1000.0;

                    sb.AppendLine($"IPD ({interPupilaryDistance.ToString("F2", CultureInfo.InvariantCulture)}mm)");
                }

                var headPostitionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_HeadPosition);
                var headPosition = headPostitionParser.GetPosition(hidReport);
                if (headPosition != null)
                {
                    sb.AppendLine($"HeadPosition ({headPosition.Value.X.ToString("F2", CultureInfo.InvariantCulture)}, {headPosition.Value.Y.ToString("F2", CultureInfo.InvariantCulture)}, {headPosition.Value.Z.ToString("F2", CultureInfo.InvariantCulture)})");
                }

                var headRotationParser = new GazeRotationHidParser(sourceDevice, GazeExtendedUsages.Usage_HeadDirectionPoint);
                var headRotation = headRotationParser.GetRotation(hidReport);

                if (headRotation != null)
                {
                    sb.AppendLine($"HeadRotation ({headRotation.Value.X.ToString("F2", CultureInfo.InvariantCulture)}, {headRotation.Value.Y.ToString("F2", CultureInfo.InvariantCulture)}, {headRotation.Value.Z.ToString("F2", CultureInfo.InvariantCulture)})");
                }
            }

            StatusTextBlock.Text = sb.ToString();
        }

        private static double MapRange(double oldStart, double oldEnd, double newStart, double newEnd, double valueToMap)
        {
            double scalingFactor = (newEnd - newStart) / (oldEnd - oldStart);
            return newStart + ((valueToMap - oldStart) * scalingFactor);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var conversionFactorX = (displayInformation.RawDpiX / displayInformation.RawPixelsPerViewPixel) / 25.4;
            var conversionFactorY = (displayInformation.RawDpiY / displayInformation.RawPixelsPerViewPixel) / 25.4;

            D40.Width   = conversionFactorX * 40;
            D40.Height  = conversionFactorY * 40;
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch(e.Key)
            {
                case Windows.System.VirtualKey.F12:
                    if (StatusTextBlock.Visibility == Visibility.Visible)
                    {
                        StatusTextBlock.Visibility = Visibility.Collapsed;
                    }
                    else // if (StatusTextBlock.Visibility == Visibility.Collapsed)
                    {
                        StatusTextBlock.Visibility = Visibility.Visible;
                    }
                    break;
                case Windows.System.VirtualKey.F11:
                    showGaze = !showGaze;
                    break;
                case Windows.System.VirtualKey.F10:
                    if (D40.Visibility == Visibility.Visible)
                    {
                        D40.Visibility = Visibility.Collapsed;
                        D4.Visibility = Visibility.Collapsed;
                    }
                    else // if (D40.Visibility == Visibility.Collapsed)
                    {
                        D40.Visibility = Visibility.Visible;
                        D4.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }
    }
}
