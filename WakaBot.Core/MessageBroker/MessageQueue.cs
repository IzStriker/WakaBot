using System.Collections.Concurrent;

namespace WakaBot.Core.MessageBroker;

public class MessageQueue
{
    private ConcurrentDictionary<string, List<Func<object, Task>>> _channelSubscriptions;

    public MessageQueue()
    {
        _channelSubscriptions = new ConcurrentDictionary<string, List<Func<object, Task>>>();
    }

    public void Subscribe<T>(string channel, Func<T, Task> action) where T : class
    {
        if (!_channelSubscriptions.ContainsKey(channel))
        {
            _channelSubscriptions[channel] = new List<Func<object, Task>>();
        }
        _channelSubscriptions[channel].Add(async (msg) => await action((T)msg));
    }

    public void Send<T>(string channel, T message) where T : class
    {
        if (_channelSubscriptions.ContainsKey(channel))
        {
            foreach (var action in _channelSubscriptions[channel])
            {
                Task.Run(() => action(message));
            }
        }
    }
}