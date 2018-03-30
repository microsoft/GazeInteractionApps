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
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter_Beta"))
                    {
                        _OneEuroFilter_Beta = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Beta"]);
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
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Beta"] = value;
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
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter_Cutoff"))
                    {
                        _OneEuroFilter_Cutoff = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Cutoff"]);
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
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Cutoff"] = value;
                SetProperty(ref _OneEuroFilter_Cutoff, value);
            }
        }

        float? _OneEuroFilter_Velocity_Cutoff;
        public float OneEuroFilter_Velocity_Cutoff
        {
            get
            {
                if (!_OneEuroFilter_Velocity_Cutoff.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("OneEuroFilter_Velocity_Cutoff"))
                    {
                        _OneEuroFilter_Velocity_Cutoff = (float)(ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Velocity_Cutoff"]);
                    }
                    else
                    {
                        _OneEuroFilter_Velocity_Cutoff = 0f;
                    }
                }
                return _OneEuroFilter_Velocity_Cutoff.Value;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Velocity_Cutoff"] = value;
                SetProperty(ref _OneEuroFilter_Velocity_Cutoff, value);
            }
        }
        #endregion

        #region GazePointer
        int? _GazePointer_Fixation_Delay;
        public int GazePointer_Fixation_Delay
        {
            get
            {
                if (!_GazePointer_Fixation_Delay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer_Fixation_Delay"))
                    {
                        _GazePointer_Fixation_Delay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer_Fixation_Delay"]);
                    }
                    else
                    {
                        _GazePointer_Fixation_Delay = 0;
                    }
                }
                return _GazePointer_Fixation_Delay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_Fixation_Delay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer_Fixation_Delay"] = value;
                }
            }
        }

        int? _GazePointer_Dwell_Delay;
        public int GazePointer_Dwell_Delay
        {
            get
            {
                if (!_GazePointer_Dwell_Delay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer_Dwell_Delay"))
                    {
                        _GazePointer_Dwell_Delay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer_Dwell_Delay"]);
                    }
                    else
                    {
                        _GazePointer_Dwell_Delay = 0;
                    }
                }
                return _GazePointer_Dwell_Delay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_Dwell_Delay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer_Dwell_Delay"] = value;
                }
            }
        }

        int? _GazePointer_Repeat_Delay;
        public int GazePointer_Repeat_Delay
        {
            get
            {
                if (!_GazePointer_Repeat_Delay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer_Repeat_Delay"))
                    {
                        _GazePointer_Repeat_Delay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer_Repeat_Delay"]);
                    }
                    else
                    {
                        _GazePointer_Repeat_Delay = 0;
                    }
                }
                return _GazePointer_Repeat_Delay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_Repeat_Delay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer_Repeat_Delay"] = value;
                }
            }
        }

        int? _GazePointer_Enter_Exit_Delay;
        public int GazePointer_Enter_Exit_Delay
        {
            get
            {
                if (!_GazePointer_Enter_Exit_Delay.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer_Enter_Exit_Delay"))
                    {
                        _GazePointer_Enter_Exit_Delay = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer_Enter_Exit_Delay"]);
                    }
                    else
                    {
                        _GazePointer_Enter_Exit_Delay = 0;
                    }
                }
                return _GazePointer_Enter_Exit_Delay.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_Enter_Exit_Delay, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer_Enter_Exit_Delay"] = value;
                }
            }
        }

        int? _GazePointer_Max_History_Duration;
        public int GazePointer_Max_History_Duration
        {
            get
            {
                if (!_GazePointer_Max_History_Duration.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer_Max_History_Duration"))
                    {
                        _GazePointer_Max_History_Duration = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer_Max_History_Duration"]);
                    }
                    else
                    {
                        _GazePointer_Max_History_Duration = 0;
                    }
                }
                return _GazePointer_Max_History_Duration.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_Max_History_Duration, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer_Max_History_Duration"] = value;
                }
            }
        }

        int? _GazePointer_Max_Single_Sample_Duration;
        public int GazePointer_Max_Single_Sample_Duration
        {
            get
            {
                if (!_GazePointer_Max_Single_Sample_Duration.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer_Max_Single_Sample_Duration"))
                    {
                        _GazePointer_Max_Single_Sample_Duration = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer_Max_Single_Sample_Duration"]);
                    }
                    else
                    {
                        _GazePointer_Max_Single_Sample_Duration = 0;
                    }
                }
                return _GazePointer_Max_Single_Sample_Duration.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_Max_Single_Sample_Duration, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer_Max_Single_Sample_Duration"] = value;
                }
            }
        }

        int? _GazePointer_Gaze_Idle_Time;
        public int GazePointer_Gaze_Idle_Time
        {
            get
            {
                if (!_GazePointer_Gaze_Idle_Time.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazePointer_Gaze_Idle_Time"))
                    {
                        _GazePointer_Gaze_Idle_Time = (int)(ApplicationData.Current.LocalSettings.Values["GazePointer_Gaze_Idle_Time"]);
                    }
                    else
                    {
                        _GazePointer_Gaze_Idle_Time = 0;
                    }
                }
                return _GazePointer_Gaze_Idle_Time.Value;
            }
            set
            {
                if (SetProperty(ref _GazePointer_Gaze_Idle_Time, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazePointer_Gaze_Idle_Time"] = value;
                }
            }
        }
        #endregion

        #region GazeCursor
        int? _GazeCursor_Cursor_Radius;
        public int GazeCursor_Cursor_Radius
        {
            get
            {
                if (!_GazeCursor_Cursor_Radius.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazeCursor_Cursor_Radius"))
                    {
                        _GazeCursor_Cursor_Radius = (int)(ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Radius"]);
                    }
                    else
                    {
                        _GazeCursor_Cursor_Radius = 0;
                    }
                }
                return _GazeCursor_Cursor_Radius.Value;
            }
            set
            {
                if (SetProperty(ref _GazeCursor_Cursor_Radius, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Radius"] = value;
                    if (GazePointer != null)
                    {
                        GazePointer.CursorRadius = value;
                    }
                }
            }
        }

        bool? _GazeCursor_Cursor_Visibility;

        public bool GazeCursor_Cursor_Visibility
        {
            get
            {
                if (!_GazeCursor_Cursor_Visibility.HasValue)
                {
                    if (ApplicationData.Current.LocalSettings.Values.Keys.Contains("GazeCursor_Cursor_Visibility"))
                    {
                        _GazeCursor_Cursor_Visibility = (bool)(ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Visibility"]);
                    }
                    else
                    {
                        _GazeCursor_Cursor_Visibility = true;
                    }
                }
                return _GazeCursor_Cursor_Visibility.Value;
            }
            set
            {
                if (SetProperty(ref _GazeCursor_Cursor_Visibility, value))
                {
                    ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Visibility"] = value;
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
