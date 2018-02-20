using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using Microsoft.Research.Input.Gaze;
using System.Diagnostics;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeeSaw
{
    public sealed class ImageData
    {
        public StorageFile File;
        public List<GazeEventArgs> GazeEvents;
        public bool TrackGaze;
        public bool ShowPoints;
        public bool ShowTracks;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaPage : Page
    {
        TrackViewer _trackViewer;
        ImageData _imageData;
        Rect _pictureRect;

        public MediaPage()
        {
            this.InitializeComponent();
            _trackViewer = new TrackViewer();
            Loaded += TrackingPage_Loaded;
        }

        private void TrackingPage_Loaded(object sender, RoutedEventArgs e)
        {
            Picture.ImageOpened += Picture_ImageOpened;
        }

        private void Picture_ImageOpened(object sender, RoutedEventArgs e)
        {
            var pos = Picture.TransformToVisual(this).TransformPoint(new Point(0, 0));
            _pictureRect = new Rect(pos.X, pos.Y, Picture.ActualWidth, Picture.ActualHeight);
        }

        private void OnGazeInput(GazePointer sender, GazeEventArgs ea)
        {
            if ((_imageData.TrackGaze) /*&& (_pictureRect.Contains(ea.Location)) */)
            {
                _imageData.GazeEvents.Add(ea);
                _trackViewer.AddEvent(ea);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _imageData = e.Parameter as ImageData;

            if (_imageData == null)
            {
                return;
            }

            foreach (var ev in _imageData.GazeEvents)
            {
                _trackViewer.AddEvent(ev);
            }
            
            _trackViewer.ShowPoints = _imageData.ShowPoints;
            _trackViewer.ShowTracks = _imageData.ShowTracks;

            if (_imageData.File != null)
            {
                ViewFile(_imageData.File);
            }

        }

        async void ViewFile(StorageFile file)
        {
            _pictureRect = new Rect(0, 0, 0, 0);

            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap.
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                Picture.Source = bitmapImage;
            }
        }

        private void Picture_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _trackViewer.Reset();
            Frame.Navigate(typeof(MainPage), _imageData);
        }
    }
}