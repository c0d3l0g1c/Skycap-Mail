using System;
using System.Collections.Generic;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the RenameMailboxFailed event data.
    /// </summary>
    public class RenameMailboxFailedEventArgs : FailedEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.RenameMailboxFailedEventArgs class.
        /// </summary>
        /// <param name="renamedMailboxes">The renamed mailboxes.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="userErrorMessage">The user error message.</param>
        public RenameMailboxFailedEventArgs(Dictionary<Mailbox, Mailbox> renamedMailboxes, Exception errorMessage, string userErrorMessage)
            : base(errorMessage, userErrorMessage)
        { 
            // Initialise local variables
            RenamedMailboxes = renamedMailboxes;
        }

        /// <summary>
        /// Gets the renamed mailbox.
        /// </summary>
        public Dictionary<Mailbox, Mailbox> RenamedMailboxes
        {
            get;
            private set;
        }
    }
}
