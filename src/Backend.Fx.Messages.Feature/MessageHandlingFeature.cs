using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;

namespace Backend.Fx.Messages.Feature;

public class MessageHandlingFeature : IFeature
{
    private readonly MessageHandlerRegistry _messageHandlerRegistry = new();

    public IMessageHandlerRegistry MessageHandlerRegistry => _messageHandlerRegistry;

    public void Enable(IBackendFxApplication application)
    {
        var messageHandlingModule = new MessageHandlingModule(_messageHandlerRegistry, [GetType().Assembly]);
        application.CompositionRoot.RegisterModules(messageHandlingModule);
    }
}