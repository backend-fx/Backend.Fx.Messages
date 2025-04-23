using System.Security.Principal;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Messages.Feature;

public static class BackendFxApplicationMessageHandlingExtensions
{
    /// <summary>
    /// Invokes a single handler. Use this method, when there must be exactly one handler for the message.
    /// A possible exception is propagated to the caller.
    /// </summary>
    public static async Task InvokeAsync<TMessage>(
        this IBackendFxApplication application,
        TMessage message,
        IIdentity? identity = null,
        CancellationToken cancellation = default) where TMessage : class
    {
        var handlerType = application.GetSingleHandlerType<TMessage>();
        await application.InvokeHandler(message, identity, cancellation, handlerType);
    }

    /// <summary>
    /// Invokes a single handler and return the result of the handler. Use this method, when there must
    /// be exactly one handler for the message, and this one is known for returning a result.
    /// A possible exception is propagated to the caller.
    /// </summary>
    public static async Task<TResult> InvokeAsync<TMessage, TResult>(
        this IBackendFxApplication application,
        TMessage message,
        IIdentity? identity = null,
        CancellationToken cancellation = default) where TMessage : class
    {
        var handlerType = application.GetSingleHandlerType<TMessage>();

        if (handlerType.GetInterfaces().Any(ift => ift == typeof(IMessageHandlerWithResult<TResult>)))
        {
            var handler = await application.InvokeHandler(message, identity, cancellation, handlerType);
            // ReSharper disable once SuspiciousTypeConversion.Global
            return ((IMessageHandlerWithResult<TResult>)handler!).Result;
        }

        throw new InvalidOperationException($"Handler {handlerType.Name} has no result of type {typeof(TResult).Name}");
    }

    /// <summary>
    /// Handles a message. Use this method, when you expect at least one handler to exist and to handle the message.
    /// An optional delegate can be provided to handle exceptions that occur during the handling of the message.
    /// </summary>
    public static Task SendAsync<TMessage>(
        this IBackendFxApplication application,
        TMessage message,
        IIdentity? identity = null,
        Action<TMessage, IIdentity, Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class
        => PublishAsync(application, true, message, identity, onException, cancellation);

    /// <summary>
    /// Fire and forget a message. Use this method, when you have neither an assumption on the existence of a handler
    /// nor any expectation of a result.
    /// An optional delegate can be provided to handle exceptions that occur during the handling of the message.
    /// </summary>
    public static Task PublishAsync<TMessage>(
        this IBackendFxApplication application,
        TMessage message,
        IIdentity? identity = null,
        Action<TMessage, IIdentity, Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class
        => PublishAsync(application, false, message, identity, onException, cancellation);

    private static async Task PublishAsync<TMessage>(
        this IBackendFxApplication application,
        bool expectAtLeastOneHandler,
        TMessage message,
        IIdentity? identity = null,
        Action<TMessage, IIdentity, Exception>? onException = null,
        CancellationToken cancellation = default) where TMessage : class
    {
        var handlerTypes = application.GetHandlerTypes<TMessage>();

        if (expectAtLeastOneHandler && handlerTypes.Length == 0)
        {
            throw new InvalidOperationException($"No handler types for {typeof(TMessage).Name} found.");
        }

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                await InvokeHandler(application, message, identity, cancellation, handlerType);
            }
            catch (Exception ex)
            {
                onException?.Invoke(message, identity ?? new AnonymousIdentity(), ex);
            }
        }
    }

    private static async Task<IMessageHandler?> InvokeHandler<TMessage>(
        this IBackendFxApplication application,
        TMessage message,
        IIdentity? identity,
        CancellationToken cancellation, Type handlerType) where TMessage : class
    {
        IMessageHandler? handler = null;
        await application.Invoker.InvokeAsync(async (sp, ct) =>
        {
            handler = (IMessageHandler)sp.GetRequiredService(handlerType);

            var handleAsyncMethod = handler.GetType().GetMethod(nameof(IMessageHandler<TMessage>.HandleAsync));
            if (handleAsyncMethod == null)
            {
                throw new InvalidOperationException(
                    $"Method {nameof(IMessageHandler<TMessage>.HandleAsync)} not found on {handler.GetType()}");
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (handler is IInitializableMessageHandler initializableCommandHandler)
            {
                await initializableCommandHandler.InitializeAsync(ct).ConfigureAwait(false);
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (handler is IAuthorizedMessageHandler authorizedCommand)
            {
                var isAuthorized = await authorizedCommand.IsAuthorizedAsync(
                    identity ?? new AnonymousIdentity(),
                    cancellation).ConfigureAwait(false);

                if (isAuthorized == false)
                {
                    throw new ForbiddenException("You are not authorized to perform this action");
                }
            }

            var task = (Task?)handleAsyncMethod.Invoke(handler, [message, ct]);
            await (task ?? Task.CompletedTask);
        }, identity, cancellation).ConfigureAwait(false);

        return handler;
    }

    private static Type GetSingleHandlerType<TMessage>(this IBackendFxApplication application) where TMessage : class
    {
        var handlerTypes = application.GetHandlerTypes<TMessage>();
        
        if (handlerTypes.Length == 0)
        {
            throw new InvalidOperationException($"No handlers for {typeof(TMessage).Name} found.");
        }

        if (handlerTypes.Length > 1)
        {
            throw new InvalidOperationException($"More than one handler for {typeof(TMessage).Name} found.");
        }

        return handlerTypes.Single();
    }
    
    private static Type[] GetHandlerTypes<TMessage>(this IBackendFxApplication application) where TMessage : class
    {
        var messageHandlingFeature = application.GetFeature<MessageHandlingFeature>();
        if (messageHandlingFeature == null)
        {
            throw new InvalidOperationException("Message handling feature is not enabled.");
        }

        var handlerTypes = messageHandlingFeature
            .MessageHandlerRegistry
            .GetCommandHandlerTypes(typeof(TMessage))
            .ToArray();
        
        return handlerTypes;
    }
}