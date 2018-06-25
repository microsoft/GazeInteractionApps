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
            InitializeComponent();

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

                var leftEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_LeftEyePosition);
                var rightEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_RightEyePosition);

                var leftEyePosition = leftEyePositionParser.GetPosition(hidReport);
                var rightEyePosition = rightEyePositionParser.GetPosition(hidReport);

                // The return values for the left and right eye are in micrometers by default
                // They are references such that X and Y origin are the top left hand corner 
                // of the calibrated display. The Z origin is the centerpoint of the display
                // (not the tracker). As such, there is a minor difference between the actual
                // sensor-to-eye distance vs the reported distance for left/right eye position.

                UpdateEyeData("Left", leftEyePosition, LeftEyePositionEllipse, sb);
                UpdateEyeData("Right", rightEyePosition, RightEyePositionEllipse, sb);

                if (rightEyePosition != null && leftEyePosition != null)
                {
                    // calculate IPD in mm
                    var interPupilaryDistance = (rightEyePosition.Value.X - leftEyePosition.Value.X) / 1000.0;

                    sb.AppendLine($"          IPD ({interPupilaryDistance,6:F2}mm)");
                }

                var headPostitionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_HeadPosition);
                var headPosition = headPostitionParser.GetPosition(hidReport);
                if (headPosition != null)
                {
                    sb.AppendLine($"HeadPosition ({headPosition.Value.X,8:F2}, {headPosition.Value.Y,8:F2}, {headPosition.Value.Z,8:F2})");
                }

                var headRotationParser = new GazeRotationHidParser(sourceDevice, GazeExtendedUsages.Usage_HeadDirectionPoint);
                var headRotation = headRotationParser.GetRotation(hidReport);

                if (headRotation != null)
                {
                    sb.AppendLine($"HeadRotation ({headRotation.Value.X,8:F2}, {headRotation.Value.Y,8:F2}, {headRotation.Value.Z,8:F2})");
                }
            }

            StatusTextBlock.Text = sb.ToString();
        }

        private void UpdateEyeData(string eyeName, System.Numerics.Vector3? eyePosition, Windows.UI.Xaml.Shapes.Ellipse eyeEllipse, StringBuilder sb)
        {
            sb.Append($"{eyeName,7}EyePos (");
            if (eyePosition != null)
            {
                sb.Append($"{(eyePosition.Value.X / 1000.0),6:F1}mm, {(eyePosition.Value.Y / 1000.0),6:F1}mm, {(eyePosition.Value.Z / 1000.0),6:F1}mm)");

                if (eyePosition.Value.X >= 0 &&
                    eyePosition.Value.X <= screenSizeMicrometersWidth &&
                    eyePosition.Value.Y >= 0 &&
                    eyePosition.Value.Y <= screenSizeMicrometersHeight)
                {
                    var newX = MapRange(0, screenSizeMicrometersWidth, 0, ActualWidth, eyePosition.Value.X);
                    var newY = MapRange(0, screenSizeMicrometersHeight, 0, ActualHeight, eyePosition.Value.Y);

                    var newZ = string.Empty;
                    if (eyePosition.Value.Z < 400000)
                    {
                        newZ = "Red";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else if (eyePosition.Value.Z < 500000)
                    {
                        newZ = "Yellow";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (eyePosition.Value.Z < 700000)
                    {
                        newZ = "Green";
                        eyeEllipse.Fill = new SolidColorBrush(Colors.Green);
                    }
                    else if (eyePosition.Value.Z < 800000)
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

                    eyeEllipse.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    eyeEllipse.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
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
            App.Current.Exit();
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
