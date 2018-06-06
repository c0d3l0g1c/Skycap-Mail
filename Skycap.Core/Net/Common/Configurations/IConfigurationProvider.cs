namespace Skycap.Net.Common.Configurations
{
    using System;

    public interface IConfigurationProvider
    {
        string AttachmentDirectory { get; set; }

        EAuthenticationType AuthenticationType { get; set; }

        string Host { get; set; }

        ushort Port { get; set; }

        string ProxyHost { get; set; }

        string ProxyPassword { get; set; }

        ushort ProxyPort { get; set; }

        EProxyType ProxyType { get; set; }

        string ProxyUser { get; set; }

        int ReceiveTimeOut { get; set; }

        int SendTimeOut { get; set; }

        EInteractionType SSLInteractionType { get; set; }
    }
}

