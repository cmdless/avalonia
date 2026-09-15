using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cmdless.UI.SDK.Helpers;

public static class ProcessHelper
{
    public record ProcessResult(string StdOut, string StdErr, int ExitCode);

    public static async Task WriteAsync<T>(this T result) where T : ProcessResult
    {
        if (!string.IsNullOrWhiteSpace(result.StdOut))
        {
            await Console.Out.WriteAsync(result.StdOut);
            await Console.Out.FlushAsync();
        }
        if (!string.IsNullOrWhiteSpace(result.StdErr))
        {
            await Console.Error.WriteAsync(result.StdErr);
            await Console.Error.FlushAsync();
        }
    }

    public static T EnsureSuccessExitCode<T>(this T result) where T : ProcessResult
    {
        if (result.ExitCode != 0) throw new InvalidOperationException($"Exit code does not indicate success: {result.ExitCode}");
        return result;
    }

    public static async Task<ProcessResult> RunAsync(
        string exePath,
        string[]? args = null,
        bool createNoWindow = true,
        IDictionary<string, string?>? environment = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = createNoWindow,
        };
        if (args != null)
            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);
        if (environment != null)
            foreach (var pair in environment)
                startInfo.Environment.Add(pair);
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start '{exePath}'.");
        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new ProcessResult(await stdOutTask, await stdErrTask, process.ExitCode);
    }

    public static async Task<int> ForwardArgsAsync(string exePath, string[]? args = null)
    {
        var startInfo = new ProcessStartInfo { FileName = exePath, UseShellExecute = false, CreateNoWindow = true };
        if (args != null)
            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start '{exePath}'.");
        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}