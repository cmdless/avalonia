using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Cmdless.UI.Runtime.Helpers;

internal sealed class AppLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public AppLoadContext(string assemblyPath)
        : base(isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(assemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var loaded = Default.Assemblies.FirstOrDefault(
            assembly => AssemblyName.ReferenceMatchesDefinition(
                assembly.GetName(),
                assemblyName));

        if (loaded is not null)
            return loaded;

        var path = _resolver.ResolveAssemblyToPath(assemblyName);

        return path is null
            ? null
            : LoadFromAssemblyPath(path);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        return path is null
            ? nint.Zero
            : LoadUnmanagedDllFromPath(path);
    }
}