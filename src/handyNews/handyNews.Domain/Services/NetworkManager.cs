using System;
using Windows.Networking.Connectivity;
using handyNews.Domain.Services.Interfaces;

namespace handyNews.Domain.Services
{
    public class NetworkManager : INetworkManager
    {
        public NetworkManager()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        public event EventHandler<NetworkChangedEventArgs> NetworkChanged;

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            try
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                var connected = profile != null;

                var handler = NetworkChanged;
                if (handler != null)
                {
                    handler(this, new NetworkChangedEventArgs(connected));
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception) {}
        }
    }
}