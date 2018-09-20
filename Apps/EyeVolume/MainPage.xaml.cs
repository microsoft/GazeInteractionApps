using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EyeVolume
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaElement _mediaElement;
        private VolumeControl _volumeControl;

        ObservableCollection<DeviceInformation> PlaybackDeviceList = new ObservableCollection<DeviceInformation>();

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            var view = ApplicationView.GetForCurrentView();
            view.SetPreferredMinSize(new Size(800, 600));
            view.TryResizeView(new Size(800, 600));
            view.TryEnterFullScreenMode();            

            _volumeControl = new VolumeControl();
            VolumeSlider.Value = (int)(_volumeControl.Volume * 100);

            LoadTestAudioAsync();

            MuteToggle.IsChecked = _volumeControl.Mute;

            MuteToggle.Click += OnMute;
        }

        private async void LoadTestAudioAsync()
        {
            _mediaElement = new MediaElement();
            var path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\Assets";
            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            var file = await folder.GetFileAsync("Windows Background.wav");
            var stream = await file.OpenReadAsync();
            _mediaElement.SetSource(stream, file.ContentType);
        }

        private async void GetPlaybackDevices()
        {

            var deviceList = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);

            PlaybackDeviceList.Clear();

            foreach (var deviceInfo in deviceList)
            {
                PlaybackDeviceList.Add(deviceInfo);
            }
            
        }

        private void OnMute(object sender, RoutedEventArgs e)
        {
            _volumeControl.Mute = !_volumeControl.Mute;
        }

        private void OnVolumeUp(object sender, RoutedEventArgs e)
        {
            _volumeControl.Volume = _volumeControl.Volume + 0.05f;
            VolumeSlider.Value = (int)(_volumeControl.Volume * 100);
            _mediaElement.Play();
        }

        private void OnVolumeDown(object sender, RoutedEventArgs e)
        {
            _volumeControl.Volume = _volumeControl.Volume - 0.05f;
            VolumeSlider.Value = (int)(_volumeControl.Volume * 100);
            _mediaElement.Play();
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void NextPlaybackDevice(object sender, RoutedEventArgs e)
        {
            //GetPlaybackDevices();
        }
    }
}
