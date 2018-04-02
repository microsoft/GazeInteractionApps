using Microsoft.Toolkit.UWP.Input.Gaze;
using System;
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
        private const int DEFAULT_ENTER_EXIT_DELAY = 50000;
        private const int DEFAULT_MAX_HISTORY_DURATION = 3000000;
        private const int MAX_SINGLE_SAMPLE_DURATION = 100000;
        private const int GAZE_IDLE_TIME = 2500000;

        // GazeCursor
        private const int DEFAULT_CURSOR_RADIUS = 5;
        private const bool DEFAULT_CURSOR_VISIBILITY = true;

        // OneEuroFilter
        private const float ONEEUROFILTER_DEFAULT_BETA = 5.0f;
        private const float ONEEUROFILTER_DEFAULT_CUTOFF = 0.1f;
        private const float ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF = 1.0f;

        #endregion

        public GazePointer GazePointer = null;

        #region Properties
        #region GazePointer
        int? _GazePointer_FixationDelay;
        public int GazePointer_FixationDelay
        {
            get
            {
                if (!_GazePointer_FixationDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.FixationDelay"))
                    {
                        _GazePointer_FixationDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"]);
                    }
                    else
                    {
                        _GazePointer_FixationDelay = DEFAULT_FIXATION_DELAY;
                    }
                }
                return _GazePointer_FixationDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_FixationDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"] = value;
                }
            }
        }

        int? _GazePointer_DwellDelay;
        public int GazePointer_DwellDelay
        {
            get
            {
                if (!_GazePointer_DwellDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.DwellDelay"))
                    {
                        _GazePointer_DwellDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"]);
                    }
                    else
                    {
                        _GazePointer_DwellDelay = DEFAULT_DWELL_DELAY;
                    }
                }
                return _GazePointer_DwellDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_DwellDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"] = value;
                }
            }
        }

        int? _GazePointer_RepeatDelay;
        public int GazePointer_RepeatDelay
        {
            get
            {
                if (!_GazePointer_RepeatDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.RepeatDelay"))
                    {
                        _GazePointer_RepeatDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"]);
                    }
                    else
                    {
                        _GazePointer_RepeatDelay = DEFAULT_REPEAT_DELAY;
                    }
                }
                return _GazePointer_RepeatDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_RepeatDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"] = value;
                }
            }
        }

        int? _GazePointer_EnterExitDelay;
        public int GazePointer_EnterExitDelay
        {
            get
            {
                if (!_GazePointer_EnterExitDelay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.EnterExitDelay"))
                    {
                        _GazePointer_EnterExitDelay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.EnterExitDelay"]);
                    }
                    else
                    {
                        _GazePointer_EnterExitDelay = DEFAULT_ENTER_EXIT_DELAY;
                    }
                }
                return _GazePointer_EnterExitDelay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_EnterExitDelay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.EnterExitDelay"] = value;
                }
            }
        }

        int? _GazePointer_MaxHistoryDuration;
        public int GazePointer_MaxHistoryDuration
        {
            get
            {
                if (!_GazePointer_MaxHistoryDuration.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.MaxHistoryDuration"))
                    {
                        _GazePointer_MaxHistoryDuration = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.MaxHistoryDuration"]);
                    }
                    else
                    {
                        _GazePointer_MaxHistoryDuration = DEFAULT_MAX_HISTORY_DURATION;
                    }
                }
                return _GazePointer_MaxHistoryDuration.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_MaxHistoryDuration, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.MaxHistoryDuration"] = value;
                }
            }
        }

        int? _GazePointer_MaxSingleSampleDuration;
        public int GazePointer_MaxSingleSampleDuration
        {
            get
            {
                if (!_GazePointer_MaxSingleSampleDuration.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.MaxSingleSampleDuration"))
                    {
                        _GazePointer_MaxSingleSampleDuration = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.MaxSingleSampleDuration"]);
                    }
                    else
                    {
                        _GazePointer_MaxSingleSampleDuration = MAX_SINGLE_SAMPLE_DURATION;
                    }
                }
                return _GazePointer_MaxSingleSampleDuration.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_MaxSingleSampleDuration, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.MaxSingleSampleDuration"] = value;
                }
            }
        }

        int? _GazePointer_GazeIdleTime;
        public int GazePointer_GazeIdleTime
        {
            get
            {
                if (!_GazePointer_GazeIdleTime.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer.GazeIdleTime"))
                    {
                        _GazePointer_GazeIdleTime = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"]);
                    }
                    else
                    {
                        _GazePointer_GazeIdleTime = GAZE_IDLE_TIME;
                    }
                }
                return _GazePointer_GazeIdleTime.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_GazeIdleTime, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"] = value;
                }
            }
        }
        #endregion

        #region GazeCursor
        int? _GazeCursor_CursorRadius;
        public int GazeCursor_CursorRadius
        {
            get
            {
                if (!_GazeCursor_CursorRadius.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazeCursor.CursorRadius"))
                    {
                        _GazeCursor_CursorRadius = (int)(ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"]);
                    }
                    else
                    {
                        _GazeCursor_CursorRadius = DEFAULT_CURSOR_RADIUS;
                    }
                }
                return _GazeCursor_CursorRadius.Value;
            }
            set
            {
                if (SetProperty(ref _GazeCursor_CursorRadius, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"] = value;
                    if (GazePointer != null)
                    {
                        GazePointer.CursorRadius = value;
                    }
                }
            }
        }

        bool? _GazeCursor_CursorVisibility;

        public bool GazeCursor_CursorVisibility
        {
            get
            {
                if (!_GazeCursor_CursorVisibility.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazeCursor.CursorVisibility"))
                    {
                        _GazeCursor_CursorVisibility = (bool)(ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"]);
                    }
                    else
                    {
                        _GazeCursor_CursorVisibility = DEFAULT_CURSOR_VISIBILITY;
                    }
                }
                return _GazeCursor_CursorVisibility.Value;
            }
            set
            {
                if (SetProperty(ref _GazeCursor_CursorVisibility, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"] = value;
                    if (GazePointer != null)
                    {
                        GazePointer.IsCursorVisible = value;
                    }
                }
            }
        }
        #endregion

        #region OneEuroFilter
        float? _OneEuroFilter_Beta;
        public float OneEuroFilter_Beta
        {
            get
            {
                if (!_OneEuroFilter_Beta.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter.Beta"))
                    {
                        _OneEuroFilter_Beta = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"]);
                    }
                    else
                    {
                        _OneEuroFilter_Beta = ONEEUROFILTER_DEFAULT_BETA;
                    }
                }
                return _OneEuroFilter_Beta.Value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"] = value;
                SetProperty(ref _OneEuroFilter_Beta, value);
            }
        }

        float? _OneEuroFilter_Cutoff;
        public float OneEuroFilter_Cutoff
        {
            get
            {
                if (!_OneEuroFilter_Cutoff.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter.Cutoff"))
                    {
                        _OneEuroFilter_Cutoff = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"]);
                    }
                    else
                    {
                        _OneEuroFilter_Cutoff = ONEEUROFILTER_DEFAULT_CUTOFF;
                    }
                }
                return _OneEuroFilter_Cutoff.Value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"] = value;
                SetProperty(ref _OneEuroFilter_Cutoff, value);
            }
        }

        float? _OneEuroFilter_VelocityCutoff;
        public float OneEuroFilter_VelocityCutoff
        {
            get
            {
                if (!_OneEuroFilter_VelocityCutoff.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter.VelocityCutoff"))
                    {
                        _OneEuroFilter_VelocityCutoff = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"]);
                    }
                    else
                    {
                        _OneEuroFilter_VelocityCutoff = ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF;
                    }
                }
                return _OneEuroFilter_VelocityCutoff.Value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"] = value;
                SetProperty(ref _OneEuroFilter_VelocityCutoff, value);
            }
        }
        #endregion
        #endregion

        #region Helpers
        public static void ValueSetFromLocalSettings(ValueSet output)
        {
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"] != null) { output["GazePointer.FixationDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"] != null) { output["GazePointer.DwellDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"] != null) { output["GazePointer.RepeatDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.EnterExitDelay"] != null) { output["GazePointer.EnterExitDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.EnterExitDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.MaxHistoryDuration"] != null) { output["GazePointer.MaxHistoryDuration"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.MaxHistoryDuration"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.MaxSingleSampleDuration"] != null) { output["GazePointer.MaxSingleSampleDuration"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.MaxSingleSampleDuration"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"] != null) { output["GazePointer.GazeIdleTime"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"]; }

            if (ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"] != null) { output["GazeCursor.CursorRadius"] = (int)ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"] != null) { output["GazeCursor.CursorVisibility"] = (bool)ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"]; }

            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"] != null) { output["OneEuroFilter.Beta"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"]; }
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"] != null) { output["OneEuroFilter.Cutoff"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"]; }
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"] != null) { output["OneEuroFilter.VelocityCutoff"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"]; }
        }

        public void Reset()
        {
            GazePointer_FixationDelay = DEFAULT_FIXATION_DELAY;
            GazePointer_DwellDelay = DEFAULT_DWELL_DELAY;
            GazePointer_RepeatDelay = DEFAULT_REPEAT_DELAY;
            GazePointer_EnterExitDelay = DEFAULT_ENTER_EXIT_DELAY;
            GazePointer_MaxHistoryDuration = DEFAULT_MAX_HISTORY_DURATION;
            GazePointer_MaxSingleSampleDuration = MAX_SINGLE_SAMPLE_DURATION;
            GazePointer_GazeIdleTime = GAZE_IDLE_TIME;

            GazeCursor_CursorRadius = DEFAULT_CURSOR_RADIUS;
            GazeCursor_CursorVisibility = DEFAULT_CURSOR_VISIBILITY;

            OneEuroFilter_Beta = ONEEUROFILTER_DEFAULT_BETA;
            OneEuroFilter_Cutoff = ONEEUROFILTER_DEFAULT_CUTOFF;
            OneEuroFilter_VelocityCutoff = ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF;
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
