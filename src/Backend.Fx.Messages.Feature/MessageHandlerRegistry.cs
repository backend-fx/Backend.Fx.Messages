using System.Collections.Concurrent;

namespace Backend.Fx.Messages.Feature;

public interface IMessageHandlerRegistry
{
    IEnumerable<Type> GetCommandHandlerTypes(Type commandType);
}

public class MessageHandlerRegistry : IMessageHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, List<Type>> _commandHandlers = new();
    
    public IEnumerable<Type> GetCommandHandlerTypes(Type commandType)
    {
        return _commandHandlers.GetOrAdd(commandType, _ => []);
    }

    public void Add(Type commandType, Type implementingType)
    {
        _commandHandlers.GetOrAdd(commandType, _ => []).Add(implementingType);
    }
}