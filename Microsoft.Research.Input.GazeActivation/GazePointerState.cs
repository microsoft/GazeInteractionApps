namespace Microsoft.Research.Input.Gaze
{
    public enum GazePointerState
    {
        Exit,

        // The order of the following elements is important because
        // they represent states that linearly transition to their
        // immediate successors. 
        PreEnter,
        Enter,
        Fixation,
        Dwell,
        //FixationRepeat,
        DwellRepeat
    }
}
