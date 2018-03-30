using Windows.Foundation.Collections;
using Windows.Storage;

namespace EyeControlToolkitSettings
{
    static class GazeSettingsHelpers
    {
        public static void ValueSetFromLocalSettings(ValueSet output)
        {
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Beta"] != null) { output["OneEuroFilter_Beta"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Beta"]; }
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Cutoff"] != null) { output["OneEuroFilter_Cutoff"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Cutoff"]; }
            if (ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Velocity_Cutoff"] != null) { output["OneEuroFilter_Velocity_Cutoff"] = (float)ApplicationData.Current.LocalSettings.Values["OneEuroFilter_Velocity_Cutoff"]; }

            if (ApplicationData.Current.LocalSettings.Values["GazePointer_Fixation_Delay"] != null) { output["GazePointer_Fixation_Delay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer_Fixation_Delay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer_Dwell_Delay"] != null) { output["GazePointer_Dwell_Delay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer_Dwell_Delay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer_Repeat_Delay"] != null) { output["GazePointer_Repeat_Delay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer_Repeat_Delay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer_Enter_Exit_Delay"] != null) { output["GazePointer_Enter_Exit_Delay"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer_Enter_Exit_Delay"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer_Max_History_Duration"] != null) { output["GazePointer_Max_History_Duration"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer_Max_History_Duration"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer_Max_Single_Sample_Duration"] != null) { output["GazePointer_Max_Single_Sample_Duration"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer_Max_Single_Sample_Duration"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazePointer_Gaze_Idle_Time"] != null) { output["GazePointer_Gaze_Idle_Time"] = (int)ApplicationData.Current.LocalSettings.Values["GazePointer_Gaze_Idle_Time"]; }

            if (ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Radius"] != null) { output["GazeCursor_Cursor_Radius"] = (int)ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Radius"]; }
            if (ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Visibility"] != null) { output["GazeCursor_Cursor_Visibility"] = (bool)ApplicationData.Current.LocalSettings.Values["GazeCursor_Cursor_Visibility"]; }
        }
    }
}
