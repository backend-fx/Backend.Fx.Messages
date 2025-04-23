using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Messages;

[PublicAPI]
public interface IAuthorizedMessageHandler
{
    Task<bool> IsAuthorizedAsync(IIdentity identity, CancellationToken cancellation = default);
}