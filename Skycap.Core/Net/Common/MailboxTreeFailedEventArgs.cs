using System;
using System.Collections.ObjectModel;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the MailboxTreeFailed event data.
    /// </summary>
    public class MailboxTreeFailedEventArgs : FailedEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MailboxTreeFailedEventArgs class.
        /// </summary>
        /// <param name="mailboxTree">The mailbox tree.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="userErrorMessage">The user error message.</param>
        public MailboxTreeFailedEventArgs(ObservableCollection<Mailbox> mailboxTree, Exception errorMessage, string userErrorMessage)
            : base(errorMessage, userErrorMessage)
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
