namespace Microsoft.Research.Input.Gaze
{
    /// <summary>
    /// Every filter must provide an Wpdate method which transforms sample data and returns filtered output
    /// </summary>
    public interface IGazeFilter
    {
        GazeEventArgs Update(GazeEventArgs args);
    }
}
