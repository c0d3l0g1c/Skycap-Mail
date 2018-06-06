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

    public class Socks5ProxyConnection : BaseConnection
    {
        public const string AuthorizationMethodNotSupportedMessage = "None of authorization methods was accepted by the proxy server";
        public const int BufferSize = 0x101;
        public const string InvalidCredentialsMessage = "Bad Username or Password.";
        public const string InvalidProxyTypeMessage = "The proxy type should match the connection class being used";
        public const string InvalidResponseMessage = "Bad response received from the proxy server.";
        public const string InvalidUseProxyValueMessage = "The UseProxy parameter must be true to use a proxy connection";
        public const string IpV6NotSupportedMessage = "IPv6-only hosts isn't supported";
        public const string NullProxyPasswordMessage = "The proxyPassword cannot be null";
        public const string NullProxyUsernameMessage = "The proxyUsername cannot be null";

        public Socks5ProxyConnection(IConfigurationProvider configurationProvider)
        {
            if (configurationProvider == null)
            {
                throw new ArgumentNullException("configurationProvider");
            }
            if (configurationProvider.ProxyUser == null)
            {
                throw new ArgumentNullException("The proxyUsername cannot be null");
            }
            if (configurationProvider.ProxyPassword == null)
            {
                throw new ArgumentNullException("The proxyPassword cannot be null");
            }
            this.ConfigurationProvider = configurationProvider;
        }

        public override int Available()
        {
            //TODO:return this._socket.Available;
            return 1;
        }

        public override bool CanRead(int delay)
        {
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
            if (this.ConfigurationProvider.ProxyType != EProxyType.SOCKS5)
            {
                throw new InvalidOperationException("The proxy type should match the connection class being used");
            }
            if (this.ConfigurationProvider.ProxyUser == null)
            {
                throw new ArgumentNullException("The proxyUsername cannot be null");
            }
            if (this.ConfigurationProvider.ProxyPassword == null)
            {
                throw new ArgumentNullException("The proxyPassword cannot be null");
            }
            byte[] buffer = new byte[0x101];
            byte[] buffer2 = new byte[0x101];
            this._client = new StreamSocket();
            this._client.Control.KeepAlive = true;
            await this._client.ConnectAsync(new HostName(this.ConfigurationProvider.ProxyHost), this.ConfigurationProvider.ProxyPort.ToString());
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

