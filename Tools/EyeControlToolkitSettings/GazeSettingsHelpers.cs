using Windows.Foundation.Collections;
using Windows.Storage;

namespace EyeControlToolkitSettings
{
    static class GazeSettingsHelpers
    {
        public static void ValueSetFromLocalSettings(ValueSet output)
        {
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"] != null) { output["OneEuroFilter.Beta"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Beta"]; }
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"] != null) { output["OneEuroFilter.Cutoff"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter.Cutoff"]; }
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"] != null) { output["OneEuroFilter.VelocityCutoff"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter.VelocityCutoff"]; }

            if (ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"] != null) { output["GazePointer.FixationDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.FixationDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"] != null) { output["GazePointer.DwellDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.DwellDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"] != null) { output["GazePointer.RepeatDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.RepeatDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.EnterExitDelay"] != null) { output["GazePointer.EnterExitDelay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.EnterExitDelay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.MaxHistoryDuration"] != null) { output["GazePointer.MaxHistoryDuration"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.MaxHistoryDuration"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.MaxSingleSampleDuration"] != null) { output["GazePointer.MaxSingleSampleDuration"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.MaxSingleSampleDuration"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"] != null) { output["GazePointer.GazeIdleTime"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer.GazeIdleTime"]; }

            if (ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"] != null) { output["GazeCursor.CursorRadius"] = (int)ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorRadius"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"] != null) { output["GazeCursor.CursorVisibility"] = (bool)ApplicationData.Current.LocalSettings.Values["GazeCursor.CursorVisibility"]; }
        }
    }
}
