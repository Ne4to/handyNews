using System;
using Windows.Networking.Connectivity;

namespace Inoreader.Services
{
	public class NetworkManager
	{
		public event EventHandler<NetworkChangedEventArgs> NetworkChanged;

		public NetworkManager()
		{
			NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
		}

		void NetworkInformation_NetworkStatusChanged(object sender)
		{
			try
			{
				var profile = NetworkInformation.GetInternetConnectionProfile();
				var connected = profile != null;

				var handler = NetworkChanged;
				if (handler != null)
					handler(this, new NetworkChangedEventArgs(connected));
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch (Exception) { }
		}
	}

	public class NetworkChangedEventArgs : EventArgs
	{
		public bool Connected { get; private set; }

		public NetworkChangedEventArgs(bool connected)
		{
			Connected = connected;
		}
	}
}