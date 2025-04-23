using JetBrains.Annotations;

namespace Backend.Fx.Messages;

[PublicAPI]
public interface IInitializableMessageHandler
{
    Task InitializeAsync(CancellationToken cancellation = default);
}
