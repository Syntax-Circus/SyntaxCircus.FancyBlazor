using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace SyntaxCircus.FancyBlazor;

internal sealed class FancyWebGlRuntime(
    IJSRuntime jsRuntime,
    IOptions<FancyWebGlOptions> options,
    IOptions<FancyBlazorOptions> fancyBlazorOptions,
    ILogger<FancyWebGlRuntime> logger) : IFancyWebGlRuntime, IAsyncDisposable, IDisposable
{
    private const string ModulePath = "./_content/SyntaxCircus.FancyBlazor.WebGL/js/fancy-blazor-webgl.js";
    private Task<IJSObjectReference>? _moduleTask;
    private bool _loggedFailure;

    public async ValueTask<long?> CreateAsync(ElementReference element, string effect, object effectOptions)
    {
        try
        {
            var module = await GetModuleAsync().ConfigureAwait(false);
            return await module.InvokeAsync<long>("createEffect", element, effect, effectOptions, CreateDefaults()).ConfigureAwait(false);
        }
        catch (JSException exception)
        {
            LogFailure(exception, effect);
            return null;
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (InvalidOperationException exception)
        {
            LogFailure(exception, effect);
            return null;
        }
    }

    public async ValueTask<bool> UpdateAsync(long handle, object effectOptions)
    {
        try
        {
            var module = await GetModuleAsync().ConfigureAwait(false);
            await module.InvokeVoidAsync("updateEffect", handle, effectOptions).ConfigureAwait(false);
            return true;
        }
        catch (JSException exception)
        {
            LogFailure(exception, handle.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return false;
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
    }

    public async ValueTask DestroyAsync(long handle)
    {
        try
        {
            if (_moduleTask is null)
            {
                return;
            }

            var module = await _moduleTask.ConfigureAwait(false);
            await module.InvokeVoidAsync("destroyEffect", handle).ConfigureAwait(false);
        }
        catch (JSDisconnectedException)
        {
            // The browser has already released the page and its resources.
        }
        catch (JSException exception)
        {
            LogFailure(exception, handle.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is null)
        {
            return;
        }

        try
        {
            var module = await _moduleTask.ConfigureAwait(false);
            await module.InvokeVoidAsync("disposeRuntime").ConfigureAwait(false);
            await module.DisposeAsync().ConfigureAwait(false);
        }
        catch (JSDisconnectedException)
        {
            // The browser has already released the page and its resources.
        }
        catch (JSException exception)
        {
            LogFailure(exception, "runtime");
        }
    }

    public void Dispose() => GC.SuppressFinalize(this);

    private Task<IJSObjectReference> GetModuleAsync() =>
        _moduleTask ??= jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();

    private object CreateDefaults()
    {
        var shared = fancyBlazorOptions.Value;
        return new
        {
            motionPreference = shared.MotionPreference.ToString(),
            quality = shared.Quality.ToString(),
            pauseWhenHidden = shared.PauseWhenHidden,
            pauseWhenOffscreen = shared.PauseWhenOffscreen,
            maxActiveContexts = options.Value.MaxActiveContexts,
        };
    }

    private void LogFailure(Exception exception, string subject)
    {
        if (_loggedFailure)
        {
            return;
        }

        _loggedFailure = true;
        logger.LogWarning(exception, "FancyBlazor WebGL could not initialize or update {Subject}; the CSS fallback remains active.", subject);
    }
}
