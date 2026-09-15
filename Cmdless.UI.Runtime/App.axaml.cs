using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cmdless.UI.SDK.Contracts;

namespace Cmdless.UI.Runtime;

public delegate void AppConfigure(IClassicDesktopStyleApplicationLifetime desktop);
public delegate Task<int> AppStarted(IClassicDesktopStyleApplicationLifetime desktop);

public partial class App : Application
{
    private readonly AppConfigure? _configure;
    private readonly AppStarted? _started;

    public App() { }
    public App(AppConfigure? configure = null, AppStarted? started = null)
    {
        _configure = configure;
        _started = started;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _configure?.Invoke(desktop);

            if (_started is not null)
            {
                desktop.Startup += async (_, _) =>
                {
                    var exitCode = 0;
                    try
                    {
                        exitCode = await _started(desktop);
                    }
                    finally
                    {
                        desktop.Shutdown(exitCode);
                    }
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}