namespace Skycap.Net.Common.Connections
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Configurations;
    using Skycap.Net.Common.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Windows.Networking.Sockets;
    using Windows.Networking;

    public class HttpProxyConnection : BaseConnection
    {
        protected bool _isHTTP1_1;
        protected StreamSocket _socket;
        protected const string AccessDenidedByHttpProxyMessage = "The HTTP proxy server does not grant access to the requested server";
        protected const int BufferSize = 0x101;
        protected static Encoding DefaultEncoding = Encoding.UTF8;
        protected const string InvalidProxyTypeMessage = "The proxy type should match the connection class being used";
        protected const string NullUserIDMessage = "The proxyUserID cannot be null";
        protected int ProxyAuthenticationRequiredCode = 0x197;
        protected const string ProxyErrorMessage = "Error during connection to proxy";
        protected int ProxyOKCode = 200;
        protected int ProxyUnauthorizedCode = 0x191;

        public HttpProxyConnection(IConfigurationProvider configurationProvider)
        {
            if (configurationProvider == null)
            {
                throw new ArgumentNullException("configurationProvider");
            }
            this.State = EConnectionState.Disonnected;
            this.ConfigurationProvider = configurationProvider;
        }

        public override int Available()
        {
            // TODO: return this._socket.Available;
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
            if (this.ConfigurationProvider.ProxyType != EProxyType.HTTP)
            {
                throw new ArgumentException("The proxy type should match the connection class being used");
            }
            this._socket = new StreamSocket();
            this._socket.Control.KeepAlive = true;
            await this._socket.ConnectAsync(new HostName(this.ConfigurationProvider.ProxyHost), this.ConfigurationProvider.ProxyPort.ToString());
            base._streamForRead = this._socket.InputStream.AsStreamForRead();
            base._streamForWrite = this._socket.OutputStream.AsStreamForWrite();
            List<string> list = new List<string> {
                string.Format("CONNECT {0}:{1} HTTP/1.{2}", this.ConfigurationProvider.Host, this.ConfigurationProvider.Port, true ? "1" : "0") + "\r\n"
            };
            if (!string.IsNullOrEmpty(this.ConfigurationProvider.ProxyUser))
            {
                list.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", this.ConfigurationProvider.ProxyUser, this.ConfigurationProvider.ProxyPassword))) + "\r\n");
            }
            list.Add("\r\n");
            int num = 0;
            int destinationIndex = 0;
            foreach (string str in list)
            {
                num += Encoding.UTF8.GetByteCount(str);
            }
            byte[] destinationArray = new byte[num];
            foreach (string str2 in list)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str2);
                Array.Copy(bytes, 0, destinationArray, destinationIndex, bytes.Length);
                destinationIndex += bytes.Length;
            }
            base._streamForWrite.Write(destinationArray, 0, destinationArray.Length);
            string str3 = this.ReceiveLine();
            if (str3.Contains(this.ProxyUnauthorizedCode.ToString()) || str3.Contains(this.ProxyAuthenticationRequiredCode.ToString()))
            {
                throw new ConnectionException("The HTTP proxy server does not grant access to the requested server");
            }
            if (!str3.Contains(this.ProxyOKCode.ToString()))
            {
                throw new ConnectionException(string.Format("{0}: {1}", "Error during connection to proxy", str3));
            }
            while (!string.IsNullOrEmpty(this.ReceiveLine()))
            {
            }
            if (this.ConfigurationProvider.SSLInteractionType == EInteractionType.SSLPort)
            {
                this.SwitchToSslChannel();
            }
            this.State = EConnectionState.Connected;
        }

        public async override void SwitchToSslChannel()
        {
            await this._client.UpgradeToSslAsync(SocketProtectionLevel.Ssl, this._client.Information.LocalAddress).AsTask().ConfigureAwait(false);
        }

        public bool HTTP1_1
        {
            get
            {
                return this._isHTTP1_1;
            }
            set
            {
                this._isHTTP1_1 = value;
            }
        }
    }
}

