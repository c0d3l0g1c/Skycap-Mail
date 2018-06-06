using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

using Skycap.Data;

using Windows.UI.Xaml;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the action to take when a mailbox unread message count changes.
    /// </summary>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="unreadEmailCount">The unread email account.</param>
    public delegate void MailboxUnreadEmailCount(string emailAddress, int unreadEmailCount);

    /// <summary>
    /// Represents a mailbox list view item.
    /// </summary>
    [DataContract]
    public class MailboxListViewItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The unread message count.
        /// </summary>
        private int _unreadEmailCount;
        /// <summary>
        /// The text block visibility.
        /// </summary>
        private Visibility _textBlockVisibility = Visibility.Collapsed;

        /// <summary>
        /// Initialises a new instance of the Skycap.Data.MailboxListViewItem class.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="mailboxUnreadEmailCount">The unread email count action.</param>
        public MailboxListViewItem(AccountSettingsData accountSettingsData, Mailbox mailbox, int padding, MailboxUnreadEmailCount mailboxUnreadEmailCountAction)
            : this(accountSettingsData, mailbox, padding, mailboxUnreadEmailCountAction, Visibility.Visible)
        { 
        
        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Data.MailboxListViewItem class.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="mailboxUnreadEmailCount">The unread email count action.</param>
        /// <param name="textBlockVisibility">The text block visibility.</param>
        public MailboxListViewItem(AccountSettingsData accountSettingsData, Mailbox mailbox, int padding, MailboxUnreadEmailCount mailboxUnreadEmailCountAction, Visibility textBlockVisibility)
        { 
            // Initialise local variables
            AccountSettingsData = accountSettingsData;
            Mailbox = mailbox;
            Mailbox.PropertyChanged += Mailbox_PropertyChanged;
            Padding = new Thickness(padding, 10, 10, 10);
            MailboxUnreadEmailCountAction = mailboxUnreadEmailCountAction;
            TextBlockVisibility = textBlockVisibility;
            MailboxImageSource = string.Format("/Assets/{0}.png", Mailbox.Folder.ToString());
            StreamingContext streamingContext;
            OnDeserialized(streamingContext);
            MailboxUnreadEmailCountAction(AccountSettingsData.EmailAddress, StorageSettings.MailHeaderDictionary.GetAccountUnreadEmailCount(AccountSettingsData.EmailAddress));
        }

        /// <summary>
        /// Gets or sets the mailbox image source.
        /// </summary>
        [DataMember]
        public string MailboxImageSource
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the account settings data.
        /// </summary>
        [DataMember]
        public AccountSettingsData AccountSettingsData
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
            private set;
        }

        /// <summary>
        /// Gets or sets the padding.
        /// </summary>
        [DataMember]
        public Thickness Padding
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the mailbox unread email count action.
        /// </summary>
        [IgnoreDataMember]
        public MailboxUnreadEmailCount MailboxUnreadEmailCountAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unread message count.
        /// </summary>
        [DataMember]
        public int UnreadEmailCount
        {
            get
            {
                return _unreadEmailCount;
            }
            set
            {
                if (_unreadEmailCount != value)
                {
                    _unreadEmailCount = value;
                    OnPropertyChanged(this, new PropertyChangedEventArgs("UnreadEmailCount"));
                    OnPropertyChanged(this, new PropertyChangedEventArgs("UnreadEmailCountVisibility"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the text block visibility.
        /// </summary>
        [DataMember]
        public Visibility TextBlockVisibility
        {
            get
            {
                return _textBlockVisibility;
            }
            set
            {
                if (_textBlockVisibility != value)
                {
                    _textBlockVisibility = value;
                    OnPropertyChanged(this, new PropertyChangedEventArgs("TextBlockVisibility"));

                    switch (value)
                    {
                        case Visibility.Visible:
                            TextBoxVisibility = Visibility.Collapsed;
                            break;

                        default:
                            TextBoxVisibility = Visibility.Visible;
                            break;
                    }
                    OnPropertyChanged(this, new PropertyChangedEventArgs("TextBoxVisibility"));
                }
            }
        }

        /// <summary>
        /// Gets the text box visibility.
        /// </summary>
        [DataMember]
        public Visibility TextBoxVisibility
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the unread message count visibility.
        /// </summary>
        [IgnoreDataMember]
        public Visibility UnreadEmailCountVisibility
        {
            get
            {
                return (UnreadEmailCount == 0 ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        /// <summary>
        /// Gets or sets the CollectionChanged event handler.
        /// </summary>
        [IgnoreDataMember]
        public NotifyCollectionChangedEventHandler NotifyCollectionChangedEventHandler
        {
            get;
            set;
        }

        /// <summary>
        /// Method called on deserializarion.
        /// </summary>
        /// <param name="c">The streaming context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            try
            {
                NotifyCollectionChangedEventHandler = MailHeaderDictionaryCollectionChanged;
                StorageSettings.MailHeaderDictionary.EnsureMailHeader(AccountSettingsData, Mailbox);
                StorageSettings.MailHeaderDictionary[AccountSettingsData.EmailAddress][Mailbox.FullName].CollectionChanged += NotifyCollectionChangedEventHandler;
                UnreadEmailCount = StorageSettings.MailHeaderDictionary[AccountSettingsData.EmailAddress][Mailbox.FullName].UnreadEmailCount;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailHeaderDictionary).</param>
        /// <param name="e">The event data (NotifyCollectionChangedEventArgs).</param>
        public void MailHeaderDictionaryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UnreadEmailCount = StorageSettings.MailHeaderDictionary[AccountSettingsData.EmailAddress][Mailbox.FullName].Count(o => !o.IsSeen);
                if (MailboxUnreadEmailCountAction != null)
                    MailboxUnreadEmailCountAction(AccountSettingsData.EmailAddress, StorageSettings.MailHeaderDictionary.GetAccountUnreadEmailCount(AccountSettingsData.EmailAddress));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Determines whether this instance and another specified MailboxListViewItem object have the same value.
        /// </summary>
        /// <param name="obj">The MailboxListViewItem to compare to this instance.</param>
        /// <returns>true if the value of the value parameter is the same as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            MailboxListViewItem mailboxListViewItem = (MailboxListViewItem)obj;
            return Mailbox.FullName.Equals(mailboxListViewItem.Mailbox.FullName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the hash code for this MailboxListViewItem.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Mailbox.GetHashCode();
        }

        /// <summary>
        /// Creates a copy of the mailbox list view item.
        /// </summary>
        /// <returns>The mailbox list view item.</returns>
        public MailboxListViewItem Copy()
        {
            MailboxListViewItem copy = (MailboxListViewItem)this;
            copy.Mailbox = this.Mailbox.Copy();
            return copy;
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailboxListViewItem).</param>
        /// <param name="e">The event data (PropertyChangedEventArgs).</param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        /// <summary>
        /// Occurs when a mailbox property changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailboxListViewItem).</param>
        /// <param name="e">The event data (PropertyChangedEventArgs).</param>
        private void Mailbox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs("Mailbox"));
        }
    }
}
