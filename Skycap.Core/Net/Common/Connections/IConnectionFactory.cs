namespace Skycap.Net.Common.Connections
{
    using Skycap.Net.Common.Configurations;

    public interface IConnectionFactory
    {
        IConnection GetConnection(IConfigurationProvider configurationProvider);
    }
}

