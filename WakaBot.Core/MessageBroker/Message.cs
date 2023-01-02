namespace WakaBot.Core.MessageBroker;

#pragma warning disable CS8618
public class Message<T> where T : class
{
    public T Payload { get; set; }
    public string Channel { get; set; }
}
#pragma warning restore CS8618