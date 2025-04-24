using System.Collections.Concurrent;

namespace Backend.Fx.Messages.Feature;

public class MessageHandlerRegistry : IMessageHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, List<Type>> _messageHandlers = new();

    public IEnumerable<Type> GetMessageHandlerTypes(Type messageType)
    {
        return _messageHandlers.GetOrAdd(messageType, _ => []);
    }

    public void Add(Type messageType, Type implementingType)
    {
        _messageHandlers.GetOrAdd(messageType, _ => []).Add(implementingType);
    }
}