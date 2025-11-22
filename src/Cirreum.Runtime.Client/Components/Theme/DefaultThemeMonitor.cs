namespace Cirreum.Components.Theme;

using Cirreum;
using Cirreum.Components.Interop;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

public sealed class DefaultThemeMonitor(
	IThemeState themeState,
	IThemeStateManager themeManager,
	IJSAppModule js) : IThemeMonitor {

	private bool _initialized;

	public ValueTask InitializeAsync() {

		if (this._initialized) {
			return ValueTask.CompletedTask;
		}
		this._initialized = true;

		// Restore persisted preferences for Mode
		var storedMode = this.GetStoredMode();
		if (string.IsNullOrEmpty(storedMode)) {
			storedMode = ThemeModeNames.Auto;
		}

		// Restore persisted preferences for Theme
		var storedScheme = this.GetStoredScheme();
		if (string.IsNullOrEmpty(storedScheme)) {
			storedScheme = ThemeNames.Default;
		}

		// Apply restored preferences
		if (Enum.TryParse<ThemeMode>(storedMode, true, out var mode)) {
			themeManager.SetMode(mode);
		}

		if (Enum.TryParse<ThemeName>(storedScheme, true, out var theme)) {
			themeManager.SetTheme(theme);
		}

		// Start monitoring system changes
		this.MonitorSystemThemeChanges();

		return ValueTask.CompletedTask;
	}

	public void RefreshAppliedMode() {
		if (themeState.Mode == ThemeModeNames.Auto) {
			var systemTheme = js.GetSystemThemeMode();
			js.SetElementAttribute("html", "data-bs-theme", systemTheme);
			themeState.SetAppliedMode(systemTheme);
		}
	}

	private void MonitorSystemThemeChanges() {
		var themeMonitor = new ThemeModeMonitorRef((isDarkMode, storedMode) => {
			if (storedMode == ThemeModeNames.Auto) {
				js.SetElementAttribute("html", "data-bs-theme", isDarkMode ? "" : "");
			}
			return Task.CompletedTask;
		});

		var themeMonitorRef = DotNetObjectReference.Create(themeMonitor);
		js.MonitorSystemThemeMode(themeMonitorRef);
	}

	private string GetStoredMode() =>
		(js.Invoke<string?>("localStorage.getItem", StorageKeys.ModeKey) ?? "").ToLowerInvariant();

	private string GetStoredScheme() =>
		(js.Invoke<string?>("localStorage.getItem", StorageKeys.ThemeKey) ?? "").ToLowerInvariant();

	private record ThemeModeMonitorRef : ISystemThemeChangedRef {
		[DynamicDependency(nameof(ThemeChanged))]
		public ThemeModeMonitorRef(Func<bool, string, Task>? onThemeChanged) {
			this.OnThemeModeChanged = onThemeChanged;
		}

		public Func<bool, string, Task>? OnThemeModeChanged { get; }

		[JSInvokable("OnThemeChange")]
		public async Task ThemeChanged(bool isDarkMode, string storedMode) {
			if (this.OnThemeModeChanged is not null) {
				await this.OnThemeModeChanged.Invoke(isDarkMode, storedMode);
			}
		}
	}
}

//public sealed class DefaultThemeMonitor(
//	IThemeState themeState,
//	IJSAppModule js
//) : IThemeMonitor {

//	private bool _initialized;

//	public ValueTask InitializeAsync() {
//		if (this._initialized) {
//			return ValueTask.CompletedTask; // Prevent reinitialization
//		}
//		this._initialized = true;

//		var storedTheme = this.GetStoredTheme();
//		if (storedTheme.IsEmpty()) {
//			storedTheme = "auto";
//		}

//		this.SetTheme(storedTheme);
//		this.MonitorSystemThemeChanges();
//		return ValueTask.CompletedTask;

//	}

//	private void MonitorSystemThemeChanges() {
//		var themeMonitor = new ThemeMonitorRef((isDarkMode, storedTheme) => {
//			if (storedTheme == "auto") {
//				js.SetElementAttribute("html", "data-bs-theme", isDarkMode ? "dark" : "light");
//			}
//			return Task.CompletedTask;
//		});
//		var themeMonitorRef = DotNetObjectReference.Create(themeMonitor);
//		js.MonitorSystemTheme(themeMonitorRef);
//	}
//	private string GetStoredTheme() {
//		return (js.Invoke<string?>("localStorage.getItem", "theme") ?? "").ToLowerInvariant();
//	}
//	private void SetStoredTheme(string theme) {
//		js.InvokeVoid("localStorage.setItem", "theme", theme.ToLowerInvariant());
//	}

//	public void SetTheme(string theme) {

//		using var scope = themeState.CreateNotificationScope();

//		var loweredTheme = theme.ToLowerInvariant();

//		this.SetStoredTheme(loweredTheme);

//		var appliedTheme = loweredTheme;
//		if (loweredTheme == "auto") {
//			themeState.SetSelectedTheme("auto");
//			appliedTheme = js.GetSystemTheme();
//		} else {
//			themeState.SetSelectedTheme(loweredTheme);
//		}

//		js.SetElementAttribute("html", "data-bs-theme", appliedTheme);

//		themeState.SetAppliedTheme(appliedTheme);

//	}

//	const string ON_THEME_CHANGED = "OnThemeChange";
//	private record ThemeMonitorRef {

//		[DynamicDependency(nameof(ThemeChanged))]
//		public ThemeMonitorRef(Func<bool, string, Task>? OnThemeChanged) {
//			this.OnThemeChanged = OnThemeChanged;
//		}

//		public Func<bool, string, Task>? OnThemeChanged { get; }

//		[JSInvokable(ON_THEME_CHANGED)]
//		public async Task ThemeChanged(bool isDarkMode, string storedTheme) {
//			if (this.OnThemeChanged is not null) {
//				await this.OnThemeChanged.Invoke(isDarkMode, storedTheme);
//			}
//		}
//	};

//}