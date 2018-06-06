namespace Skycap.Net.Common.Connections
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Configurations;
    using System;
    using System.Collections.Generic;
    using Windows.Networking.Sockets;
    using System.IO;
    using Windows.Networking;
    using System.Threading.Tasks;
    using System.Threading;

    public class DirectConnection : BaseConnection
    {
        public DirectConnection(IConfigurationProvider configurationProvider)
        {
            base._configurationProvider = configurationProvider;
            this.State = EConnectionState.Disonnected;
        }

        public override int Available()
        {
            // TODO: return this._client.Available;
            return 1;
        }

        public override bool CanRead(int delay)
        {
            //List<Socket> checkRead = new List<Socket> {
            //    this._client.Client
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
                this._client.Dispose();
                this.State = EConnectionState.Disonnected;
            }
        }

        public override void Open()
        {
            this._client = new StreamSocket();
            this._client.Control.KeepAlive = true;
            Task.Run(async () =>
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await this._client.ConnectAsync(new HostName(this.ConfigurationProvider.Host), this.ConfigurationProvider.Port.ToString()).AsTask(cancellationTokenSource.Token).ConfigureAwait(false);
            }).Wait();
            base._streamForRead = this._client.InputStream.AsStreamForRead();
            base._streamForWrite = this._client.OutputStream.AsStreamForWrite();
            if (base._configurationProvider.SSLInteractionType == EInteractionType.SSLPort)
            {
                this.SwitchToSslChannel();
            }
            this.State = EConnectionState.Connected;
        }

        public override IConfigurationProvider ConfigurationProvider
        {
            get
            {
                return base._configurationProvider;
            }
        }
    }
}

