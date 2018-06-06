namespace Skycap.Net.Common.Connections
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Configurations;
    using Skycap.Net.Common.Extensions;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Networking.Sockets;
    using Windows.Networking;

    public abstract class BaseConnection : IConnection, IDisposable
    {
        protected IConfigurationProvider _configurationProvider;
        protected EConnectionState _state;
        protected StreamSocket _client;
        protected Stream _streamForRead;
        protected Stream _streamForWrite;

        protected BaseConnection()
        {
        }

        public abstract int Available();
        public abstract bool CanRead(int delay);
        public abstract void Close();

        public virtual void Dispose()
        {
            this.Close();
        }

        public abstract void Open();
        public virtual byte[] ReceiveBytes()
        {
            byte[] buffer;
            try
            {
                buffer = StreamExtensions.ReadToEndLine(this._streamForRead);
            }
            catch (IOException)
            {
                this.State = EConnectionState.Disonnected;
                throw;
            }
            return buffer;
        }

        public virtual byte[] ReceiveBytes(ulong count)
        {
            byte[] buffer = new byte[count];
            ulong num = 0L;
            try
            {
                while (num < count)
                {
                    num += (ulong)this._streamForRead.Read(buffer, (int)num, (int)(count - num));
                }
            }
            catch (IOException)
            {
                this.State = EConnectionState.Disonnected;
                throw;
            }
            return buffer;
        }

        public virtual string ReceiveLine()
        {
            byte[] bytes = this.ReceiveBytes();
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public virtual void SendLine(string textLine)
        {
            string s = textLine + "\r\n";
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            try
            {
                this._streamForWrite.Write(bytes, 0, bytes.Length);
                this._streamForWrite.Flush();
            }
            catch (IOException)
            {
                this.State = EConnectionState.Disonnected;
                throw;
            }
        }

        public virtual void SwitchToSslChannel()
        {
            try
            {
                Task.Run(async () => await this._client.UpgradeToSslAsync(SocketProtectionLevel.Ssl, new HostName(_configurationProvider.Host)).AsTask().ConfigureAwait(false)).Wait();
            }
            catch
            {
                this._configurationProvider.SSLInteractionType = EInteractionType.StartTLS;
                Open();
            }
        }

        public virtual IConfigurationProvider ConfigurationProvider
        {
            get
            {
                return this._configurationProvider;
            }
            set
            {
                this._configurationProvider = value;
            }
        }

        public virtual EConnectionState State
        {
            get
            {
                return this._state;
            }
            protected set
            {
                this._state = value;
            }
        }
    }
}

