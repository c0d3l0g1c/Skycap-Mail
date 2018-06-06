namespace Skycap.Net.Common.Configurations
{
    using System;
    using Windows.Storage;

    public class CodeConfigurationProvider : IConfigurationProvider
    {
        protected string _attachmentDirectory;
        protected EAuthenticationType _authenticationType;
        protected string _host;
        protected ushort _port;
        protected string _proxyHost;
        protected string _proxyPassword;
        protected ushort _proxyPort;
        protected EProxyType _proxyType;
        protected string _proxyUser;
        protected int _receiveTimeOut;
        protected int _sendTimeOut;
        protected EInteractionType _sslInteractionType;
        public const string DefaultAttachmentDirectory = ".";
        public const EAuthenticationType DefaultAuthenticationType = EAuthenticationType.Auto;
        public const EInteractionType DefaultInteractionType = EInteractionType.Plain;
        public const string DefaultProxyHost = "";
        public const ushort DefaultProxyPort = 0;
        public const EProxyType DefaultProxyType = EProxyType.No;
        public const int DefaultTimeout = 0x2710;

        public CodeConfigurationProvider(string host, ushort port)
        {
            this._proxyUser = "";
            this._proxyPassword = "";
            this.Init(host, port, EInteractionType.Plain, EAuthenticationType.Auto, 0x2710, 0x2710);
        }

        public CodeConfigurationProvider(string host, ushort port, EInteractionType interactionType, EAuthenticationType authenticationType)
        {
            this._proxyUser = "";
            this._proxyPassword = "";
            this.Init(host, port, interactionType, authenticationType, 0x2710, 0x2710);
        }

        public CodeConfigurationProvider(string host, ushort port, EInteractionType interactionType, EAuthenticationType authenticationType, int sendTimeOut, int recieveTimeOut)
        {
            this._proxyUser = "";
            this._proxyPassword = "";
            this.Init(host, port, interactionType, authenticationType, sendTimeOut, recieveTimeOut);
        }

        protected void Init(string host, ushort port, EInteractionType interactionType, EAuthenticationType authenticationType, int sendTimeOut, int receiveTimeOut)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (sendTimeOut < 0)
            {
                throw new ArgumentOutOfRangeException("sendTimeOut");
            }
            if (receiveTimeOut < 0)
            {
                throw new ArgumentOutOfRangeException("receiveTimeOut");
            }
            this.Host = host;
            this.Port = port;
            this.SSLInteractionType = interactionType;
            this.AuthenticationType = authenticationType;
            this.SendTimeOut = sendTimeOut;
            this.ReceiveTimeOut = receiveTimeOut;
            this.ProxyHost = "";
            this.ProxyPort = 0;
            this.ProxyType = EProxyType.No;
            this.AttachmentDirectory = ApplicationData.Current.LocalFolder.Path;
        }

        public virtual string AttachmentDirectory
        {
            get
            {
                return this._attachmentDirectory;
            }
            set
            {
                this._attachmentDirectory = value;
            }
        }

        public virtual EAuthenticationType AuthenticationType
        {
            get
            {
                return this._authenticationType;
            }
            set
            {
                this._authenticationType = value;
            }
        }

        public virtual string Host
        {
            get
            {
                return this._host;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._host = value;
            }
        }

        public virtual ushort Port
        {
            get
            {
                return this._port;
            }
            set
            {
                this._port = value;
            }
        }

        public virtual string ProxyHost
        {
            get
            {
                return this._proxyHost;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._proxyHost = value;
            }
        }

        public virtual string ProxyPassword
        {
            get
            {
                return this._proxyPassword;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._proxyPassword = value;
            }
        }

        public virtual ushort ProxyPort
        {
            get
            {
                return this._proxyPort;
            }
            set
            {
                this._proxyPort = value;
            }
        }

        public virtual EProxyType ProxyType
        {
            get
            {
                return this._proxyType;
            }
            set
            {
                this._proxyType = value;
            }
        }

        public virtual string ProxyUser
        {
            get
            {
                return this._proxyUser;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._proxyUser = value;
            }
        }

        public virtual int ReceiveTimeOut
        {
            get
            {
                return this._receiveTimeOut;
            }
            set
            {
                this._receiveTimeOut = value;
            }
        }

        public virtual int SendTimeOut
        {
            get
            {
                return this._sendTimeOut;
            }
            set
            {
                this._sendTimeOut = value;
            }
        }

        public virtual EInteractionType SSLInteractionType
        {
            get
            {
                return this._sslInteractionType;
            }
            set
            {
                this._sslInteractionType = value;
            }
        }
    }
}

