using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Messages;

[PublicAPI]
public interface IBackendFxMessageBus
{
    Task InvokeAsync<TMessage>(
        TMessage command,
        IIdentity? identity = null,
        Action<Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class;
    
    Task<TResult> InvokeAsync<TMessage, TResult>(
        TMessage command,
        IIdentity? identity = null,
        Action<Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class;
    
    Task PublishAsync<TMessage>(
        TMessage command,
        IIdentity? identity = null,
        Action<Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class;
    
    Task SendAsync<TMessage>(
        TMessage command,
        IIdentity? identity = null,
        Action<Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class;
}