using System.Security.Principal;
using Backend.Fx.Execution;

namespace Backend.Fx.Messages.Feature;

public class Mediator : IMediator
{
    private readonly IBackendFxApplication _application;

    public Mediator(IBackendFxApplication application)
    {
        _application = application;
    }

    public ValueTask InvokeAsync(
        object message,
        IIdentity? identity = null,
        CancellationToken cancellation = default)
        => _application.InvokeAsync(message, identity, cancellation);

    public ValueTask<TResult> InvokeAsync<TResult>(
        object message,
        IIdentity? identity = null,
        CancellationToken cancellation = default)
        => _application.InvokeAsync<TResult>(message, identity, cancellation);

    public ValueTask SendAsync<TMessage>(
        TMessage message,
        IIdentity? identity = null,
        Action<TMessage, IIdentity, Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class
        => _application.SendAsync(message, identity, onException, cancellation);

    public ValueTask PublishAsync<TMessage>(
        TMessage message,
        IIdentity? identity = null,
        Action<TMessage, IIdentity, Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class
        => _application.PublishAsync(message, identity, onException, cancellation);
}
