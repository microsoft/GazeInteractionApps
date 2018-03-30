//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.UWP.Input.Gaze;

namespace Memory
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const byte MIN_CHAR = 0x21;
        const byte MAX_CHAR = 0xE8;

        GazePointer _gazePointer;

        Random _rnd;
        Button _firstButton;
        Button _secondButton;
        DispatcherTimer _flashTimer;
        int _remaining;
        int _numMoves;

        public MainPage()
        {
            this.InitializeComponent();

            _rnd = new Random();
            _flashTimer = new DispatcherTimer();
            _flashTimer.Interval = new TimeSpan(0, 0, 2);
            _flashTimer.Tick += OnFlashTimerTick;

            Loaded += MainPage_Loaded;

            GazeSettings.RetrieveSharedSettings(GazeSettings.Instance).Completed = new Windows.Foundation.AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
                _gazePointer = new GazePointer(this);
                _gazePointer.OnGazePointerEvent += OnGazePointerEvent;
            });
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ResetBoard();
        }

        private void OnFlashTimerTick(object sender, object e)
        {
            _firstButton.Content = null;
            _secondButton.Content = null;
            _firstButton = null;
            _secondButton = null;
            _flashTimer.Stop();
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }

        List<Button> ShuffleList(List<Button> list)
        {
            int len = list.Count;
            int shuffleCount = len * 10;
            while (shuffleCount > 0)
            {
                int index = _rnd.Next(0, len);
                var item = list[index];
                list.RemoveAt(index);
                list.Insert(0, item);
                shuffleCount--;
            }
            return list;
        }

        List<char> GetNewContent(int len)
        {
            List<char> list = new List<char>();
            for (int i = 0; i < len; i++)
            {
                list.Add(System.Convert.ToChar(_rnd.Next(MIN_CHAR, MAX_CHAR)));
            }
            return list;
        }

        List<Button> GetButtonList()
        {
            List<Button> list = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var name = $"button_{i}_{j}";
                    var button = FindName(name) as Button;
                    list.Add(button);
                }
            }
            return ShuffleList(list);
        }

        void ResetBoard()
        {
            _firstButton = null;
            _secondButton = null;
            _numMoves = 0;
            _remaining = 16;

            List<char> listChars = GetNewContent(8);
            List<Button> listButtons = GetButtonList();

            for (int i = 0; i < 16; i += 2)
            {
                listButtons[i].Content = null;
                listButtons[i + 1].Content = null;

                listButtons[i].Tag = listChars[i / 2];
                listButtons[i + 1].Tag = listChars[i / 2];
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if ((btn.Content != null) || (_flashTimer.IsEnabled))
            {
                return;
            }

            _numMoves++;

            if (_firstButton == null)
            {
                _firstButton = btn;
            }
            else
            {
                _secondButton = btn;
            }

            btn.Content = btn.Tag;

            if (_secondButton == null)
            {
                return;
            }

            if (_secondButton.Content.ToString() != _firstButton.Content.ToString())
            {
                _flashTimer.Start();
            }
            else
            {
                _firstButton = null;
                _secondButton = null;
                _remaining -= 2;

                CheckGameCompletion();
            }
        }

        void CheckGameCompletion()
        {
            if (_remaining > 0)
            {
                return;
            }

            string message = $"Congratulations!! You solved it in {_numMoves} moves";
            dialogText.Text = message;
            dialogGrid.Visibility = Visibility.Visible;           
        }

        private void dialogButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBoard();
            dialogGrid.Visibility = Visibility.Collapsed;
        }
    }
}
