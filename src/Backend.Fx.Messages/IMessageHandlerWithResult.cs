using JetBrains.Annotations;

namespace Backend.Fx.Messages;

[PublicAPI]
public interface IMessageHandlerWithResult<out TResult>
{
    TResult Result { get; }
}