using CaterinGO.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CaterinGO.Components.Layout;

public partial class NavMenu : IDisposable
{
    [Inject]
    protected LanguageState LanguageState { get; set; } = default!;

    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;

    protected override void OnInitialized()
    {
        LanguageState.Changed += StateHasChanged;
    }

    protected async Task SetLanguageAsync(string language)
    {
        LanguageState.Set(language);
        await JsRuntime.InvokeVoidAsync("localStorage.setItem", "cateringo-language", LanguageState.Current);
        await JsRuntime.InvokeVoidAsync("eval", $"document.documentElement.lang = '{LanguageState.Current}';");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var storedLanguage = await JsRuntime.InvokeAsync<string?>("localStorage.getItem", "cateringo-language");
        if (!string.IsNullOrWhiteSpace(storedLanguage))
        {
            LanguageState.Set(storedLanguage);
            await JsRuntime.InvokeVoidAsync("eval", $"document.documentElement.lang = '{LanguageState.Current}';");
        }
    }

    public void Dispose()
    {
        LanguageState.Changed -= StateHasChanged;
    }
}
