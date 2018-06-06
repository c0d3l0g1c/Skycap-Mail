namespace Skycap.Net.Imap.Parsers
{
    using System;

    internal enum EFetchResponseParserState
    {
        Base,
        InQuotes,
        InParanthesis,
        InWord,
        InMultilineHeader,
        InMultilineBlock,
        AfterMultilineHeader,
        AfterSlash
    }
}

