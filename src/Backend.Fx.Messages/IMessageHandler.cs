using JetBrains.Annotations;

namespace Backend.Fx.Messages;

public interface IMessageHandler;

[PublicAPI]
public interface IMessageHandler<in TMessage> : IMessageHandler where TMessage : class
{
    Task HandleAsync(TMessage command, CancellationToken cancellation = default);
}