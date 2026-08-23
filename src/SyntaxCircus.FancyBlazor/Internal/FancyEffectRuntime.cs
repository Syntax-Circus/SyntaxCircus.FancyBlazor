using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace SyntaxCircus.FancyBlazor;

internal sealed class FancyEffectRuntime(
    IJSRuntime jsRuntime,
    IOptions<FancyBlazorOptions> options,
    ILogger<FancyEffectRuntime> logger) : IFancyEffectRuntime, IAsyncDisposable, IDisposable
{
    private const string ModulePath = "./_content/SyntaxCircus.FancyBlazor/js/fancy-blazor.js";
    private Task<IJSObjectReference>? _moduleTask;

    public async ValueTask<long?> CreateAsync(ElementReference element, string effect, object effectOptions)
    {
        try
        {
            var module = await GetModuleAsync().ConfigureAwait(false);
            return await module.InvokeAsync<long>(
                "createEffect",
                element,
                effect,
                effectOptions,
                CreateDefaults()).ConfigureAwait(false);
        }
        catch (JSException exception)
        {
            logger.LogWarning(exception, "FancyBlazor could not initialize the {Effect} effect; the static fallback remains active.", effect);
            return null;
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (InvalidOperationException exception)
        {
            logger.LogDebug(exception, "FancyBlazor JavaScript is not available during this render pass.");
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
            logger.LogWarning(exception, "FancyBlazor could not update effect {Handle}; the existing or static state remains active.", handle);
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
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(exception, "FancyBlazor effect {Handle} was already unavailable during disposal.", handle);
            }
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
            // Browser-side resources no longer exist.
        }
        catch (JSException exception)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(exception, "FancyBlazor runtime disposal completed after JavaScript teardown.");
            }
        }
    }

    public void Dispose()
    {
        // Synchronous service-provider disposal cannot safely call JavaScript.
        // Async Blazor scopes use DisposeAsync; a torn-down browser releases any
        // remaining module-owned resources itself.
        GC.SuppressFinalize(this);
    }

    private Task<IJSObjectReference> GetModuleAsync() =>
        _moduleTask ??= jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();

    private object CreateDefaults()
    {
        var value = options.Value;
        return new
        {
            motionPreference = value.MotionPreference.ToString(),
            quality = value.Quality.ToString(),
            pauseWhenHidden = value.PauseWhenHidden,
            pauseWhenOffscreen = value.PauseWhenOffscreen,
            enableDiagnostics = value.EnableDiagnostics,
        };
    }
}
