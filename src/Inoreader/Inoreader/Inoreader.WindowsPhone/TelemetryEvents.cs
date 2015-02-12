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
		public const string MarkAsRead = "MarkAsRead";
		public const string ManualRefreshSubscriptions = "ManualRefreshSubscriptions";
	}

	public static class TemetryMetrics
	{
		public const string SignInResponseTime = "SignIn ResponseTime";
		public const string GetStreamResponseTime = "GetStream ResponseTime";
		public const string GetSubscriptionsTotalResponseTime = "GetSubscriptions Total ResponseTime";
	}
}