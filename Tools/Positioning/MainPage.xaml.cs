using System.Numerics;
using Windows.Devices.HumanInterfaceDevice;
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
            if (args.CurrentPoint.HidInputReport != null)
            {
                var hidReport = args.CurrentPoint.HidInputReport;
                var sourceDevice = args.CurrentPoint.SourceDevice;

                var leftEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_LeftEyePosition);
                var rightEyePositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_RightEyePosition);
                var headPositionParser = new GazePositionHidParser(sourceDevice, GazeExtendedUsages.Usage_HeadPosition);

                var leftEyePosition = leftEyePositionParser.GetPosition(hidReport);
                var rightEyePosition = rightEyePositionParser.GetPosition(hidReport);
                var headPosition = headPositionParser.GetPosition(hidReport);

                Canvas.SetLeft(HeadPositionEllipse, headPosition.Value.X);
                Canvas.SetTop(HeadPositionEllipse, headPosition.Value.Y);
            }
        }
    }
}
