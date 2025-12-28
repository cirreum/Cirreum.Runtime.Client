namespace Cirreum.Demo.Client;

using Microsoft.JSInterop;

public class MermaidService(
	IJSRuntime jsRuntime
) : IMermaidService
  , IAsyncDisposable {

	private IJSObjectReference? _mermaidModule;
	private bool _isInitialized = false;

	public bool IsInitialized => this._isInitialized;

	public async Task InitializeAsync() {
		if (this._isInitialized) {
			return;
		}
		this._isInitialized = true;

		this._mermaidModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/mermaid-service.js");
		await this._mermaidModule.InvokeVoidAsync("initializeMermaid");

	}

	public async Task<string> RenderDiagramAsync(string diagramDefinition, string? theme = null) {
		if (!this._isInitialized) {
			await this.InitializeAsync();
		}

		if (this._mermaidModule == null) {
			throw new InvalidOperationException("Mermaid service not initialized");
		}

		return await this._mermaidModule.InvokeAsync<string>("renderDiagram", diagramDefinition, theme);
	}

	public async Task ClearDiagramAsync(string diagramId) {
		if (this._mermaidModule != null) {
			await this._mermaidModule.InvokeVoidAsync("clearDiagram", diagramId);
		}
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);
		if (this._mermaidModule != null) {
			await this._mermaidModule.DisposeAsync();
		}
	}

}