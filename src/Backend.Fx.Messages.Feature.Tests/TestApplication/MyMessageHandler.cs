using System.Security.Principal;

namespace Backend.Fx.Messages.Feature.Tests.TestApplication;

public class MyMessageHandler(IMessageHandlerSpy spy) : IMessageHandler<MyMessage>
{
    public Task HandleAsync(MyMessage message, CancellationToken cancellation)
    {
        return spy.HandleAsync(message, cancellation);
    }
}

public class MyAuthorizedMessageHandler(IMessageHandlerSpy spy) : IMessageHandler<MyMessage>, IAuthorizedMessageHandler
{
    public Task HandleAsync(MyMessage message, CancellationToken cancellation)
    {
        return spy.HandleAsync(message, cancellation);
    }

    public Task<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation = default)
    {
        return spy.IsAuthorizedAsync(identity, cancellation);
    }
}

public class MyInitializableMessageHandler(IMessageHandlerSpy spy) : IMessageHandler<MyMessage>, IInitializableMessageHandler
{
    public Task HandleAsync(MyMessage message, CancellationToken cancellation)
    {
        return spy.HandleAsync(message, cancellation);
    }

    public Task InitializeAsync(CancellationToken cancellation = default)
    {
        return spy.InitializeAsync(cancellation);
    }
}