using Microsoft.Research.Input.Gaze;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace EyeControlToolkitSettings
{
    static class GazeSettingsHelpers
    {
        public static void GazeSettingsFromLocalSettings(GazeSettings output)
        {
            var input = ApplicationData.Current.LocalSettings;

            if (input.Values["OneEuroFilter_Beta"] != null) { output.OneEuroFilter_Beta = (float)input.Values["OneEuroFilter_Beta"]; }
            if (input.Values["OneEuroFilter_Cutoff"] != null) { output.OneEuroFilter_Cutoff = (float)input.Values["OneEuroFilter_Cutoff"]; }
            if (input.Values["OneEuroFilter_Velocity_Cutoff"] != null) { output.OneEuroFilter_Velocity_Cutoff = (float)input.Values["OneEuroFilter_Velocity_Cutoff"]; }

            if (input.Values["GazePointer_Fixation_Delay"] != null) { output.GazePointer_Fixation_Delay = (int)input.Values["GazePointer_Fixation_Delay"]; }
            if (input.Values["GazePointer_Dwell_Delay"] != null) { output.GazePointer_Dwell_Delay = (int)input.Values["GazePointer_Dwell_Delay"]; }
            if (input.Values["GazePointer_Repeat_Delay"] != null) { output.GazePointer_Repeat_Delay = (int)input.Values["GazePointer_Repeat_Delay"]; }
            if (input.Values["GazePointer_Enter_Exit_Delay"] != null) { output.GazePointer_Enter_Exit_Delay = (int)input.Values["GazePointer_Enter_Exit_Delay"]; }
            if (input.Values["GazePointer_Max_History_Duration"] != null) { output.GazePointer_Max_History_Duration = (int)input.Values["GazePointer_Max_History_Duration"]; }
            if (input.Values["GazePointer_Max_Single_Sample_Duration"] != null) { output.GazePointer_Max_Single_Sample_Duration = (int)input.Values["GazePointer_Max_Single_Sample_Duration"]; }
            if (input.Values["GazePointer_Gaze_Idle_Time"] != null) { output.GazePointer_Gaze_Idle_Time = (int)input.Values["GazePointer_Gaze_Idle_Time"]; }

            if (input.Values["GazeCursor_Cursor_Radius"] != null) { output.GazeCursor_Cursor_Radius = (int)input.Values["GazeCursor_Cursor_Radius"]; }
            if (input.Values["GazeCursor_Cursor_Visibility"] != null) { output.GazeCursor_Cursor_Visibility = (bool)input.Values["GazeCursor_Cursor_Visibility"]; }
        }

        public static void LocalSettingsFromGazeSettings(GazeSettings input)
        {
            var output = ApplicationData.Current.LocalSettings;

            output.Values["OneEuroFilter_Beta"] = input.OneEuroFilter_Beta;
            output.Values["OneEuroFilter_Cutoff"] = input.OneEuroFilter_Cutoff;
            output.Values["OneEuroFilter_Velocity_Cutoff"] = input.OneEuroFilter_Velocity_Cutoff;

            output.Values["GazePointer_Fixation_Delay"] = input.GazePointer_Fixation_Delay;
            output.Values["GazePointer_Dwell_Delay"] = input.GazePointer_Dwell_Delay;
            output.Values["GazePointer_Repeat_Delay"] = input.GazePointer_Repeat_Delay;
            output.Values["GazePointer_Enter_Exit_Delay"] = input.GazePointer_Enter_Exit_Delay;
            output.Values["GazePointer_Max_History_Duration"] = input.GazePointer_Max_History_Duration;
            output.Values["GazePointer_Max_Single_Sample_Duration"] = input.GazePointer_Max_Single_Sample_Duration;
            output.Values["GazePointer_Gaze_Idle_Time"] = input.GazePointer_Gaze_Idle_Time;

            output.Values["GazeCursor_Cursor_Radius"] = input.GazeCursor_Cursor_Radius;
            output.Values["GazeCursor_Cursor_Visibility"] = input.GazeCursor_Cursor_Visibility;
        }

        public static void ValueSetFromGazeSettings(GazeSettings input, ValueSet output)
        {
            output.Add("OneEuroFilter_Beta", input.OneEuroFilter_Beta);
            output.Add("OneEuroFilter_Cutoff", input.OneEuroFilter_Cutoff);
            output.Add("OneEuroFilter_Velocity_Cutoff", input.OneEuroFilter_Velocity_Cutoff);

            output.Add("GazePointer_Fixation_Delay", input.GazePointer_Fixation_Delay);
            output.Add("GazePointer_Dwell_Delay", input.GazePointer_Dwell_Delay);
            output.Add("GazePointer_Repeat_Delay", input.GazePointer_Repeat_Delay);
            output.Add("GazePointer_Enter_Exit_Delay", input.GazePointer_Enter_Exit_Delay);
            output.Add("GazePointer_Max_History_Duration", input.GazePointer_Max_History_Duration);
            output.Add("GazePointer_Max_Single_Sample_Duration", input.GazePointer_Max_Single_Sample_Duration);
            output.Add("GazePointer_Gaze_Idle_Time", input.GazePointer_Gaze_Idle_Time);

            output.Add("GazeCursor_Cursor_Radius", input.GazeCursor_Cursor_Radius);
            output.Add("GazeCursor_Cursor_Visibility", input.GazeCursor_Cursor_Visibility);
        }

        public static void GazeSettingsFromValueSet(ValueSet input, GazeSettings output)
        {
            if (input["OneEuroFilter_Beta"] != null) { output.OneEuroFilter_Beta = (float)input["OneEuroFilter_Beta"]; }
            if (input["OneEuroFilter_Cutoff"] != null) { output.OneEuroFilter_Cutoff = (float)input["OneEuroFilter_Cutoff"]; }
            if (input["OneEuroFilter_Velocity_Cutoff"] != null) { output.OneEuroFilter_Velocity_Cutoff = (float)input["OneEuroFilter_Velocity_Cutoff"]; }

            if (input["GazePointer_Fixation_Delay"] != null) { output.GazePointer_Fixation_Delay = (int)input["GazePointer_Fixation_Delay"]; }
            if (input["GazePointer_Dwell_Delay"] != null) { output.GazePointer_Dwell_Delay = (int)input["GazePointer_Dwell_Delay"]; }
            if (input["GazePointer_Repeat_Delay"] != null) { output.GazePointer_Repeat_Delay = (int)input["GazePointer_Repeat_Delay"]; }
            if (input["GazePointer_Enter_Exit_Delay"] != null) { output.GazePointer_Enter_Exit_Delay = (int)input["GazePointer_Enter_Exit_Delay"]; }
            if (input["GazePointer_Max_History_Duration"] != null) { output.GazePointer_Max_History_Duration = (int)input["GazePointer_Max_History_Duration"]; }
            if (input["GazePointer_Max_Single_Sample_Duration"] != null) { output.GazePointer_Max_Single_Sample_Duration = (int)input["GazePointer_Max_Single_Sample_Duration"]; }
            if (input["GazePointer_Gaze_Idle_Time"] != null) { output.GazePointer_Gaze_Idle_Time = (int)input["GazePointer_Gaze_Idle_Time"]; }

            if (input["GazeCursor_Cursor_Radius"] != null) { output.GazeCursor_Cursor_Radius = (int)input["GazeCursor_Cursor_Radius"]; }
            if (input["GazeCursor_Cursor_Visibility"] != null) { output.GazeCursor_Cursor_Visibility = (bool)input["GazeCursor_Cursor_Visibility"]; }
        }
    }
}
