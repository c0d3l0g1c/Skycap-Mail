namespace Skycap.Net.Imap
{
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Runtime.CompilerServices;
    using Skycap.Net.Imap.Events;
    using Windows.Networking.Sockets;

    public interface IInteractDispatcher
    {
        event EventHandler<ServerResponseReceivedEventArgs> ServerResponseReceived;

        void Close();
        void GetAccess();
        void GetMonopolyAccess();
        byte[] GetRawData();
        byte[] GetRawData(ulong size);
        IMAP4Response GetResponse(uint commandId);
        void Open();
        void ReleaseAccess();
        void ReleaseMonopolyAccess();
        uint SendCommand(string command);
        uint SendCommand(string command, ResponseFilter filter);
        uint SendContinuationCommand(string command);
        void SwitchToSslChannel();
    }
}

