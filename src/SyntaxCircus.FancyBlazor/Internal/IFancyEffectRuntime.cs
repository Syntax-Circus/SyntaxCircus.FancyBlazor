using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

internal interface IFancyEffectRuntime
{
    ValueTask<long?> CreateAsync(ElementReference element, string effect, object options);

    ValueTask<bool> UpdateAsync(long handle, object options);

    ValueTask DestroyAsync(long handle);
}
