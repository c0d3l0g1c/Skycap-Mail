namespace Skycap.Net.Smtp
{
    using System;

    public enum ESendResult
    {
        Ok,
        OkWithInvalidEmails,
        AuthenticationFailed,
        FromAddressFailed,
        DataSendingFailed,
        StrangeServerResponse,
        SslWasNotStarted
    }
}

