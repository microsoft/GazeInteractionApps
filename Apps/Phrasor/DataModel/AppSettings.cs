//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI;

namespace Phrasor
{
    class AppSettings : INotifyPropertyChanged
    {        

        public void Load()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            var settingValue = localSettings.Values["normalDwellDuration"];
            if (settingValue!=null) NormalDwellDuration = (TimeSpan)settingValue;

            settingValue = localSettings.Values["destructiveDwellDuration"];
            if (settingValue != null) DestructiveDwellDuration = (TimeSpan)settingValue;

            settingValue = localSettings.Values["rows"];
            if (settingValue != null) Rows = (int)settingValue;

            settingValue = localSettings.Values["cols"];
            if (settingValue != null) Cols = (int)settingValue;

            settingValue = localSettings.Values["gazePlusClickMode"];
            if (settingValue != null) GazePlusClickMode = (bool)settingValue;

            settingValue = localSettings.Values["fontSize"];
            if (settingValue != null) FontSize = (double)settingValue;
            
            settingValue = localSettings.Values["fontColorA"];
            if (settingValue != null)
            {
                byte a = (byte)settingValue;
                settingValue = localSettings.Values["fontColorR"];
                byte r= (byte)settingValue;
                settingValue = localSettings.Values["fontColorG"];
                byte g= (byte)settingValue;
                settingValue = localSettings.Values["fontColorB"];
                byte b = (byte)settingValue;

                FontColor = Color.FromArgb(a, r, g, b);
            }
        }

        public void Save()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["normalDwellDuration"] = NormalDwellDuration;
            localSettings.Values["destructiveDwellDuration"] = DestructiveDwellDuration;

            localSettings.Values["rows"] = Rows;
            localSettings.Values["cols"] = Cols;

            localSettings.Values["gazePlusClickMode"] = GazePlusClickMode;

            localSettings.Values["fontSize"] = FontSize;

            localSettings.Values["fontColorA"] = FontColor.A;
            localSettings.Values["fontColorR"] = FontColor.R;
            localSettings.Values["fontColorG"] = FontColor.G;
            localSettings.Values["fontColorB"] = FontColor.B;
        }

        private TimeSpan normalDwellDuration = new TimeSpan(0, 0, 0, 0, 400);
        public TimeSpan NormalDwellDuration
        {
            get { return normalDwellDuration; }
            set
            {
                normalDwellDuration = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan destructiveDwellDuration = new TimeSpan(0,0,0,0,1500);
        public TimeSpan DestructiveDwellDuration
        {
            get { return destructiveDwellDuration; }
            set
            {
                destructiveDwellDuration = value;
                OnPropertyChanged();
            }
        }

        private double fontSize = 20;
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                OnPropertyChanged();
            }
        }

        private Color fontColor = Colors.Blue;
        public Color FontColor
        {
            get { return fontColor; }
            set
            {
                fontColor = value;
                OnPropertyChanged();
            }
        }

        private int rows = 4;
        public int Rows
        {
            get { return rows; }
            set
            {
                rows = value;
                OnPropertyChanged();
            }
        }

        private int cols = 4;
        public int Cols
        {
            get { return cols; }
            set
            {
                cols = value;
                OnPropertyChanged();
            }
        }

        private bool gazePlusClickMode = false;
        public bool GazePlusClickMode
        {
            get { return gazePlusClickMode; }
            set
            {
                gazePlusClickMode = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            var holder = PropertyChanged;
            if (holder != null)
            {
                holder(this,
                    new PropertyChangedEventArgs(caller));
            }
        }
    }
}
