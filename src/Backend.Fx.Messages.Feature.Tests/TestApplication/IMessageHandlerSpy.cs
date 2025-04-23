using System.Security.Principal;

namespace Backend.Fx.Messages.Feature.Tests.TestApplication;

public interface IMessageHandlerSpy
{
    Task<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation);
    
    Task HandleAsync(MyMessage message, CancellationToken cancellation);
    
    Task InitializeAsync(CancellationToken cancellationToken);
}