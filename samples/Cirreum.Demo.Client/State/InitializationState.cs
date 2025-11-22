namespace Cirreum.Demo.Client.State;

public class InitializationState(
	IStateManager stateManager
) : ScopedNotificationState, IInitializationState {

	private int _initializerCount = 0;
	public bool IsInitializing => this._initializerCount > 0;
	public void StartTask(string status) {
		this._initializerCount++;
		this.DisplayStatus = status;
		this.NotifyStateChanged();
	}
	public int GetTaskCount() => this._initializerCount;

	public string DisplayStatus { get; private set; } = "";
	public void SetDisplayStatus(string status) {
		this.DisplayStatus = status;
		this.NotifyStateChanged();
	}

	public void CompleteTask() {
		if (this._initializerCount > 0) {
			this._initializerCount--;
			if (this._initializerCount == 0) {
				this.DisplayStatus = "";
			}
			this.NotifyStateChanged();
		}
	}

	private readonly List<InitializationError> _errors = [];
	public IReadOnlyList<InitializationError> Errors => this._errors.AsReadOnly();
	public bool HasErrors => this._errors.Count != 0;
	public int ErrorCount => this._errors.Count;
	public void LogError(string storeName, Exception exception) {
		this._errors.Add(new InitializationError {
			StoreName = storeName,
			Exception = exception,
			Timestamp = DateTime.Now,
			ErrorMessage = exception.Message,
			StackTrace = exception.StackTrace
		});
		this.NotifyStateChanged();
	}

	public void ClearErrors() {
		this._errors.Clear();
		this.NotifyStateChanged();
	}

	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IInitializationState>(this);
	}

}