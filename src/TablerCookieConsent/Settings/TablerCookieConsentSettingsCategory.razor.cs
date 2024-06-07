using BytexDigital.Blazor.Components.CookieConsent;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace TablerCookieConsent.Settings;

public partial class TablerCookieConsentSettingsCategory
{

    [Inject]
    protected IOptions<CookieConsentOptions> Options { get; set; }

    [Inject]
    public CookieConsentLocalizer Localizer { get; set; }

    [Parameter]
    public CookieCategory Category { get; set; }

    [Parameter]
    public bool Selected { get; set; }

    [Parameter]
    public EventCallback<bool> SelectedChanged { get; set; }

    private string MainKey => $"{Category.Identifier}-svg-container";

    private bool Collapsed { get; set; } = true;

    private async Task OnChangedAsync(ChangeEventArgs args)
    {
        Selected = (bool)args.Value;
        await SelectedChanged.InvokeAsync(Selected);
    }

}
