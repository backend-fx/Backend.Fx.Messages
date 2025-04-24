namespace Backend.Fx.Messages;

public interface IMessageHandlerRegistry
{
    IEnumerable<Type> GetMessageHandlerTypes(Type messageType);
}