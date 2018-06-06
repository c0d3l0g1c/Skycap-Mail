using System;
using System.ComponentModel;
using System.Runtime.Serialization;

using Skycap.Net.Common;
using Skycap.Net.Imap;

using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the mail message header.
    /// </summary>
    [DataContract]
    public class MailHeader : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// A value indicating whether the message has been seen.
        /// </summary>
        private bool _isSeen;
        /// <summary>
        /// A value indicating whether the message has been flagged.
        /// </summary>
        private bool _isFlagged;

        /// <summary>
        /// Initialises a new instance of the Skycap.Data.MailHeader class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="uid">The unique message id.</param>
        /// <param name="message">The mail message.</param>
        /// <param name="mailbox">The mailbox.</param>
        public MailHeader(string emailAddress, string uid, StructuredMessage message, Mailbox mailbox)
        { 
            // Initialise local variables
            EmailAddress = emailAddress;
            Uid = uid;
            Comments = message.Header.Comments;
            Mailbox = mailbox;
            MessagePath = message.MessagePath;
            Date = message.Date;
            From = (message.From == null ? string.Empty : (string.IsNullOrEmpty(message.From.DisplayName) ? message.From.Address : message.From.DisplayName));
            Subject = message.Header.Subject;
            IsSeen = message.IsSeen;
            IsDeleted = message.IsDeleted;
            AttachmentsImageVisibility = (message.Attachments.Count > 0 ? Visibility.Visible : Visibility.Collapsed);
            ImportanceVisibility = (message.Header.Importance == MailImportance.Normal ? Visibility.Collapsed : Visibility.Visible);
            switch (message.Header.Importance)
            { 
                case MailImportance.Low:
                    ImportanceForeground = "Blue";
                    break;

                case MailImportance.High:
                    ImportanceForeground = "Red";
                    break;

                default:
                    ImportanceForeground = "Black";
                    break;
            }
            IsFlagged = message.IsFlagged;
            IsImapMessage = (message is ImapMessage);
        }

        /// <summary>
        /// Gets the email address.
        /// </summary>
        [DataMember]
        public string EmailAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the unique message id.
        /// </summary>
        [DataMember]
        public string Uid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        [DataMember]
        public string Comments
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the mailbox.
        /// </summary>
        [DataMember]
        public Mailbox Mailbox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full message path.
        /// </summary>
        [DataMember]
        public string MessagePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message date.
        /// </summary>
        [DataMember]
        public DateTime Date
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the message date string.
        /// </summary>
        [IgnoreDataMember]
        public string DateString
        {
            get
            {
                return GetFormattedDate(Date);
            }
        }

        /// <summary>
        /// Gets or sets the message time.
        /// </summary>
        [IgnoreDataMember]
        public string Time
        {
            get
            {
                return GetFormattedTime(Date);
            }
        }

        /// <summary>
        /// Gets or sets the from mail address.
        /// </summary>
        [DataMember]
        public string From
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        [DataMember]
        public string Subject
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the message was seen.
        /// </summary>
        [DataMember]
        public bool IsSeen
        {
            get
            {
                return _isSeen;
            }
            private set
            {
                if (_isSeen.Equals(value))
                    return;

                _isSeen = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("HeaderWeight"));
            }
        }

        /// <summary>
        /// Gets or sets the header weight.
        /// </summary>
        [IgnoreDataMember]
        public FontWeight HeaderWeight
        {
            get
            {
                return (IsSeen ? FontWeights.Normal : FontWeights.Bold);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the message was deleted.
        /// </summary>
        [DataMember]
        public bool IsDeleted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the message was flagged.
        /// </summary>
        [DataMember]
        public bool IsFlagged
        {
            get
            {
                return _isFlagged;
            }
            private set
            {
                if (_isFlagged.Equals(value))
                    return;

                _isFlagged = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("FlaggedVisibility"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the message is a draft.
        /// </summary>
        [IgnoreDataMember]
        public Visibility DraftVisibility
        {
            get
            { 
                return (Mailbox.Folder == MailboxFolders.Drafts ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        /// <summary>
        /// Gets or sets the flagged symbol visibility.
        /// </summary>
        [IgnoreDataMember]
        public Visibility FlaggedVisibility
        {
            get
            {
                return (IsFlagged ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        /// <summary>
        /// Gets or sets the attachments image visibility.
        /// </summary>
        [DataMember]
        public Visibility AttachmentsImageVisibility
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the importance symbol visibility.
        /// </summary>
        [DataMember]
        public Visibility ImportanceVisibility
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the importance symbol foreground.
        /// </summary>
        [DataMember]
        public string ImportanceForeground
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is an imap message.
        /// </summary>
        [DataMember]
        public bool IsImapMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Marks the mail header as read.
        /// </summary>
        public void MarkAsRead()
        {
            IsSeen = true;
        }

        /// <summary>
        /// Marks the mail header as unread.
        /// </summary>
        public void MarkAsUnread()
        {
            IsSeen = false;
        }

        /// <summary>
        /// Marks the mail header as deleted.
        /// </summary>
        public void MarkAsDeleted()
        { 
            IsDeleted = true;
        }

        /// <summary>
        /// Marks the mail header as undeleted.
        /// </summary>
        public void MarkAsUndeleted()
        {
            IsDeleted = false;
        }

        /// <summary>
        /// Marks the mail header as flagged.
        /// </summary>
        public void MarkAsFlagged()
        {
            IsFlagged = true;
        }

        /// <summary>
        /// Marks the mail header as unflagged.
        /// </summary>
        public void MarkAsUnflagged()
        {
            IsFlagged = false;
        }

        /// <summary>
        /// Updates the flags.
        /// </summary>
        /// <param name="message">The message.</param>
        public void UpdateFlags(StructuredMessage message)
        {
            IsDeleted = message.IsDeleted;
            IsFlagged = message.IsFlagged;
            IsSeen = message.IsSeen;
        }

        /// <summary>
        /// Determines whether this instance and another specified mail header object have the same value.
        /// </summary>
        /// <param name="obj">The mail header to compare to this instance.</param>
        /// <returns>true if the value of the value parameter is the same as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            MailHeader mailHeader = (MailHeader)obj;
            return mailHeader.Mailbox.FullName.Equals(Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                && mailHeader.Uid.Equals(Uid, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the hash code for this mail header.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Mailbox.FullName.GetHashCode() ^ Uid.GetHashCode();
        }

        /// <summary>
        /// Gets the formatted date.
        /// </summary>
        /// <param name="date">The date time.</param>
        /// <returns>The formatted date.</returns>
        private string GetFormattedDate(DateTime date)
        {
            int daysDifference = DateTime.Now.Subtract(date).Days;
            if (daysDifference <= 6)
            {
                switch ((DateTime.Now.Day - date.Day))
                {
                    case 0:
                        return "Today";

                    case 1:
                        return "Yesterday";

                    default:
                        return date.DayOfWeek.ToString().Substring(0, 3);
                }
            }
            else
                return date.ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// Gets the formatted time.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>The formatted time.</returns>
        private string GetFormattedTime(DateTime time)
        {
            return time.ToString("H:mm tt");
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailHeader).</param>
        /// <param name="e">The event data (PropertyChangedEventArgs).</param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, e);
        }
    }
}
