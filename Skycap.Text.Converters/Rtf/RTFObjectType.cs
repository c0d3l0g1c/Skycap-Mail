namespace Skycap.Text.Converters.Rtf
{
    using System;

    public enum RTFObjectType
    {
        None,
        KeyWord,
        Control,
        Text,
        EOF,
        GroupStart,
        GroupEnd,
        Root,
        Group
    }
}

