//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Input.Preview;

using Microsoft.Toolkit.UWP.Input.Gaze;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeeSaw
{
    public sealed class ImageData
    {
        public StorageFile File;
        public List<GazeMovedPreviewEventArgs> GazeEvents;
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

            var gazeInputSource = GazeInputSourcePreview.GetForCurrentView();
            if (gazeInputSource != null)
            {
                gazeInputSource.GazeMoved += OnGazeInput;
            }

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

        private void OnGazeInput(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs ea)
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