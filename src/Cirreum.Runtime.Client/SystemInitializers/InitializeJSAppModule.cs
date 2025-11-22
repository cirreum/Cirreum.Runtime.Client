namespace Cirreum.Runtime.SystemInitializers;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

internal class InitializeJSAppModule : ISystemInitializer {
	public async ValueTask RunAsync(IServiceProvider serviceProvider) {
		var appModule = serviceProvider.GetRequiredService<IJSAppInterop>();
		await appModule.InitializeAsync();
	}
}