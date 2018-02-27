namespace Microsoft.Research.Input.Gaze
{
    /// <summary>
    /// Basic filter which performs no input filtering -- easy to use as a default filter.
    /// </summary>
    public sealed class NullFilter : IGazeFilter
    {
        GazeEventArgs IGazeFilter.Update(GazeEventArgs args)
        {
            return args;
        }
    }
}
