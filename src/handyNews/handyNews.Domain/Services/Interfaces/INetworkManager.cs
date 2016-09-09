using System;

namespace handyNews.Domain.Services.Interfaces
{
    public interface INetworkManager
    {
        event EventHandler<NetworkChangedEventArgs> NetworkChanged;
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