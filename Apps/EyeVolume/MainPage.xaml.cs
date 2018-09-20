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
using Windows.Media.Devices;
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
        private VolumeControl _volumeControl;                       

        public MainPage()
        {
            this.InitializeComponent();

            var view = ApplicationView.GetForCurrentView();            
            view.TryResizeView(new Size(VolumeControlGrid.Width, VolumeControlGrid.Height - 32));

            ApplicationViewTitleBar formattableTitleBar = view.TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            _volumeControl = new VolumeControl();
            VolumeSlider.Value = (int)(_volumeControl.Volume * 100);
                        
            ShowPlaybackDevice(MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default));

            MediaDevice.DefaultAudioRenderDeviceChanged += MediaDevice_DefaultAudioRenderDeviceChanged;

            MuteToggle.IsChecked = _volumeControl.Mute;

            MuteToggle.Checked += OnMuteOn;
            MuteToggle.Unchecked += OnMuteOff;            
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {            
            var _titleBarHeight = CoreApplication.GetCurrentView().TitleBar.Height;
            var view = ApplicationView.GetForCurrentView();
            view.SetPreferredMinSize(new Size(VolumeControlGrid.Width, VolumeControlGrid.Height - _titleBarHeight));
            view.TryResizeView(new Size(VolumeControlGrid.Width, VolumeControlGrid.Height - _titleBarHeight));            
            view.TryEnterFullScreenMode();
        }

        private void MediaDevice_DefaultAudioRenderDeviceChanged(object sender, DefaultAudioRenderDeviceChangedEventArgs args)
        {
            ShowPlaybackDevice(args.Id);
        }       

        private async void ShowPlaybackDevice(string defaultDeviceId)
        {
            var deviceList = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);                             

            foreach (var deviceInfo in deviceList)
            {
                if (deviceInfo.Id == defaultDeviceId)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PlaybackDeviceText.Text = deviceInfo.Name;
                    });
                   
                    return;
                }
            }
            PlaybackDeviceText.Text = "";
        }

        private void OnMuteOn(object sender, RoutedEventArgs e)
        {            
            _volumeControl.Mute = true;
            MuteToggle.IsChecked = _volumeControl.Mute;            
        }

        private void OnMuteOff(object sender, RoutedEventArgs e)
        {
            _volumeControl.Mute = false;
            MuteToggle.IsChecked = _volumeControl.Mute;
        }

        private void OnVolumeUp(object sender, RoutedEventArgs e)
        {
            _volumeControl.Volume = _volumeControl.Volume + 0.05f;
            VolumeSlider.Value = (int)(_volumeControl.Volume * 100);            
            AudioPlayer.Position = new TimeSpan(0);
            AudioPlayer.Play();
        }

        private void OnVolumeDown(object sender, RoutedEventArgs e)
        {
            _volumeControl.Volume = _volumeControl.Volume - 0.05f;
            VolumeSlider.Value = (int)(_volumeControl.Volume * 100);
            AudioPlayer.Position = new TimeSpan(0);
            AudioPlayer.Play();
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }           
    }
}
