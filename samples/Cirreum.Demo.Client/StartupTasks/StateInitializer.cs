namespace Cirreum.Demo.Client.StartupTasks;

public class StateInitializer(
	IStateManager stateManager,
	IInitializationState initState,
	INavMenuState navMenuState
) : IStartupTask {

	public int Order => 10000;

	public async ValueTask ExecuteAsync() {

		await navMenuState.InitializeMinimalModeAsync();

		stateManager.Subscribe<IUserState>(userState => {
			if (userState.IsAuthenticated) {
				_ = this.InitializeStores(); // fire and forget...
			}
		});

	}

	private async Task InitializeStores() {

		initState.StartTask("Initializing application data");

		try {
			await Task.CompletedTask;
			//initState.SetDisplayStatus("Loading Events...");
			//await SafeLoadStore("Events", () => eventsStore.LoadEventsAsync());

			//initState.SetDisplayStatus("Loading Event Entries...");
			//await SafeLoadStore("Event Entries", () => eventEntriesStore.LoadEntriesAsync());

			//initState.SetDisplayStatus("Loading SMS Templates...");
			//await SafeLoadStore("SMS Templates", () => smsTemplatesStore.LoadTemplatesAsync());

		} finally {
			initState.CompleteTask();
		}

	}

	//private async Task SafeLoadStore(string storeName, Func<Task> loadOperation) {
	//	try {
	//		await loadOperation();
	//	} catch (Exception ex) {
	//		initState.LogError(storeName, ex);
	//		notificationState.AddNotification(Notification.Create(
	//			title: "Initialization Error",
	//			message: ex.Message,
	//			type: NotificationType.Error));
	//		// Don't rethrow - let other stores continue loading
	//	}
	//}

}