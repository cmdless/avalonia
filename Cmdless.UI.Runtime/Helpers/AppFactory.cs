using System;
using Avalonia;
using Cmdless.UI.SDK.Contracts;

namespace Cmdless.UI.Runtime.Helpers;

internal class AppFactory : ICmdlessAppFactory
{
    readonly Func<App> factory;

    public AppFactory(AppConfigure? configure = null, AppStarted? started = null)
    {
        factory = () => new App(configure, started);
    }

    public AppBuilder Build()
        => AppBuilder.Configure(factory)
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}