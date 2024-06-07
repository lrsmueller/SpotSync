using BytexDigital.Blazor.Components.CookieConsent;
using TablerCookieConsent.Settings;

namespace TablerCookieConsent;
public class TablerCookieConsentSettingsModelVariant : CookieConsentPreferencesVariantBase
{
	public override Type ComponentType { get; set; } = typeof(TablerCookieConsentSettingsModal);
}
