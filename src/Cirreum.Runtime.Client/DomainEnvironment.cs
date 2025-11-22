namespace Cirreum.Runtime;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

sealed class DomainEnvironment(
	IWebAssemblyHostEnvironment hostEnvironment,
	IJSAppModule jsApp
) : IDomainEnvironment {
	public string ApplicationName => jsApp.Invoke<string>("window.appName");
	public string EnvironmentName => hostEnvironment.Environment;
	public DomainRuntimeType RuntimeType { get; } = DomainRuntimeType.BlazorWasm;
}