using System;

namespace handyNews.Domain.Services.Interfaces
{
    public interface INetworkManager
    {
        event EventHandler<NetworkChangedEventArgs> NetworkChanged;
    }

    public class NetworkChangedEventArgs : EventArgs
    {
        public NetworkChangedEventArgs(bool connected)
        {
            Connected = connected;
        }

        public bool Connected { get; private set; }
    }
}