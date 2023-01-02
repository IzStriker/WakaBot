using System.Collections.Concurrent;
using System.Reflection;

namespace WakaBot.Core.MessageBroker;

public class MessageQueue
{
    private readonly BlockingCollection<object> _queue;

    public MessageQueue()
    {
        _queue = new BlockingCollection<object>();
    }

    public void Subscribe<T>(string channel, Func<T, Task> action) where T : class
    {
        Task.Run(() =>
        {
            foreach (var message in _queue.GetConsumingEnumerable())
            {
                if (message is Message<T> && ((Message<T>)message).Channel == channel)
                {
                    action(((Message<T>)message).Payload);
                }
            }
        });
    }

    public void Send<T>(string channel, T message) where T : class
    {
        _queue.Add(new Message<T>
        {
            Channel = channel,
            Payload = message
        });
    }
}