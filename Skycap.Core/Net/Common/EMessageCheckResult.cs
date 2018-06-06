namespace Skycap.Net.Common
{
    using System;

    public enum EMessageCheckResult
    {
        Correct,
        NoPlainTextInHTMLMessage,
        NoFromField,
        NoToField,
        NoText,
        Incorrect,
        AttachmentFileIsMissing
    }
}

