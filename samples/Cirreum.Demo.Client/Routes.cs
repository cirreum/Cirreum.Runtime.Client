namespace Cirreum.Demo.Client;

public static class Routes {

	public const string Index = "/";
	public const string IndexIcon = "house";
	public const string IndexName = "Home";

	public const string Counter = "/counter";
	public const string CounterIcon = "plus-square-fill";
	public const string CounterName = "Counter";

	public const string Weather = "/weather";
	public const string WeatherIcon = "list-nested";
	public const string WeatherName = "Weather";

	//browser-info
	public const string BrowserInfo = "/browser-info";
	public const string BrowserInfoIcon = "browser-edge";
	public const string BrowserInfoName = "Browser Formats";

	//timezone-info
	public const string TimeZoneInfo = "/timezone-tests";
	public const string TimeZoneIcon = "calendar2-check";
	public const string TimeZoneName = "Time-Zone";


	public const string Authentication = "authentication";
	public const string Login = $"{Authentication}/login";
	public const string Logout = $"{Authentication}/logout";
	public const string LogoutIcon = "box-arrow-right";
	public const string LogoutName = "Logout";

	public const string TermsOfService = "terms";
	public const string PrivacyPolicy = "privacy";
	public const string Unauthorized = "unauthorized";
	public const string Error = "error";

	// Communications Folder routes
	public static class Communications {

		public const string Root = "communications";

		public const string Index = $"{Root}/";
		public const string IndexIcon = "chat-left-text";
		public const string IndexName = "Communications";

	}

	// User Folder routes
	public static class Users {

		public const string Root = "users";
		public const string Index = $"{Root}/";

		/// <summary>
		/// The user's profile page
		/// </summary>
		public const string Profile = $"{Root}/profile";
		public const string ProfileIcon = "person-gear";
		public const string ProfileName = "Profile";

	}

	// Management Folder routes
	public static class Management {

		public const string Root = "manage";

		/// <summary>
		/// Index page...
		/// </summary>
		public const string Index = $"{Root}/";
		public const string IndexIcon = "building-gear";
		public const string IndexName = "Manage";

		/// <summary>
		/// All Events Index page...
		/// </summary>
		public const string Events = $"{Root}/events";
		public const string EventsIcon = "calendar-event";
		public const string EventsName = "All Events";

		/// <summary>
		/// All Entries Index page...
		/// </summary>
		public const string Entries = $"{Root}/entries";
		public const string EntriesIcon = "ticket-detailed-fill";
		public const string EntriesName = "All Entries";

		/// <summary>
		/// Sms Templates Index page...
		/// </summary>
		public const string SmsTemplates = $"{Root}/smstemplates";
		public const string SmsTemplatesIcon = "file-text";
		public const string SmsTemplatesName = "Sms Templates";

	}

}