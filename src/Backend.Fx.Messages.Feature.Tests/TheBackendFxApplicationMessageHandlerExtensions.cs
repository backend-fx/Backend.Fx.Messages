using System.Security.Principal;
using Backend.Fx.Exceptions;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Backend.Fx.Messages.Feature.Tests.TestApplication;
using Backend.Fx.Util;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Messages.Feature.Tests;

public class TheBackendFxApplicationMessageHandlerExtensions
{
    private readonly MyMessage _message = new("Name1", 1, DateTime.UtcNow);
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly IMessageHandlerSpy _commandHandlerSpy = A.Fake<IMessageHandlerSpy>();

    private readonly int _handlerCount = typeof(TheBackendFxApplicationMessageHandlerExtensions)
        .Assembly
        .GetTypes()
        .Count(t => t.IsImplementationOfOpenGenericInterface(typeof(IMessageHandler<>)));

    private readonly BackendFxApplication _app;

    public TheBackendFxApplicationMessageHandlerExtensions()
    {
        _app = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _app.EnableFeature(new MessageHandlingFeature());
        _app.CompositionRoot.Register(ServiceDescriptor.Singleton(_commandHandlerSpy));
    }
    
    [Fact]
    public async Task CallsAllHandlers()
    {
        A.CallTo(() => _commandHandlerSpy.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._)).Returns(true);
        
        await _app.BootAsync();
        await _app.PublishAsync(_message);

        A.CallTo(() => _commandHandlerSpy.HandleAsync(A<MyMessage>.That.IsSameAs(_message), A<CancellationToken>._))
            .MustHaveHappened(_handlerCount, Times.Exactly);
    }
    
    [Fact]
    public async Task Calls_IsAuthorized_OnAuthorizedHandlers()
    {
        A.CallTo(() => _commandHandlerSpy.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._)).Returns(true);

        await _app.BootAsync();
        await _app.PublishAsync(_message);

        A.CallTo(() => _commandHandlerSpy.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _commandHandlerSpy.HandleAsync(A<MyMessage>.That.IsSameAs(_message), A<CancellationToken>._))
            .MustHaveHappened(_handlerCount, Times.Exactly);
    }

    [Fact]
    public async Task UnauthorizedHandlerThrowsForbiddenException()
    {
        A.CallTo(() => _commandHandlerSpy.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._)).Returns(false);
        await _app.BootAsync();
        await _app.PublishAsync(_message);

        A.CallTo(() => _exceptionLogger.LogException(A<ForbiddenException>._)).MustHaveHappenedOnceExactly();

//        A.CallTo(() => commandHandlerSpy.HandleAsync(A<MyCommand>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Calls_Initialize_OnInitializableHandler()
    {
        A.CallTo(() => _commandHandlerSpy.IsAuthorizedAsync(A<IIdentity>._, A<CancellationToken>._)).Returns(true);
        
        await _app.BootAsync();
        await _app.PublishAsync(_message);

        A.CallTo(() => _commandHandlerSpy.InitializeAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        
        A.CallTo(() => _commandHandlerSpy.HandleAsync(A<MyMessage>.That.IsSameAs(_message), A<CancellationToken>._))
            .MustHaveHappened(_handlerCount, Times.Exactly);
    }
}