//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechSynthesis;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Phrasor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class KeyboardPage : Page
    {
        KeyboardPageNavigationParams _navParams;

        MediaElement _mediaElement;
        SpeechSynthesizer _speechSynthesizer;

        public KeyboardPage()
        {
            this.InitializeComponent();

            _mediaElement = new MediaElement();
            _speechSynthesizer = new SpeechSynthesizer();

            this.GazeKeyboard.EnterButton.Content = "\uE73E";
            this.GazeKeyboard.EnterButton.Click += OnEnterClick;
            this.GazeKeyboard.CloseButton.Click += OnCancelClick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _navParams = (KeyboardPageNavigationParams)e.Parameter;
            if (_navParams.SpeechMode)
            {
                this.GazeKeyboard.EnterButton.Content = "\uE768";
                GazeKeyboard.TextControl.Focus(FocusState.Programmatic);
                return;
            }
            this.GazeKeyboard.EnterButton.Content = "\uE73E";
            if (_navParams.ChildNode != null)
            {
                GazeKeyboard.TextControl.Text = _navParams.ChildNode.Caption;
                GazeKeyboard.TextControl.SelectAll();
            }
            
        }
        private async void OnEnterClick(object sender, RoutedEventArgs e)
        {            
            if (_navParams.SpeechMode)
            {
                var text = GazeKeyboard.TextControl.Text.ToString();
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
                        childNode.Children = new List<PhraseNode>();
                    }
                    _navParams.CurrentNode.Children.Add(childNode);
                }
                childNode.Caption = GazeKeyboard.TextControl.Text;
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
