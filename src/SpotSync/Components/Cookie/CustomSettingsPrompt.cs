using BytexDigital.Blazor.Components.CookieConsent;

namespace SpotSync.Components.Cookie;

public class CustomSettingsPrompt : CookieConsentPromptVariantBase
{
	public override Type ComponentType { get; set; } = typeof(TablerCookieConsentPrompt);
}


//public class CookieConsentPreference : CookieConsentPreferencesVariantBase
//{
//	public override Type ComponentType { get; set; } = typeof(TablerCookieConsentPreference);
//}
