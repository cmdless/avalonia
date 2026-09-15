using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Cmdless.UI.SDK.Contracts;

namespace Cmdless.UI.Runtime.Helpers;

internal static class AppRunner
{
    static Assembly LoadAssembly(string assemblyPath)
    {
        var path = Path.GetFullPath(assemblyPath);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Assembly '{path}' does not exist.",
                path);

        var context = new AppLoadContext(path);
        return context.LoadFromAssemblyPath(path);
    }

    static Type LoadFactoryType(string assemblyPath, string typeName)
    {
        var assembly = LoadAssembly(assemblyPath);
        var factoryType = assembly.GetType(typeName)
            ?? throw new InvalidOperationException($"Could not find factory type {typeName}");
        return factoryType;
    }

    static int RunFactory<T>(Type factoryType, Func<T, int> action, params object?[]? args) where T : class
    {
        if (!typeof(T).IsAssignableFrom(factoryType))
            throw new InvalidOperationException($"Type {factoryType.FullName} must be assignable to {typeof(T).Name}");
        var factory = Activator.CreateInstance(factoryType, args) as T
            ?? throw new InvalidOperationException($"Could not create instance of type {factoryType.FullName}");
        return action(factory);
    }

    static readonly Func<ICmdlessAppFactory, int> AppExecutor = factory
        => factory.Build().StartWithClassicDesktopLifetime([]);

    public static int RunApp(Type factoryType, params object?[]? args)
        => RunFactory(factoryType, AppExecutor, args);

    public static int RunApp(string assemblyPath, string typeName, params object?[]? args)
        => RunApp(LoadFactoryType(assemblyPath, typeName), args);

    public static int RunAction(AppConfigure? configure = null, AppStarted? started = null)
         => RunApp(typeof(AppFactory), configure, started);

    static readonly Func<ICmdlessWindowFactory, int> WindowExecutor = factory
        => RunAction(configure: desktop => desktop.MainWindow = factory.Create());

    public static int RunWindow(Type factoryType, params object?[]? args)
        => RunFactory(factoryType, WindowExecutor, args);

    public static int RunWindow(string assemblyPath, string typeName, params object?[]? args)
        => RunWindow(LoadFactoryType(assemblyPath, typeName), args);

    public static int RunWindow(Func<Window> factory)
        => RunWindow(typeof(WindowFactory), factory);

    public static int RunWindow<T>() where T : Window, new()
        => RunWindow(typeof(WindowFactory), () => new T());
}