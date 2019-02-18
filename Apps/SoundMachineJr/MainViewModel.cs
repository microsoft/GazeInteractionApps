using PeteBrown.Devices.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SoundMachineJr
{
    public class NotificationBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public sealed class ValueToBackgroundColorConverter : IValueConverter
    {
        private static Brush[] _backgroundColors = new Brush[]
        {
            new SolidColorBrush(Colors.LightGray),

            new SolidColorBrush(Colors.PaleTurquoise),
            new SolidColorBrush(Colors.LightBlue),
            new SolidColorBrush(Colors.LightSkyBlue),
            new SolidColorBrush(Colors.DeepSkyBlue),

            new SolidColorBrush(Colors.LemonChiffon),
            new SolidColorBrush(Colors.Khaki),
            new SolidColorBrush(Colors.DarkKhaki),
            new SolidColorBrush(Colors.Goldenrod),

            new SolidColorBrush(Colors.PaleGreen),
            new SolidColorBrush(Colors.LightGreen),
            new SolidColorBrush(Colors.YellowGreen),
            new SolidColorBrush(Colors.LimeGreen),

            new SolidColorBrush(Colors.MistyRose),
            new SolidColorBrush(Colors.Pink),
            new SolidColorBrush(Colors.HotPink),
            new SolidColorBrush(Colors.DeepPink),
        };

        public object Convert(object value, Type targetType,
                              object parameter, string culture)
        {
            return _backgroundColors[((byte)value) % _backgroundColors.Count<Brush>()];
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MidiMessage : NotificationBase
    {
        public static SolidColorBrush EmptyBrush = new SolidColorBrush(Colors.Gray);
        public static SolidColorBrush BlueBrush = new SolidColorBrush(Colors.Red);

        private byte _channel;
        public byte Channel
        {
            get { return _channel; }
            set { SetField<byte>(ref _channel, value, "Channel"); }
        }

        private byte _number;
        public byte Number
        {
            get { return _number; }
            set
            {
                SetField<byte>(ref _number, value, "Number");
            }
        }

        private byte _velocity;
        public byte Velocity
        {
            get { return _number; }
            set { SetField<byte>(ref _velocity, value, "Velocity"); }
        }

        // This is an ugly hack to work around the bug that
        // the UniformGrid always returns 0 for GetRow and GetColumn
        public int Index;

        public SolidColorBrush Color
        {
            get
            {
                return (Number == 0) ? EmptyBrush : BlueBrush;
            }
        }
    }

    public class MidiMessageList
    {
        public ObservableCollection<MidiMessage> MessageList;

        public MidiMessageList(int rows)
        {
            MessageList = new ObservableCollection<MidiMessage>();
            for (int i = 0; i < rows; i++)
            {
                var msg = new MidiMessage();
                msg.Index = i;
                MessageList.Add(msg);
            }
        }

        public void PlayMessageList(IMidiOutPort midiOutPort)
        {
            int numRows = MessageList.Count;
            for (int i = 0; i < numRows; i++)
            {
                var msg = MessageList[i];
                if (msg.Number != 0)
                {
                    var noteMessage = new MidiNoteOnMessage(msg.Channel, msg.Number, msg.Velocity);
                    midiOutPort.SendMessage(noteMessage);
                }
            }
        }
    }

    public class MidiMessageBlock
    {
        public ObservableCollection<MidiMessage> MessageBlock;

        public int CurrentColumn;

        private int _rows;
        private int _cols;

        public MidiMessageBlock(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;

            int count = rows * cols;
            MessageBlock = new ObservableCollection<MidiMessage>();
            for (int i = 0; i < count; i++)
            {
                var msg = new MidiMessage();
                msg.Index = i;
                msg.Channel = 0;
                msg.Number = 0;
                msg.Velocity = 0;
                MessageBlock.Add(msg);
            }
            CurrentColumn = 0;
        }

        public void ToggleNote(int index)
        {
            var midiMessage = MessageBlock[index];

            int row = index % _rows;
            if (midiMessage.Number == 0)
            {
                midiMessage.Channel = 9;
                midiMessage.Number = (byte)(35 + row);
                midiMessage.Velocity = 65;
            }
            else
            {
                midiMessage.Channel = midiMessage.Number = midiMessage.Velocity = 0;
            }
            
        }

        public bool PlayCurrentColumn(IMidiOutPort midiOutPort)
        {
            var start = (CurrentColumn * _rows);
            var end = start + _cols;
            for (int i = start; i < end; i++)
            {
                var msg = MessageBlock[i];
                if (msg.Number != 0)
                {
                    var noteMessage = new MidiNoteOnMessage(msg.Channel, msg.Number, msg.Velocity);
                    midiOutPort.SendMessage(noteMessage);
                }
            }
            CurrentColumn++;
            if (CurrentColumn >= _cols)
            {
                CurrentColumn = 0;
                return true;
            }
            return false;
        }
    }

    public class MainViewModel : NotificationBase
    {
        private IMidiOutPort _activeOutPort = null;
        public IMidiOutPort ActiveOutPort
        {
            get { return _activeOutPort; }
            set { SetField<IMidiOutPort>(ref _activeOutPort, value, "ActiveOutPport"); }
        }

        private ObservableCollection<MidiMessageBlock> _music;
        public ObservableCollection<MidiMessageBlock> Music
        {
            get { return _music; }
            set { SetField <ObservableCollection<MidiMessageBlock>>(ref _music, value, "Music"); }
        }

        private MidiDeviceWatcher _watcher = new MidiDeviceWatcher();

        private int _currentBlockIndex;
        public int CurrentBlockIndex
        {
            get { return _currentBlockIndex; }
            set { SetField<int>(ref _currentBlockIndex, value, "CurrentBlockIndex"); }
        }

        private MidiMessageBlock _currentBlock;
        public MidiMessageBlock CurrentBlock
        {
            get { return _currentBlock; }
            set { SetField<MidiMessageBlock>(ref _currentBlock, value, "CurrentBlock"); }
        }

        public bool PlayAll;

        int _rows;
        int _cols;

        public MainViewModel(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
            // make sure you have the event handlers wired up before this.
            _watcher.EnumerateOutputPorts();
            _watcher.OutputPortsEnumerated += OnMidiDevicesEnumerated;

            Music = new ObservableCollection<MidiMessageBlock>();
            Music.Add(new MidiMessageBlock(rows, cols));
            CurrentBlockIndex = 0;
            CurrentBlock = _music[CurrentBlockIndex];

            PlayAll = true;
        }

        private async void OnMidiDevicesEnumerated(MidiDeviceWatcher sender)
        {
            var id = _watcher.OutputPortDescriptors.GetAt(0).Id;
            _activeOutPort = await MidiOutPort.FromIdAsync(id);
        }

        public ObservableDeviceInformationCollection OutputPortDescriptors
        {
            get { return _watcher.OutputPortDescriptors; }
        }

        public void InsertBlock(bool before)
        {
            var block = new MidiMessageBlock(_rows, _cols);
            if (before)
            {
                _music.Insert(CurrentBlockIndex, block);
            }
            else
            {
                if (CurrentBlockIndex == _music.Count)
                {
                    _music.Add(block);
                }
                else
                {
                    CurrentBlockIndex++;
                    _music.Insert(CurrentBlockIndex, block);
                }
            }
            CurrentBlock = _music[CurrentBlockIndex];
        }

        public void PlayCurrentColumn()
        {
            if (_activeOutPort == null)
            {
                return;
            }

            if (CurrentBlock.PlayCurrentColumn(_activeOutPort))
            {
                int increment = PlayAll ? 1 : 0;
                CurrentBlockIndex = (CurrentBlockIndex + increment) % _music.Count;
            }

            CurrentBlock = _music[CurrentBlockIndex];
        }

        public void ToggleNote(int index)
        {
            CurrentBlock.ToggleNote(index);
        }

        public void SelectBlock(int index)
        {
            CurrentBlockIndex = index;
            CurrentBlock = _music[CurrentBlockIndex];
        }
    }
}
