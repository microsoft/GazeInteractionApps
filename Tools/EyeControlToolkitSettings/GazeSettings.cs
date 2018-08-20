//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace EyeControlToolkitSettings
{
    public class GazeSettings : INotifyPropertyChanged
    {
        #region Default Values
        // GazePointer
        // units in microseconds
        private const int DEFAULT_FIXATION_DELAY = 400000;
        private const int DEFAULT_DWELL_DELAY = 800000;
        private const int DEFAULT_REPEAT_DELAY = int.MaxValue;
        private const int DEFAULT_DWELL_REPEAT_DELAY = int.MaxValue;
        private const int DEFAULT_THRESHOLD_DELAY = 50000;
        private const int GAZE_IDLE_TIME = 2500000;
        private const bool DEFAULT_SWITCH_ENABLED = false;

        // GazeCursor
        private const int DEFAULT_CURSOR_RADIUS = 5;
        private const bool DEFAULT_CURSOR_VISIBILITY = true;

        // OneEuroFilter
        private const float ONEEUROFILTER_DEFAULT_BETA = 5.0f;
        private const float ONEEUROFILTER_DEFAULT_CUTOFF = 0.1f;
        private const float ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF = 1.0f;

        #endregion

        #region Properties
        #region GazePointer
        int? _GazePointerFixationDelay;
        public int GazePointerFixationDelay
        {
            get
            {
                if (!_GazePointerFixationDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.FixationDelay"))
                    {
                        _GazePointerFixationDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"]);
                    }
                    else
                    {
                        _GazePointerFixationDelay = DEFAULT_FIXATION_DELAY;
                    }
                }
                return _GazePointerFixationDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointerFixationDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"] = value;
                }
            }
        }

        int? _GazePointerDwellDelay;
        public int GazePointerDwellDelay
        {
            get
            {
                if (!_GazePointerDwellDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.DwellDelay"))
                    {
                        _GazePointerDwellDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"]);
                    }
                    else
                    {
                        _GazePointerDwellDelay = DEFAULT_DWELL_DELAY;
                    }
                }
                return _GazePointerDwellDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointerDwellDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"] = value;
                }
            }
        }

        int? _GazePointerRepeatDelay;
        public int GazePointerRepeatDelay
        {
            get
            {
                if (!_GazePointerRepeatDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.RepeatDelay"))
                    {
                        _GazePointerRepeatDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"]);
                    }
                    else
                    {
                        _GazePointerRepeatDelay = DEFAULT_REPEAT_DELAY;
                    }
                }
                return _GazePointerRepeatDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointerRepeatDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"] = value;
                }
            }
        }

        int? _GazePointerDwellRepeatDelay;
        public int GazePointerDwellRepeatDelay
        {
            get
            {
                if (!_GazePointerDwellRepeatDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.DwellRepeatDelay"))
                    {
                        _GazePointerDwellRepeatDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.DwellRepeatDelay"]);
                    }
                    else
                    {
                        _GazePointerDwellRepeatDelay = DEFAULT_DWELL_REPEAT_DELAY;
                    }
                }
                return _GazePointerDwellRepeatDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointerDwellRepeatDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.DwellRepeatDelay"] = value;
                }
            }
        }

        int? _GazePointerThresholdDelay;
        public int GazePointerThresholdDelay
        {
            get
            {
                if (!_GazePointerThresholdDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.ThresholdDelay"))
                    {
                        _GazePointerThresholdDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.ThresholdDelay"]);
                    }
                    else
                    {
                        _GazePointerThresholdDelay = DEFAULT_THRESHOLD_DELAY;
                    }
                }
                return _GazePointerThresholdDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointerThresholdDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.ThresholdDelay"] = value;
                }
            }
        }

        int? _GazePointerGazeIdleTime;
        public int GazePointerGazeIdleTime
        {
            get
            {
                if (!_GazePointerGazeIdleTime.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.GazeIdleTime"))
                    {
                        _GazePointerGazeIdleTime = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"]);
                    }
                    else
                    {
                        _GazePointerGazeIdleTime = GAZE_IDLE_TIME;
                    }
                }
                return _GazePointerGazeIdleTime.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointerGazeIdleTime, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"] = value;
                }
            }
        }

        bool? _GazePointerIsSwitchEnabled;
        public bool GazePointerIsSwitchEnabled
        {
            get
            {
                if (!_GazePointerIsSwitchEnabled.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.IsSwitchEnabled"))
                    {
                        _GazePointerIsSwitchEnabled = (bool)(ApplicationData.Current.LocalSettings.Values["GazePointer.IsSwitchEnabled"]);
                    }
                    else
                    {
                        _GazePointerIsSwitchEnabled = DEFAULT_SWITCH_ENABLED;
                    }
                }
                return _GazePointerIsSwitchEnabled.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointerIsSwitchEnabled, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.IsSwitchEnabled"] = value;
                }
            }
        }
        #endregion

        #region GazeCursor
        int? _GazeCursorRadius;
        public int GazeCursorRadius
        {
            get
            {
                if (!_GazeCursorRadius.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazeCursor.CursorRadius"))
                    {
                        _GazeCursorRadius = (int)(ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"]);
                    }
                    else
                    {
                        _GazeCursorRadius = DEFAULT_CURSOR_RADIUS;
                    }
                }
                return _GazeCursorRadius.Value;
            }
            set
            {
                if (IsGazeCursorRadiusValid(value))
                {
                    if (SetProperty(ref _GazeCursorRadius, value))
                    {
                        ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"] = value;
                    }
                }
            }
        }

        bool? _GazeCursorVisibility;
        public bool GazeCursorVisibility
        {
            get
            {
                if (!_GazeCursorVisibility.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazeCursor.CursorVisibility"))
                    {
                        _GazeCursorVisibility = (bool)(ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"]);
                    }
                    else
                    {
                        _GazeCursorVisibility = DEFAULT_CURSOR_VISIBILITY;
                    }
                }
                return _GazeCursorVisibility.Value;
            }
            set
            {
                if (SetProperty(ref _GazeCursorVisibility, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"] = value;
                }
            }
        }
        #endregion

        #region OneEuroFilter
        float? _OneEuroFilterBeta;
        public float OneEuroFilterBeta
        {
            get
            {
                if (!_OneEuroFilterBeta.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter.Beta"))
                    {
                        _OneEuroFilterBeta = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"]);
                    }
                    else
                    {
                        _OneEuroFilterBeta = ONEEUROFILTER_DEFAULT_BETA;
                    }
                }
                return _OneEuroFilterBeta.Value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"] = value;
                SetProperty(ref _OneEuroFilterBeta, value);
            }
        }

        float? _OneEuroFilterCutoff;
        public float OneEuroFilterCutoff
        {
            get
            {
                if (!_OneEuroFilterCutoff.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter.Cutoff"))
                    {
                        _OneEuroFilterCutoff = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"]);
                    }
                    else
                    {
                        _OneEuroFilterCutoff = ONEEUROFILTER_DEFAULT_CUTOFF;
                    }
                }
                return _OneEuroFilterCutoff.Value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"] = value;
                SetProperty(ref _OneEuroFilterCutoff, value);
            }
        }

        float? _OneEuroFilterVelocityCutoff;
        public float OneEuroFilterVelocityCutoff
        {
            get
            {
                if (!_OneEuroFilterVelocityCutoff.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter.VelocityCutoff"))
                    {
                        _OneEuroFilterVelocityCutoff = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"]);
                    }
                    else
                    {
                        _OneEuroFilterVelocityCutoff = ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF;
                    }
                }
                return _OneEuroFilterVelocityCutoff.Value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"] = value;
                SetProperty(ref _OneEuroFilterVelocityCutoff, value);
            }
        }
        #endregion
        #endregion

        #region Helpers
        public static void ValueSetFromLocalSettings(ValueSet output)
        {
            foreach (var key in ApplicationData.Current.LocalSettings.Values.Keys)
            {
                switch(key)
                {
                    case "GazePointer.FixationDelay":
                    case "GazePointer.DwellDelay":
                    case "GazePointer.DwellRepeatDelay":
                    case "GazePointer.RepeatDelay":
                    case "GazePointer.ThresholdDelay":
                    case "GazePointer.GazeIdleTime":
                    case "GazeCursor.CursorRadius":
                        output[key] = (int)ApplicationData.Current.LocalSettings.Values[key];
                        break;
                    case "GazePointer.IsSwitchEnabled":
                    case "GazeCursor.CursorVisibility":
                        output[key] = (bool)ApplicationData.Current.LocalSettings.Values[key];
                        break;
                    case "OneEuroFilter.Beta":
                    case "OneEuroFilter.Cutoff":
                    case "OneEuroFilter.VelocityCutoff":
                        output[key] = (float)ApplicationData.Current.LocalSettings.Values[key];
                        break;
                    default:
                        output["ERROR"] = $"Error: Uknown key {key} {ApplicationData.Current.LocalSettings.Values[key]}";
                        break;
                }
            }
        }

        public void Reset()
        {
            GazePointerFixationDelay = DEFAULT_FIXATION_DELAY;
            GazePointerDwellDelay = DEFAULT_DWELL_DELAY;
            GazePointerRepeatDelay = DEFAULT_REPEAT_DELAY;
            GazePointerDwellRepeatDelay = DEFAULT_DWELL_REPEAT_DELAY;
            GazePointerThresholdDelay = DEFAULT_THRESHOLD_DELAY;
            GazePointerGazeIdleTime = GAZE_IDLE_TIME;
            GazePointerIsSwitchEnabled = DEFAULT_SWITCH_ENABLED;

            GazeCursorRadius = DEFAULT_CURSOR_RADIUS;
            GazeCursorVisibility = DEFAULT_CURSOR_VISIBILITY;

            OneEuroFilterBeta = ONEEUROFILTER_DEFAULT_BETA;
            OneEuroFilterCutoff = ONEEUROFILTER_DEFAULT_CUTOFF;
            OneEuroFilterVelocityCutoff = ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF;
        }

        static public bool IsGazeCursorRadiusValid(int radius)
        {
            return (radius > 0) && (radius < 100);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {

            if (!Equals(property, value))
            {
                property = value;
                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
