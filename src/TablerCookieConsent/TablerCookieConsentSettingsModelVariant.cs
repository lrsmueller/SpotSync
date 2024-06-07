using BytexDigital.Blazor.Components.CookieConsent;

namespace TablerCookieConsent;

public class TablerCookieConsentSettingsModelVariant : CookieConsentPreferencesVariantBase
{
	public override Type ComponentType { get; set; } = typeof(TablerCookieConsentSettingsModal);
}
