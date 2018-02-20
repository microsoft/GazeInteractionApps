using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Research.Input.Gaze;
using Windows.Media.SpeechSynthesis;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MinAAC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        StringBuilder       _textToSpeak;
        GazePointer         _gazePointer;
        MediaElement        _mediaElement;
        SpeechSynthesizer   _speechSynthesizer;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            _mediaElement = new MediaElement();
            _speechSynthesizer = new SpeechSynthesizer();
            _textToSpeak = new StringBuilder();
            _gazePointer = new GazePointer(this);
        }

        private void _gazePointer_OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                sender.InvokeTarget(ea.HitTarget);
            }
        }

        private void OnSettings(object sender, RoutedEventArgs e)
        {

        }

        private async void OnSpeak(object sender, RoutedEventArgs e)
        {
            var text = _textToSpeak.ToString();
            var stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(text);
            _mediaElement.SetSource(stream, stream.ContentType);
            _mediaElement.AutoPlay = true;
            _mediaElement.Play();
        }

        private void OnChar(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button.Tag.ToString();
            if (tag == "0x22")
            {
                _textToSpeak.Append(' ');
            }
            else if (tag == "0x0E")
            {
                _textToSpeak.Remove(_textToSpeak.Length - 1, 1);
            }
            else
            {
                var content = button.Content.ToString();
                _textToSpeak.Append(content);
            }
            TextToSpeak.Text = _textToSpeak.ToString();
        }

        private void OnWordDelete(object sender, RoutedEventArgs e)
        {
            var text = _textToSpeak.ToString();
            int lastSpace = text.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                _textToSpeak.Remove(lastSpace, _textToSpeak.Length - lastSpace);
            }
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
