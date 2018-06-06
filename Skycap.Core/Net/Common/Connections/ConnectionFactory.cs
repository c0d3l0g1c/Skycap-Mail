namespace Skycap.Net.Common.Connections
{
    using Skycap.Net.Common.Configurations;
    using System;

    public class ConnectionFactory : IConnectionFactory
    {
        public IConnection GetConnection(IConfigurationProvider configurationProvider)
        {
            switch (configurationProvider.ProxyType)
            {
                case EProxyType.SOCKS5:
                    return new Socks5ProxyConnection(configurationProvider);

                case EProxyType.SOCKS4:
                    return new Socks4ProxyConnection(configurationProvider);

                case EProxyType.HTTP:
                    return new HttpProxyConnection(configurationProvider);
            }
            return new DirectConnection(configurationProvider);
        }
    }
}

