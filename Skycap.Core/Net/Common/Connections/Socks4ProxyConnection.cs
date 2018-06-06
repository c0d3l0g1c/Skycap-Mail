namespace Skycap.Net.Common.Connections
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Configurations;
    using Skycap.Net.Common.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.IO;
    using Windows.Networking.Sockets;
    using Windows.Networking;

    public class Socks4ProxyConnection : BaseConnection
    {
        public const string AccessDeniedBySocks4ProxyMessage = "The SOCKS4 proxy server does not grant access to the requested server";
        public const int BufferSize = 0x101;
        public const string InvalidProxyTypeMessage = "The proxy type should match the connection class being used";
        public const string InvalidResponseLengthMessage = "The SOCKS4 proxy response doesn't have a valid length";
        public const string InvalidSocks4ResponseMessage = "The SOCKS4 proxy server returned an invalid response";
        public const string IPv6NotSupportedMessage = "SOCKS4 proxy servers don't support IPv6 relaying";
        public const string NullUserIDMessage = "The proxyUserID cannot be null";

        public Socks4ProxyConnection(IConfigurationProvider configurationProvider)
        {
            if (configurationProvider == null)
            {
                throw new ArgumentNullException("configurationProvider");
            }
            if (configurationProvider.ProxyUser == null)
            {
                throw new ArgumentNullException("configurationProvider.ProxyUser");
            }
            this.State = EConnectionState.Disonnected;
            this.ConfigurationProvider = configurationProvider;
        }

        public override int Available()
        {
            //TODO: return this._socket.Available;
            return -1;
        }

        public override bool CanRead(int delay)
        {
            // TODO: CanRead
            //List<Socket> checkRead = new List<Socket> {
            //    this._socket
            //};
            //Socket.Select(checkRead, null, null, delay);
            //return (checkRead.Count != 0);
            return true;
        }

        public override void Close()
        {
            if (this.State != EConnectionState.Disonnected)
            {
                base._streamForRead.Dispose();
                base._streamForWrite.Dispose();
                this.State = EConnectionState.Disonnected;
            }
        }

        public async override void Open()
        {
            if (this.ConfigurationProvider.ProxyType != EProxyType.SOCKS4)
            {
                throw new ArgumentException("The proxy type should match the connection class being used");
            }
            if (this.ConfigurationProvider.ProxyUser == null)
            {
                throw new ArgumentException("The proxyUserID cannot be null");
            }
            this._client = new StreamSocket();
            this._client.Control.KeepAlive = true;
            await this._client.ConnectAsync(new HostName(this.ConfigurationProvider.ProxyHost), this.ConfigurationProvider.ProxyPort.ToString()).AsTask().ConfigureAwait(false);
            byte[] array = new byte[0x101];
            byte[] buffer = new byte[0x101];
            int index = 0;
            array[index++] = 4;
            array[index++] = 1;
            byte[] bytes = BitConverter.GetBytes(this.ConfigurationProvider.Port);
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                array[index++] = bytes[i];
            }
            base._streamForRead = this._client.InputStream.AsStreamForRead();
            base._streamForWrite = this._client.OutputStream.AsStreamForWrite();
            if (this.ConfigurationProvider.SSLInteractionType == EInteractionType.SSLPort)
            {
                this.SwitchToSslChannel();
            }
            this.State = EConnectionState.Connected;
        }

        public virtual Encoding CredentialsEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}

