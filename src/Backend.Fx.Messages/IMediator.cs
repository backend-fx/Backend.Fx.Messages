using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Messages;

[PublicAPI]
public interface IMediator
{
    ValueTask InvokeAsync(
        object message,
        IIdentity? identity = null,
        CancellationToken cancellation = default);

    ValueTask<TResult> InvokeAsync<TResult>(
        object message,
        IIdentity? identity = null,
        CancellationToken cancellation = default);

    ValueTask SendAsync<TMessage>(
        TMessage message,
        IIdentity? identity = null,
        Action<TMessage, IIdentity, Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class;

    ValueTask PublishAsync<TMessage>(
        TMessage message,
        IIdentity? identity = null,
        Action<TMessage, IIdentity, Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class;
}
