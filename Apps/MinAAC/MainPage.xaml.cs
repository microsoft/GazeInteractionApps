//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.SpeechSynthesis;
using EyeGazeUserControls;

namespace MinAAC
{
    public sealed partial class MainPage : Page
    {
        const int NUM_PREDICTIONS = 5;
        MediaElement _mediaElement;
        SpeechSynthesizer   _speechSynthesizer;
        Button[]         _predictions;

        public MainPage()
        {
            InitializeComponent();
            _predictions = new Button[NUM_PREDICTIONS];
            _predictions[0] = Prediction0;
            _predictions[1] = Prediction1;
            _predictions[2] = Prediction2;
            _predictions[3] = Prediction3;
            _predictions[4] = Prediction4;

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            _mediaElement = new MediaElement();
            _speechSynthesizer = new SpeechSynthesizer();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            GazeKeyboard.Target = TextControl;
            await GazeKeyboard.LoadLayout("MinAAC.xaml");
            GazeKeyboard.PredictionTargets = _predictions;
        }

        private async void OnSpeak(object sender, RoutedEventArgs e)
        {
            var text = TextControl.Text.ToString();
            var stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(text);
            _mediaElement.SetSource(stream, stream.ContentType);
            _mediaElement.AutoPlay = true;
            _mediaElement.Play();
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
