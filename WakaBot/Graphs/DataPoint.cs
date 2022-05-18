namespace WakaBot.Graphs;

public class DataPoint<T>
{
    public string label { get; private set; }
    public T value { get; private set; }

    public DataPoint(string label, T value)
    {
        this.label = label;
        this.value = value;
    }
}