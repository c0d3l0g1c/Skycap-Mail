namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;

    public interface IPOP3Command
    {
        Pop3Response Interact(IConnection connection);
    }
}

