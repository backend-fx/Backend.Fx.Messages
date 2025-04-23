using System.Reflection;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Messages.Feature;

public class MessageHandlingModule : IModule
{
    private readonly Assembly[] _assemblies;
    private readonly MessageHandlerRegistry _messageHandlerRegistry;


    public MessageHandlingModule(MessageHandlerRegistry messageHandlerRegistry, Assembly[] assemblies)
    {
        _messageHandlerRegistry = messageHandlerRegistry;
        _assemblies = assemblies;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        var serviceDescriptors = _assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsImplementationOfOpenGenericInterface(typeof(IMessageHandler<>)))
            .Select(type => new ServiceDescriptor(type, type, ServiceLifetime.Scoped));

        foreach (var serviceDescriptor in serviceDescriptors)
        {
            var commandType = serviceDescriptor.ImplementationType!.GetTypeInfo()
                .ImplementedInterfaces
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .GenericTypeArguments.First();

            _messageHandlerRegistry.Add(commandType, serviceDescriptor.ServiceType);
            compositionRoot.Register(serviceDescriptor);
        }
    }
}
