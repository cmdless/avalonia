using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cmdless.UI.SDK.Contracts;

namespace Cmdless.UI.SDK.Helpers;

public static class CmdlessLoader
{
    public static string HomePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string DefaultInstallPath => Path.Combine(HomePath, ".cmdless", "dotnet", "tools");

    public static async Task<string> GetLatestVersionAsync(string packageId)
    {
        var result = await ProcessHelper.RunAsync("dotnet", [
            "tool",
            "search",
            packageId,
            "--detail"
        ]);

        await result.WriteAsync();
        result.EnsureSuccessExitCode();

        var match = Regex.Match(
            result.StdOut,
            $@"^{Regex.Escape(packageId)}\r?\nLatest Version:\s*(\S+)\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        if (!match.Success)
            throw new InvalidOperationException($"Could not find package '{packageId}' using dotnet tool search");

        return match.Groups[1].Value;
    }

    public record ToolInstallParams(string PackageId, string? CommandName = null, string? Version = null, string? InstallPath = null)
    {
        public static implicit operator ToolInstallParams(string packageId) => new ToolInstallParams(packageId);
    }

    static string RequiredAssemblyMetadataValue(string key)
        => typeof(CmdlessLoader).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(x => x.Key == key).Value!;

    public static string GetRuntimePackage() => RequiredAssemblyMetadataValue("CmdlessRuntimePackage");

    public static string GetRuntimeVersion() => RequiredAssemblyMetadataValue("CmdlessRuntimeVersion");

    public static ToolInstallParams GetRuntimeInstallParams(string? installPath = null)
        => new ToolInstallParams(GetRuntimePackage(), null, GetRuntimeVersion(), installPath);

    public static async Task<string> EnsureToolInstalledAsync(ToolInstallParams parameters)
    {
        var packageId = parameters.PackageId;
        var version = parameters.Version;

        // a version given in the package id takes precedence
        if (packageId.Contains('@'))
        {
            var split = packageId.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (split.Length != 2)
                throw new InvalidOperationException($"Invalid package ID: '{packageId}'");

            packageId = split[0];
            version = split[1];
        }

        // if we still don't have a version, grab the latest
        if (string.IsNullOrWhiteSpace(version))
            version = await GetLatestVersionAsync(packageId);

        var commandName = parameters.CommandName;
        if (string.IsNullOrWhiteSpace(commandName))
            commandName = packageId;

        var installPath = parameters.InstallPath;
        if (string.IsNullOrWhiteSpace(installPath))
            installPath = DefaultInstallPath;

        var toolPath = Path.Combine(installPath, packageId, version);
        var execPath = Path.Combine(toolPath, OperatingSystem.IsWindows() ? $"{commandName}.exe" : commandName);
        if (File.Exists(execPath)) return execPath;

        Directory.CreateDirectory(toolPath);
        var result = await ProcessHelper.RunAsync("dotnet", [
            "tool",
            "install",
            packageId,
            "--version",
            version,
            "--tool-path",
            toolPath
        ]);

        await result.WriteAsync();
        result.EnsureSuccessExitCode();

        if (!File.Exists(execPath))
            throw new InvalidOperationException($"Tool '{packageId}' installed but executable '{execPath}' was not found.");

        return execPath;
    }

    public static async Task<int> RunToolAsync(ToolInstallParams parameters, string[]? args = null)
    {
        var execPath = await EnsureToolInstalledAsync(parameters);
        return await ProcessHelper.ForwardArgsAsync(execPath, args);
    }

    public record RunParams(string? InstallPath = null);

    public static async Task<int> RunAsync(RunParams? parameters = null, string[]? args = null)
    {
        var execPath = await EnsureToolInstalledAsync(GetRuntimeInstallParams(parameters?.InstallPath));
        return await ProcessHelper.ForwardArgsAsync(execPath, args);
    }

    public static Task<int> RunAppAsync<T>(RunParams? parameters = null) where T : ICmdlessAppFactory
        => RunAsync(parameters, ["app", typeof(T).Assembly.Location, typeof(T).FullName!]);

    public static Task<int> RunWindowAsync<T>(RunParams? parameters = null) where T : ICmdlessWindowFactory
        => RunAsync(parameters, ["window", typeof(T).Assembly.Location, typeof(T).FullName!]);
}