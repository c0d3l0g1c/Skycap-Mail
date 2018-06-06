namespace Skycap.Net.Imap
{
    using System;

    [Flags]
    public enum EClientState
    {
        Connected = 2,
        Disconnected = 1,
        Loggined = 4,
        Selected = 8
    }
}

