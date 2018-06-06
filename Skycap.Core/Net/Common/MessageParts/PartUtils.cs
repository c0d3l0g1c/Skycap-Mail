namespace Skycap.Net.Common.MessageParts
{
    using Skycap.Net.Common;
    using System;
    using Skycap.IO;

    internal static class PartUtils
    {
        public static string ExtractOriginalFilename(ContentType contentType, ContentDisposition contentDisposition)
        {
            if ((contentDisposition != null) && contentDisposition.Attributes.ContainsKey("filename"))
            {
                return IOUtil.FormatFileSystemName(contentDisposition.Attributes["filename"]);
            }
            if (contentType.Attributes.ContainsKey("filename"))
            {
                return IOUtil.FormatFileSystemName(contentType.Attributes["filename"]);
            }
            return "";
        }

        public static bool IsMessagePart(ContentType contentType)
        {
            return (contentType.Type == "message");
        }

        public static bool IsMultipart(ContentType contentType)
        {
            return (contentType.Type == "multipart");
        }

        public static bool IsTextPart(ContentType contentType, ContentDisposition contentDisposition)
        {
            if (contentDisposition != null)
            {
                if (contentType.Type != "text")
                {
                    return false;
                }
                if (contentDisposition.Disposition == "attachment")
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(ExtractOriginalFilename(contentType, contentDisposition)) && (contentDisposition.Disposition != "inline"))
                {
                    return false;
                }
                return true;
            }
            return ((contentType.Type == "text") && string.IsNullOrEmpty(ExtractOriginalFilename(contentType, contentDisposition)));
        }
    }
}

