namespace Skycap.Net.Common
{
    using Skycap.Net.Common.MessageParts;
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(Attachment))]
    [KnownType(typeof(BaseContentPart))]
    [KnownType(typeof(ContentPart))]
    [KnownType(typeof(MessagePart))]
    [KnownType(typeof(MultiPart))]
    [KnownType(typeof(TextPart))]
    [KnownType(typeof(MessageDeliveryStatusPart))]
    [KnownType(typeof(MessageDispositionNotificationPart))]
    public abstract class StructuredMessage : MailMessage
    {
        protected IPart _rootPart;

        public void FillText()
        {
            if (this.RootPart != null)
            {
                string htmlText = this.GetHtmlText(this.RootPart);
                string plainText = this.GetPlainText(this.RootPart);
                if (!string.IsNullOrEmpty(htmlText))
                {
                    this.TextContentType = ETextContentType.Html;
                    this.Text = htmlText;
                    this.PlainText = plainText;
                }
                else if (!string.IsNullOrEmpty(plainText))
                {
                    this.TextContentType = ETextContentType.Plain;
                    this.Text = plainText;
                }
                else
                {
                    this.TextContentType = ETextContentType.Plain;
                    this.Text = string.Empty;
                }
            }
        }

        private string GetHtmlText(IPart node)
        {
            if (node.Type == EPartType.Multi)
            {
                MultiPart part = (MultiPart) node;
                if ((part.Header.ContentType.SubType.Equals("mixed", StringComparison.OrdinalIgnoreCase) || part.Header.ContentType.SubType.Equals("alternative", StringComparison.OrdinalIgnoreCase)) || part.Header.ContentType.SubType.Equals("related", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IPart part2 in ((MultiPart) node).Parts)
                    {
                        string htmlText = this.GetHtmlText(part2);
                        if (htmlText != null)
                        {
                            return htmlText;
                        }
                    }
                }
            }
            else if (PartUtils.IsTextPart(node.Header.ContentType, node.Header.ContentDisposition) && node.Header.ContentType.SubType.Equals("html", StringComparison.CurrentCultureIgnoreCase))
            {
                return ((TextPart) node).Text;
            }
            return null;
        }

        private string GetPlainText(IPart node)
        {
            if (node.Type == EPartType.Multi)
            {
                MultiPart part = (MultiPart) node;
                if ((part.Header.ContentType.SubType.Equals("mixed", StringComparison.OrdinalIgnoreCase) || part.Header.ContentType.SubType.Equals("alternative", StringComparison.OrdinalIgnoreCase)) || part.Header.ContentType.SubType.Equals("related", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IPart part2 in ((MultiPart) node).Parts)
                    {
                        string plainText = this.GetPlainText(part2);
                        if (plainText != null)
                        {
                            return plainText;
                        }
                    }
                }
            }
            else if (PartUtils.IsTextPart(node.Header.ContentType, node.Header.ContentDisposition) && node.Header.ContentType.SubType.Equals("plain", StringComparison.CurrentCultureIgnoreCase))
            {
                return ((TextPart) node).Text;
            }
            return null;
        }

        [DataMember]
        public virtual IPart RootPart
        {
            get
            {
                return this._rootPart;
            }
            set
            {
                this._rootPart = value;
            }
        }

        [DataMember]
        public abstract bool IsSeen
        {
            get;
            internal set;
        }

        [DataMember]
        public abstract bool IsDeleted
        {
            get;
            internal set;
        }

        [DataMember]
        public abstract bool IsFlagged
        {
            get;
            internal set;
        }
    }
}

