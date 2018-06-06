using System;
using System.Collections.ObjectModel;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the MailboxTree event data.
    /// </summary>
    public class MailboxTreeEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MailboxTreeUpdatedEventArgs class.
        /// </summary>
        /// <param name="mailboxTree">The mailbox tree.</param>
        public MailboxTreeEventArgs(ObservableCollection<Mailbox> mailboxTree)
        {
            // Initialise local variables
            MailboxTree = mailboxTree;
        }

        /// <summary>
        /// Gets the mailbox tree.
        /// </summary>
        public ObservableCollection<Mailbox> MailboxTree
        {
            get;
            private set;
        }
    }
}
