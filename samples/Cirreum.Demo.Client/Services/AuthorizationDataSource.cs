namespace Cirreum.Demo.Client.Services;

/// <summary>
/// Indicates the source of authorization data
/// </summary>
public enum AuthorizationDataSource {
	/// <summary>
	/// Data is computed locally using the AuthorizationModel
	/// </summary>
	Client,

	/// <summary>
	/// Data is fetched from a remote API
	/// </summary>
	Api,

	/// <summary>
	/// Data source is currently unknown or not yet determined
	/// </summary>
	Unknown
}