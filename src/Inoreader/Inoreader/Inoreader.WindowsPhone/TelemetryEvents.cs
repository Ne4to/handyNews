namespace Inoreader
{
	public static class TelemetryEvents
	{
		public const string HardwareButtonsBackPressed = "HardwareButtonsBackPressed";
		public const string SignIn = "SignIn";
		public const string Review = "Review";
		public const string SubmitBug = "SubmitBug";
		public const string Contribute = "Contribute";
		public const string ChangeDisplayCulture = "ChangeDisplayCulture";
		public const string ChangeHideEmptySubscriptions = "ChangeHideEmptySubscriptions";
		public const string ChangeShowOrder = "ChangeShowOrder";
		public const string ChangeFontSize = "ChangeFontSize";
		public const string ChangeStreamView = "ChangeStreamView";
		public const string MarkAsRead = "MarkAsRead";
		public const string MarkAsStarred = "MarkAsStarred";		
		public const string ManualRefreshSubscriptions = "ManualRefreshSubscriptions";
		public const string ManualRefreshStream = "ManualRefreshStream";
		public const string MarkAllAsRead = "MarkAllAsRead";
		public const string OpenItemInWeb = "OpenItemInWeb";
		public const string LoadSubscriptionsFromCache = "Cache.LoadSubscriptions";
		public const string LoadStreamFromCache = "Cache.LoadStream";
	}

	public static class TemetryMetrics
	{
		public const string SignInResponseTime = "SignIn ResponseTime";
		public const string GetStreamResponseTime = "GetStream ResponseTime";
		public const string GetSubscriptionsTotalResponseTime = "GetSubscriptions Total ResponseTime";
	}
}