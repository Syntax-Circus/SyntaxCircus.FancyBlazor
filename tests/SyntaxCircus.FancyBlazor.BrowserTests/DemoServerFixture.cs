using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace SyntaxCircus.FancyBlazor.BrowserTests;

public sealed class DemoServerFixture : IAsyncLifetime
{
    private Process? _demoProcess;

    public const string DemoUrl = "http://127.0.0.1:5187";

    public IPlaywright Playwright { get; private set; } = default!;

    public IBrowser Browser { get; private set; } = default!;

    public string RepositoryRoot { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        RepositoryRoot = FindRepositoryRoot();
        var project = Path.Combine(RepositoryRoot, "samples", "FancyBlazor.Demo", "FancyBlazor.Demo.csproj");
        _demoProcess = StartDotNetProject(project, DemoUrl);
        await WaitUntilReadyAsync(DemoUrl).ConfigureAwait(false);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        }).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync().ConfigureAwait(false);
        }
        Playwright?.Dispose();
        StopProcess(_demoProcess);
    }

    public Process StartStandaloneHost() => StartDotNetProject(
        Path.Combine(RepositoryRoot, "tests", "SyntaxCircus.FancyBlazor.StandaloneHost", "SyntaxCircus.FancyBlazor.StandaloneHost.csproj"),
        "http://127.0.0.1:5191");

    public static async Task WaitUntilReadyAsync(string baseUrl)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        Exception? lastError = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
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

        throw new TimeoutException($"Host {baseUrl} did not become ready.", lastError);
    }

    public static void StopProcess(Process? process)
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
        finally
        {
            process.Dispose();
        }
    }

    private static Process StartDotNetProject(string project, string url)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            Arguments = $"run --project \"{project}\" --no-build --configuration Release --urls {url}",
            WorkingDirectory = Path.GetDirectoryName(project)!,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {project}.");
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "SyntaxCircus.FancyBlazor.slnx")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the FancyBlazor repository root.");
    }
}
