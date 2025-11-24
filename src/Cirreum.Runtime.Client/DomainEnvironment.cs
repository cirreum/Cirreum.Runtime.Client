namespace Cirreum.Runtime;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

sealed class DomainEnvironment(
	IWebAssemblyHostEnvironment hostEnvironment,
	IJSAppModule jsApp
) : IDomainEnvironment {
	public string ApplicationName { get; } = jsApp.Invoke<string>("window.appName");
	public string EnvironmentName { get; } = hostEnvironment.Environment;
	public DomainRuntimeType RuntimeType { get; } = DomainRuntimeType.BlazorWasm;
}