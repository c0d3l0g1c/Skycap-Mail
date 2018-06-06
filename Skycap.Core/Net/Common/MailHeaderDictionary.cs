using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;

using Skycap.Data;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents a collection of Account Name keys and Folder Name mail header values.
    /// </summary>
    [CollectionDataContract]
    [KnownType(typeof(ConcurrentDictionary<string, ConcurrentDictionary<string, MailHeaderObservableCollectionView>>))]
    [KnownType(typeof(ConcurrentDictionary<string, MailHeaderObservableCollectionView>))]
    [KnownType(typeof(MailHeaderObservableCollectionView))]
    public class MailHeaderDictionary : ConcurrentDictionary<string, ConcurrentDictionary<string, MailHeaderObservableCollectionView>>
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Data.MailHeaderDictionary class.
        /// </summary>
        public MailHeaderDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        { 

        }

        /// <summary>
        /// Ensures a mail header dictionary entry exists for the specified account and folder.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        public void EnsureMailHeader(AccountSettingsData accountSettingsData, Mailbox mailbox)
        {
            try
            {
                // Make sure the account exists
                if (!this.ContainsKey(accountSettingsData.EmailAddress))
                    this.TryAdd(accountSettingsData.EmailAddress, new ConcurrentDictionary<string, MailHeaderObservableCollectionView>(StringComparer.OrdinalIgnoreCase));

                // Make sure the folder for the account exists
                if (!this[accountSettingsData.EmailAddress].ContainsKey(mailbox.FullName))
                    this[accountSettingsData.EmailAddress].TryAdd(mailbox.FullName, new MailHeaderObservableCollectionView());
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.Name, ex.ToString());
            }
        }

        /// <summary>
        /// Adds or updates the mail header for the specified account and mailbox.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="mailHeader">The mail header.</param>
        public void AddOrUpdate(AccountSettingsData accountSettingsData, Mailbox mailbox, MailHeader mailHeader)
        {
            try
            {
                // Ensure the mail header dictionary entry exists
                EnsureMailHeader(accountSettingsData, mailbox);

                // If the header exists - update it
                if (this[accountSettingsData.EmailAddress][mailbox.FullName].Contains(mailHeader))
                    this[accountSettingsData.EmailAddress][mailbox.FullName][this[accountSettingsData.EmailAddress][mailbox.FullName].IndexOf(mailHeader)] = mailHeader;
                // Else if the header does not exist = add it
                else
                    this[accountSettingsData.EmailAddress][mailbox.FullName].Add(mailHeader);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.Name, ex.ToString());
            }
        }

        /// <summary>
        /// Gets the total email count.
        /// </summary>
        public int AllEmailCount
        {
            get
            {
                lock (base.Values)
                {
                    return base.Values
                          .SelectMany(o => o.Values)
                          .Sum(o => o.Count);
                }
            }
        }

        /// <summary>
        /// Gets the unread email count.
        /// </summary>
        public int AllUnreadEmailCount
        {
            get
            {
                lock (base.Values)
                {
                    return base.Values
                          .SelectMany(o => o.Values)
                          .Select(o => o.UnreadEmailCount)
                          .Sum();
                }
            }
        }

        /// <summary>
        /// Gets the unread email count for the specified email address.
        /// </summary>
        public int GetAccountUnreadEmailCount(string emailAddress)
        {
            lock (base[emailAddress].Values)
            {
                return base[emailAddress].Values
                      .Select(o => o.UnreadEmailCount)
                      .Sum();
            }
        }
    }
}
