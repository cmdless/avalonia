using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Cmdless.UI.SDK.Helpers;

public static class CommandLineExtensions
{
    public static Argument<T> Arg<T>(
        this Command command,
        string name,
        string? description = null,
        Func<ArgumentResult, T>? defaultValueFactory = null)
    {
        var arg = new Argument<T>(name)
        {
            Description = description,
            DefaultValueFactory = defaultValueFactory
        };
        command.Arguments.Add(arg);
        return arg;
    }

    public static Option<T> Opt<T>(
        this Command command,
        string name,
        string? description = null,
        Func<ArgumentResult, T>? defaultValueFactory = null)
    {
        var opt = new Option<T>($"--{name}")
        {
            Description = description,
            DefaultValueFactory = defaultValueFactory
        };
        command.Options.Add(opt);
        return opt;
    }
}