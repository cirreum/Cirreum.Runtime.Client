namespace Cirreum.Components.Theme;

public sealed class DefaultThemeStateManager(
	IThemeState themeState,
	IJSAppModule js
) : IThemeStateManager {

	public void SetMode(ThemeMode mode) {
		using var scope = themeState.CreateNotificationScope();

		var modeString = mode.ToShortName(); // "light", "dark", "auto"
		this.SetStoredMode(modeString);

		var appliedMode = modeString;
		if (mode == ThemeMode.Auto) {
			themeState.SetMode(modeString);
			appliedMode = js.GetSystemThemeMode(); // "light" or "dark"
		} else {
			themeState.SetMode(modeString);
		}

		js.SetElementAttribute("html", "data-bs-theme", appliedMode);
		themeState.SetAppliedMode(appliedMode);
	}

	public void SetTheme(ThemeName theme) {
		using var scope = themeState.CreateNotificationScope();

		var themeString = theme.ToShortName(); // "default", "aspire", etc.
		this.SetStoredScheme(themeString);

		js.SetElementAttribute("html", "data-color-scheme", themeString);
		themeState.SetTheme(themeString);
	}

	private void SetStoredMode(string mode) =>
		js.InvokeVoid("localStorage.setItem", StorageKeys.ModeKey, mode);

	private void SetStoredScheme(string scheme) =>
		js.InvokeVoid("localStorage.setItem", StorageKeys.ThemeKey, scheme);

}