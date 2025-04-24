using System.Reflection;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Messages.Feature;

public class MessageHandlingModule : IModule
{
    private readonly Assembly[] _assemblies;
    
    public MessageHandlingModule(Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        var messageHandlerRegistry = new MessageHandlerRegistry();
        
        var serviceDescriptors = _assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsImplementationOfOpenGenericInterface(typeof(IMessageHandler<>)))
            .Select(type => new ServiceDescriptor(type, type, ServiceLifetime.Scoped));

        foreach (var serviceDescriptor in serviceDescriptors)
        {
            var messageType = serviceDescriptor.ImplementationType!.GetTypeInfo()
                .ImplementedInterfaces
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .GenericTypeArguments.First();

            messageHandlerRegistry.Add(messageType, serviceDescriptor.ServiceType);
            compositionRoot.Register(serviceDescriptor);
        }
        
        compositionRoot.Register(ServiceDescriptor.Singleton<IMessageHandlerRegistry>(messageHandlerRegistry));
    }
}
