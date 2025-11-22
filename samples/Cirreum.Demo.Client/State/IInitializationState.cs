namespace Cirreum.Demo.Client.State;

public interface IInitializationState : IScopedNotificationState {

	bool IsInitializing { get; }

	void StartTask(string status);
	void CompleteTask();
	int GetTaskCount();

	string DisplayStatus { get; }
	void SetDisplayStatus(string status);

	IReadOnlyList<InitializationError> Errors { get; }
	bool HasErrors { get; }
	int ErrorCount { get; }
	void LogError(string storeName, Exception exception);
	void ClearErrors();

}