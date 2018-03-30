using Microsoft.Research.Input.Gaze;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace EyeControlToolkitSettings
{
    public class GazeSettings : INotifyPropertyChanged
    {
        public GazePointer GazePointer = null;

        #region Properties
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
                        _OneEuroFilter_Beta = 0f;
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
                        _OneEuroFilter_Cutoff = 0f;
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
                        _OneEuroFilter_VelocityCutoff = 0f;
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
                        _GazePointer_FixationDelay = 0;
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
                        _GazePointer_DwellDelay = 0;
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
                        _GazePointer_RepeatDelay = 0;
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
                        _GazePointer_EnterExitDelay = 0;
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
                        _GazePointer_MaxHistoryDuration = 0;
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
                        _GazePointer_MaxSingleSampleDuration = 0;
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
                        _GazePointer_GazeIdleTime = 0;
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
                        _GazeCursor_CursorRadius = 0;
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
                        _GazeCursor_CursorVisibility = true;
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
