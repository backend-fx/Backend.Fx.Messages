namespace Backend.Fx.Messages;

public interface IMessageHandler;

public interface IMessageHandler<in TMessage> : IMessageHandler where TMessage : class
{
    Task HandleAsync(TMessage command, CancellationToken cancellation = default);
}