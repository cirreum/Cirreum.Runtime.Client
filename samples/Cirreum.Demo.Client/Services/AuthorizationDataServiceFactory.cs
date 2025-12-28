namespace Cirreum.Demo.Client.Services;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Factory for resolving authorization data services by source type.
/// Allows using both Client and API services in the same application.
/// </summary>
public class AuthorizationDataServiceFactory(IServiceProvider serviceProvider) {

	/// <summary>
	/// Gets the authorization data service for the specified source.
	/// </summary>
	public IAuthorizationDataService GetService(AuthorizationDataSource source) {
		return source switch {
			AuthorizationDataSource.Client => serviceProvider.GetRequiredService<ClientAuthorizationDataService>(),
			AuthorizationDataSource.Api => serviceProvider.GetRequiredService<ApiAuthorizationDataService>(),
			_ => this.GetDefaultService()
		};
	}

	/// <summary>
	/// Gets the default authorization data service (Client if available, otherwise API).
	/// </summary>
	public IAuthorizationDataService GetDefaultService() {
		// Prefer client-side if available
		var clientService = serviceProvider.GetService<ClientAuthorizationDataService>();
		if (clientService?.IsAvailable == true) {
			return clientService;
		}

		var apiService = serviceProvider.GetService<ApiAuthorizationDataService>();
		if (apiService?.IsAvailable == true) {
			return apiService;
		}

		// Fallback to client service even if not fully configured
		return clientService ?? throw new InvalidOperationException(
			"No authorization data service is available. Register either ClientAuthorizationDataService or ApiAuthorizationDataService.");
	}

	/// <summary>
	/// Tries to get the authorization data service for the specified source.
	/// </summary>
	public bool TryGetService(AuthorizationDataSource source, out IAuthorizationDataService? service) {
		try {
			service = source switch {
				AuthorizationDataSource.Client => serviceProvider.GetService<ClientAuthorizationDataService>(),
				AuthorizationDataSource.Api => serviceProvider.GetService<ApiAuthorizationDataService>(),
				_ => null
			};
			return service?.IsAvailable == true;
		} catch {
			service = null;
			return false;
		}
	}

	/// <summary>
	/// Gets all available authorization data services.
	/// </summary>
	public IEnumerable<IAuthorizationDataService> GetAvailableServices() {
		var clientService = serviceProvider.GetService<ClientAuthorizationDataService>();
		if (clientService?.IsAvailable == true) {
			yield return clientService;
		}

		var apiService = serviceProvider.GetService<ApiAuthorizationDataService>();
		if (apiService?.IsAvailable == true) {
			yield return apiService;
		}
	}
}
