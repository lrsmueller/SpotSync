using BytexDigital.Blazor.Components.CookieConsent;

namespace TablerCookieConsent;
public class TablerCookieConsentPromptVariant : CookieConsentPromptVariantBase
{
	public override Type ComponentType { get; set; } = typeof(TablerCookieConsentPrompt);
}
