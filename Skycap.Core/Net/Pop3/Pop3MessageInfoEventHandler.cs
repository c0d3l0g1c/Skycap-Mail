namespace Skycap.Net.Pop3
{
    using Skycap.Net.Common;
    using System;
    using System.Runtime.CompilerServices;

    public delegate void Pop3MessageInfoEventHandler(Pop3Client sender, Pop3MessageInfo messageInfo, string errorMessage, PopMessage message);
}

