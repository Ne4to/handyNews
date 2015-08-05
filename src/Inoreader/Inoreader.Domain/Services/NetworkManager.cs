﻿using System;
using Windows.Networking.Connectivity;
using Inoreader.Domain.Services.Interfaces;

namespace Inoreader.Domain.Services
{
	public class NetworkManager : INetworkManager
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
}