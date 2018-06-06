using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the MailboxFailed event data.
    /// </summary>
    public class MailboxFailedEventArgs : FailedEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MailboxFailedEventArgs class.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="userErrorMessage">The user error message.</param>
        public MailboxFailedEventArgs(Mailbox mailbox, Exception errorMessage, string userErrorMessage)
            : base(errorMessage, userErrorMessage)
        { 
            // Initialise local variables
            Mailbox = mailbox;
        }

        /// <summary>
        /// Gets the mailbox.
        /// </summary>
        public Mailbox Mailbox
        {
            get;
            private set;
        }
    }
}
