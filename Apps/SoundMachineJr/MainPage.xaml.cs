using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PeteBrown.Devices.Midi;
using Windows.Devices.Midi;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SoundMachineJr
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int _numRows;
        private int _numCols;
        public MainViewModel ViewModel;

        private DispatcherTimer _noteTimer;

        public MainPage()
        {
            this.InitializeComponent();

            _numRows = 8;
            _numCols = 8;
            ViewModel = new MainViewModel(_numRows, _numCols);

            _noteTimer = new DispatcherTimer();
            _noteTimer.Interval = TimeSpan.FromMilliseconds(62.5);
            _noteTimer.Tick += OnNoteTimerTick;

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnNoteTimerTick(object sender, object e)
        {
            ViewModel.PlayCurrentColumn();
        }

        private void OnNoteClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var msg = button.DataContext as MidiMessage;
            ViewModel.ToggleNote(msg.Index);
        }

        private void OnInsertBeforeClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.InsertBlock(true);
        }

        private void OnInsertAfterClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.InsertBlock(false);
        }

        private void MusicBlock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectBlock(MusicBlocksList.SelectedIndex);
        }

        private void OnPlayOrStopClicked(object sender, RoutedEventArgs e)
        {
            if (_noteTimer.IsEnabled)
            {
                _noteTimer.Stop();
            }
            else
            {
                _noteTimer.Start();
            }
        }

        private void OnAllOrCurrentClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.PlayAll = !ViewModel.PlayAll;
        }
    }
}
