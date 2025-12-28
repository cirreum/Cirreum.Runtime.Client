namespace Cirreum.Demo.Client;

public interface IMermaidService {
	Task InitializeAsync();
	Task<string> RenderDiagramAsync(string diagramDefinition, string? theme = null);
	Task ClearDiagramAsync(string diagramId);
	bool IsInitialized { get; }
}