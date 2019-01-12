using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation;

using Windows.Graphics.Display;
using Windows.Media.Devices;

using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            MaximizeWindowOnLoad();

            this.InitializeComponent();

            VersionTextBlock.Text = GetAppVersion();

            var view = ApplicationView.GetForCurrentView();                        

            ApplicationViewTitleBar formattableTitleBar = view.TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;            
            coreTitleBar.ExtendViewIntoTitleBar = true;

            _volumeControl = new VolumeControl();
            VolumeSlider.Value = (int)(_volumeControl.Volume * 100);
                        
            ShowPlaybackDevice(MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default));

            MediaDevice.DefaultAudioRenderDeviceChanged += MediaDevice_DefaultAudioRenderDeviceChanged;

            MuteToggle.IsChecked = _volumeControl.Mute;

            MuteToggle.Checked += OnMuteOn;
            MuteToggle.Unchecked += OnMuteOff;            
        }

        void MaximizeWindowOnLoad()
        {
            var view = DisplayInformation.GetForCurrentView();
                        
            var resolution = new Size(view.ScreenWidthInRawPixels, view.ScreenHeightInRawPixels);                       
            var scale = view.ResolutionScale == ResolutionScale.Invalid ? 1 : view.RawPixelsPerViewPixel;                      
            var bounds = new Size(resolution.Width / scale, resolution.Height / scale);

            ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width, bounds.Height);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;          
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

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
