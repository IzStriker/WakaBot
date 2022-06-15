namespace WakaBot.Graphs;

/// <summary>
/// Representation of of a data point for a graph.
/// Data points cannot be modified.
/// </summary>
public class DataPoint<T>
{
    public string label { get; private set; }
    public T value { get; private set; }

    /// <summary>
    /// Create instance of immutable data point.
    /// </summary>
    /// <param name="label">Name of data point.</param>
    /// <param name="value">Value of data point</param>
    public DataPoint(string label, T value)
    {
        this.label = label;
        this.value = value;
    }
}