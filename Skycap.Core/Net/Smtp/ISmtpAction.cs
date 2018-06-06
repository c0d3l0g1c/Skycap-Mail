namespace Skycap.Net.Smtp
{
    using Skycap.Net.Common.Connections;

    public interface ISmtpAction
    {
        SmtpResponse Interact(IConnection connection);
    }
}

