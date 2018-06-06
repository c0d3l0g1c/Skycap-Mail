namespace Skycap.Net.Imap.Commands
{
    using System;

    public enum EIMAP4StatusRequest
    {
        Messages = 1,
        Recent = 2,
        UidNext = 4,
        UidValidity = 8,
        Unseen = 0x10
    }
}

