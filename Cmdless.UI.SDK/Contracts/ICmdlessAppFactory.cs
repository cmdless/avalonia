using Avalonia;

namespace Cmdless.UI.SDK.Contracts;

public interface ICmdlessAppFactory
{
    AppBuilder Build();
}