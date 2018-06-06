namespace Skycap.Net.Common
{
    using Skycap.Net.Common.Collections;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    [DebuggerDisplay("{Header.ContentType}")]
    public class PopMessage : StructuredMessage
    {
        public const string MIMEVersionExceptionMessage = "MIME version is not 1.0";
        public const string ParseArgumentExceptionMessage = "Unknown status of the parser";

        internal PopMessage()
        {
        }

        public PopMessage(EmailAddress from, EmailAddressCollection to, string subject, string messageText)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }
            if (messageText == null)
            {
                throw new ArgumentNullException("messageText");
            }
            this.From = from;
            this.Subject = subject;
            this.Text = messageText;
        }

        public PopMessage(EmailAddress from, EmailAddress to, string subject, string messageText)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }
            if (messageText == null)
            {
                throw new ArgumentNullException("messageText");
            }
            this.From = from;
            this.To.Add(to);
            this.Subject = subject;
            this.Text = messageText;
        }

        public override bool IsSeen
        {
            get;
            internal set;
        }

        public override bool IsDeleted
        {
            get;
            internal set;
        }

        public override bool IsFlagged
        {
            get;
            internal set;
        }
    }
}

