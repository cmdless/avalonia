using System;
using Avalonia.Controls;
using Cmdless.UI.SDK.Contracts;

namespace Cmdless.UI.Runtime.Helpers;

public class WindowFactory : ICmdlessWindowFactory
{
    readonly Func<Window> _factory;
    public WindowFactory(Func<Window> factory) => _factory = factory;
    public Window Create() => _factory();
}