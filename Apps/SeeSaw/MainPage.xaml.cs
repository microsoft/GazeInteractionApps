//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Research.Input.Gaze;

namespace SeeSaw
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ImageData _imageData;
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async void SelectMedia_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                _imageData = new ImageData();
                _imageData.File = file;
                _imageData.GazeEvents = new List<GazeEventArgs>();
                SelectedMedia.Text = file.Path;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _imageData = e.Parameter as ImageData;
            if ((_imageData != null) && (_imageData.File != null))
            {
                SelectedMedia.Text = _imageData.File.Path;
            }
        }
        private void ViewMedia_Click(object sender, RoutedEventArgs e)
        {
            _imageData.TrackGaze = true;
            _imageData.ShowPoints = false;
            _imageData.ShowTracks = false;
            Frame.Navigate(typeof(MediaPage), _imageData);
        }
        private void ViewResults_Click(object sender, RoutedEventArgs e)
        {
            _imageData.TrackGaze = false;
            _imageData.ShowPoints = true;
            _imageData.ShowTracks = (bool)ShowTracks.IsChecked;
            Frame.Navigate(typeof(MediaPage), _imageData);
        }
    }
}
