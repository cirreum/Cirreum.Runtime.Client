namespace Cirreum.Components.Theme;

public interface IThemeIconNameProvider : IThemeIconProvider {
	string ResolveModeIcon(string mode);
	string ResolveThemeIcon(string theme);
}

public class DefaultThemeIconProvider : IThemeIconNameProvider {

	public string ResolveModeIcon(ThemeMode mode) => mode switch {
		ThemeMode.Light => "bi-sun-fill",
		ThemeMode.Dark => "bi-moon-stars-fill",
		ThemeMode.Auto => "bi-circle-half",
		_ => "bi-question-circle"
	};

	public string ResolveThemeIcon(ThemeName theme) => theme switch {
		ThemeName.Default => "bi-palette-fill",
		ThemeName.Aspire => "bi-stars",
		ThemeName.Excel => "bi-file-earmark-excel-fill",
		ThemeName.Office => "bi-microsoft",
		ThemeName.Outlook => "bi-envelope-fill",
		ThemeName.Windows => "bi-windows",
		ThemeName.Aqua => "bi-apple",
		_ => "bi-palette"
	};

	public string ResolveModeIcon(string mode) => mode.ToLowerInvariant() switch {
		ThemeModeNames.Light => "bi-sun-fill",
		ThemeModeNames.Dark => "bi-moon-stars-fill",
		ThemeModeNames.Auto => "bi-circle-half",
		_ => "bi-question-circle"
	};

	public string ResolveThemeIcon(string theme) => theme.ToLowerInvariant() switch {
		ThemeNames.Default => "bi-palette-fill",
		ThemeNames.Aspire => "bi-stars",
		ThemeNames.Excel => "bi-file-earmark-excel-fill",
		ThemeNames.Office => "bi-microsoft",
		ThemeNames.Outlook => "bi-envelope-fill",
		ThemeNames.Windows => "bi-windows",
		ThemeNames.Aqua => "bi-apple",
		_ => "bi-palette"
	};

}