namespace Cirreum.Runtime.StartupTasks;

using Cirreum.Runtime.Security;

sealed class ConfigureUserState(
	IUserState user,
	IJSAppModule jsApp
) : IStartupTask {

	int IStartupTask.Order => int.MinValue;

	ValueTask IStartupTask.ExecuteAsync() {
		if (user is ClientUser currentUser) {
			var authType = jsApp.GetAuthInfo();
			if (authType.Include) {
				if (authType.AuthType.Equals("oidc", StringComparison.OrdinalIgnoreCase)) {
					currentUser.SetAuthenticationLibrary(AuthenticationLibraryType.OIDC);
				} else if (authType.AuthType.Equals("msal", StringComparison.OrdinalIgnoreCase)) {
					currentUser.SetAuthenticationLibrary(AuthenticationLibraryType.MSAL);
				}
			}
		}

		return ValueTask.CompletedTask;
	}

}