namespace Cirreum.Demo.Client.State;

public record InitializationError {
	public string StoreName { get; init; } = "";
	public Exception Exception { get; init; } = null!;
	public string ErrorMessage { get; init; } = "";
	public string? StackTrace { get; init; }
	public DateTime Timestamp { get; init; }
}