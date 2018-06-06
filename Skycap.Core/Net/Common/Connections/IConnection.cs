namespace Skycap.Net.Common.Connections
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Configurations;
    using System;
    using Windows.Networking.Sockets;

    public interface IConnection
    {
        int Available();
        bool CanRead(int delay);
        void Close();
        void Open();
        byte[] ReceiveBytes();
        byte[] ReceiveBytes(ulong count);
        string ReceiveLine();
        void SendLine(string textLine);
        void SwitchToSslChannel();

        IConfigurationProvider ConfigurationProvider { get; }

        EConnectionState State { get; }
    }
}

