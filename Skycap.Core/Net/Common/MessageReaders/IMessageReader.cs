namespace Skycap.Net.Common.MessageReaders
{
    using System;

    public interface IMessageReader
    {
        byte[] ReadLine();

        bool EndOfMessage { get; }
    }
}

