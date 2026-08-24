using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;
using SyntaxCircus.FancyBlazor.StandaloneHost;
using SyntaxCircus.FancyBlazor.TestHost;
using Xunit;

namespace SyntaxCircus.FancyBlazor.BrowserTests;

/// <summary>Launches compiled test hosts so browser tests remain independent of repository paths.</summary>
public sealed class BrowserHostFixture : IAsyncLifetime
{
    private Process? _testHostProcess;
    private Task<string>? _testHostStandardError;
    private Task<string>? _testHostStandardOutput;

    public IPlaywright Playwright { get; private set; } = default!;

    public IBrowser Browser { get; private set; } = default!;

    public string TestHostUrl { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        try
        {
            TestHostUrl = GetAvailableUrl();
            (_testHostProcess, _testHostStandardOutput, _testHostStandardError) = StartAssembly(typeof(TestHostAssemblyMarker).Assembly.Location, TestHostUrl);
            await WaitUntilReadyAsync(TestHostUrl, _testHostProcess, _testHostStandardOutput, _testHostStandardError).ConfigureAwait(false);
            await VerifyTestHostAssetsAsync(TestHostUrl).ConfigureAwait(false);

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            }).ConfigureAwait(false);
        }
        catch
        {
            StopProcess(_testHostProcess);
            _testHostProcess = null;
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync().ConfigureAwait(false);
        }

        Playwright?.Dispose();
        StopProcess(_testHostProcess);
        _testHostProcess = null;
    }

    public static Process StartStandaloneHost(out string hostUrl)
    {
        hostUrl = GetAvailableUrl();
        var standaloneAssembly = typeof(StandaloneHostAssemblyMarker).Assembly.Location;
        var devServer = GetDevServerToolPath(OperatingSystem.IsWindows() ? "blazor-devserver.exe" : "blazor-devserver.dll");
        var startInfo = new ProcessStartInfo(OperatingSystem.IsWindows() ? devServer : "dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(standaloneAssembly)!,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        if (!OperatingSystem.IsWindows())
        {
            startInfo.ArgumentList.Add(devServer);
        }
        startInfo.ArgumentList.Add("--applicationpath");
        startInfo.ArgumentList.Add(standaloneAssembly);
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(hostUrl);

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the standalone WebAssembly test host.");
    }

    public static Process StartServerHost(string assemblyPath, out string hostUrl)
    {
        hostUrl = GetAvailableUrl();
        return StartAssembly(assemblyPath, hostUrl).Process;
    }

    public static async Task WaitUntilReadyAsync(
        string baseUrl,
        Process? process = null,
        Task<string>? standardOutput = null,
        Task<string>? standardError = null)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        Exception? lastError = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (process?.HasExited == true)
            {
                throw new InvalidOperationException($"Host {baseUrl} exited with code {process.ExitCode} before becoming ready.{await GetHostOutputAsync(standardOutput, standardError).ConfigureAwait(false)}");
            }

            try
            {
                using var response = await client.GetAsync(baseUrl).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException exception)
            {
                lastError = exception;
            }
            catch (TaskCanceledException exception)
            {
                lastError = exception;
            }

            await Task.Delay(250).ConfigureAwait(false);
        }

        StopProcess(process);
        throw new TimeoutException($"Host {baseUrl} did not become ready.{await GetHostOutputAsync(standardOutput, standardError).ConfigureAwait(false)}", lastError);
    }

    private static (Process Process, Task<string> StandardOutput, Task<string> StandardError) StartAssembly(string assemblyPath, string url)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(assemblyPath)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        startInfo.ArgumentList.Add(assemblyPath);
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(url);
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the compiled browser test host.");
        return (process, process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
    }

    private static async Task VerifyTestHostAssetsAsync(string baseUrl)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var response = await client.GetAsync($"{baseUrl}/_content/SyntaxCircus.FancyBlazor/js/fancy-blazor.js").ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"The browser test host did not serve FancyBlazor's JavaScript asset (HTTP {(int)response.StatusCode}).");
        }
    }

    private static string GetAvailableUrl()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        return $"http://127.0.0.1:{port}";
    }

    private static string GetDevServerToolPath(string toolName)
    {
        var packageRoot = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (string.IsNullOrWhiteSpace(packageRoot))
        {
            packageRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        }

        var packageDirectory = Path.Combine(packageRoot, "microsoft.aspnetcore.components.webassembly.devserver");
        var candidate = Directory.EnumerateDirectories(packageDirectory)
            .OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Select(versionDirectory => Path.Combine(versionDirectory, "tools", toolName))
            .FirstOrDefault(File.Exists);

        return candidate ?? throw new FileNotFoundException($"The Blazor WebAssembly development-server tool '{toolName}' was not restored.", packageDirectory);
    }

    private static void StopProcess(Process? process)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(10_000);
            }
        }
        catch (InvalidOperationException)
        {
            // Fixture initialization may already have released a failed process.
        }
        finally
        {
            process.Dispose();
        }
    }

    private static async Task<string> GetHostOutputAsync(Task<string>? standardOutput, Task<string>? standardError)
    {
        if (standardOutput is null || standardError is null)
        {
            return string.Empty;
        }

        var output = await standardOutput.ConfigureAwait(false);
        var error = await standardError.ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(output) && string.IsNullOrWhiteSpace(error)
            ? string.Empty
            : $"{Environment.NewLine}Host output:{Environment.NewLine}{output}{error}";
    }
}
