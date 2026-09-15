using System;
using System.CommandLine;
using System.IO;
using Cmdless.UI.Runtime.Helpers;
using Cmdless.UI.SDK.Contracts;
using Cmdless.UI.SDK.Helpers;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Cmdless.UI.Runtime;

public static class CLI
{
    static Command FactoryCommand<T>(string name, string description, Func<FileInfo, string, int> executor)
    {
        var cmd = new Command(name, description);
        var assemblyArg = cmd.Arg<FileInfo>("assembly", $"Path to assembly containing the concrete {typeof(T).Name}");
        var factoryArg = cmd.Arg<string>("factory", $"Type name used to retrieve concrete {typeof(T).Name} from assembly");
        cmd.SetAction(x => executor(x.GetRequiredValue(assemblyArg), x.GetRequiredValue(factoryArg)));
        return cmd;
    }

    public static Command AppCommand()
    {
        return FactoryCommand<ICmdlessAppFactory>(
            "app",
            "Run an application",
            (assembly, typeName) => AppRunner.RunApp(assembly.FullName, typeName));
    }

    public static Command WindowCommand()
    {
        return FactoryCommand<ICmdlessWindowFactory>(
            "window",
            "Run a Window inside a default application",
            (assembly, typeName) => AppRunner.RunWindow(assembly.FullName, typeName));
    }

    public static Command MessageBoxCommand()
    {
        var messageBoxCmd = new Command("message-box", "Show a message box");
        var textArg = messageBoxCmd.Arg<string>("text", "Message to display in message box");
        var captionOpt = messageBoxCmd.Opt("caption", "Caption to display in title bar", x => "Message");
        var buttonOpt = messageBoxCmd.Opt("button", "Buttons to display on message box", x => ButtonEnum.Ok);
        messageBoxCmd.SetAction(parseResult => AppRunner.RunAction(started: async desktop =>
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                parseResult.GetRequiredValue(captionOpt),
                parseResult.GetRequiredValue(textArg),
                parseResult.GetRequiredValue(buttonOpt));
            var result = await box.ShowWindowAsync();
            Console.WriteLine($"{result}");
            return 0;
        }));
        return messageBoxCmd;
    }

    public static RootCommand Build()
    {
        var root = new RootCommand();
        root.Subcommands.Add(AppCommand());
        root.Subcommands.Add(WindowCommand());
        root.Subcommands.Add(MessageBoxCommand());
        return root;
    }
}