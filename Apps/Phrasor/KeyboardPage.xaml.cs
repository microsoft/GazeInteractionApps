//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechSynthesis;

namespace Phrasor
{
    public sealed partial class KeyboardPage : Page
    {
        KeyboardPageNavigationParams _navParams;

        MediaElement _mediaElement;
        SpeechSynthesizer _speechSynthesizer;

        public KeyboardPage()
        {
            InitializeComponent();

            _mediaElement = new MediaElement();
            _speechSynthesizer = new SpeechSynthesizer();
            GazeKeyboard.Target = TextControl;
            Loaded += KeyboardPage_Loaded;

        }

        private async void KeyboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            await GazeKeyboard.LoadLayout("MinAAC.xaml");

            GazeKeyboard.GazePlusClickMode = _navParams.GazePlusClickMode;
            if (_navParams.SpeechMode)
            {
                EnterButton.Content = "\uE768";
                TextControl.Focus(FocusState.Programmatic);
                return;
            }
            EnterButton.Content = "\uE73E";
            if (_navParams.ChildNode != null)
            {
                TextControl.Text = _navParams.ChildNode.Caption;
                TextControl.SelectAll();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _navParams = (KeyboardPageNavigationParams)e.Parameter;
        }
        private async void OnEnterClick(object sender, RoutedEventArgs e)
        {            
            if (_navParams.SpeechMode)
            {
                var text = TextControl.Text.ToString();
                var stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(text);
                _mediaElement.SetSource(stream, stream.ContentType);
                _mediaElement.AutoPlay = true;
                _mediaElement.Play();
            }
            else
            {
                var childNode = _navParams.ChildNode;
                if (childNode == null)
                {
                    childNode = new PhraseNode();
                    if (_navParams.IsCategory)
                    {
                        childNode.IsCategory = true;                        
                    }
                    _navParams.CurrentNode.Children.Add(childNode);
                }
                childNode.Caption = TextControl.Text;
                childNode.Parent = _navParams.CurrentNode;
                _navParams.NeedsSaving = true;
                Frame.Navigate(typeof(MainPage), _navParams);
            }
            
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), _navParams);
        }
    }
}
