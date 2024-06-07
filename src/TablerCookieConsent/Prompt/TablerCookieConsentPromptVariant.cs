using BytexDigital.Blazor.Components.CookieConsent;
using TablerCookieConsent.Prompt;

namespace TablerCookieConsent;
public class TablerCookieConsentPromptVariant : CookieConsentPromptVariantBase
{
    public override Type ComponentType { get; set; } = typeof(TablerCookieConsentPrompt);

    public bool SecondaryActionOpensSettings { get; set; } = false;

}
