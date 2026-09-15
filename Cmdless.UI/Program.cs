using Cmdless.UI.SDK.Helpers;

namespace Cmdless.UI.Runtime;

class Program
{
    public static Task<int> Main(string[] args)
        => CmdlessLoader.RunAsync(args: args);
}
