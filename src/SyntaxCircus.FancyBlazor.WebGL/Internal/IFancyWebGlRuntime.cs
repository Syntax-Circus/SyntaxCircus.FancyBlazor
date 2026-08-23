using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

internal interface IFancyWebGlRuntime
{
    ValueTask<long?> CreateAsync(ElementReference element, string effect, object options);

    ValueTask<bool> UpdateAsync(long handle, object options);

    ValueTask DestroyAsync(long handle);
}
