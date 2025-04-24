using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;

namespace Backend.Fx.Messages.Feature;

public class MessageHandlingFeature : IFeature
{
    public void Enable(IBackendFxApplication application)
    {
        var messageHandlingModule = new MessageHandlingModule(new Mediator(application), application.Assemblies);
        application.CompositionRoot.RegisterModules(messageHandlingModule);
    }
}