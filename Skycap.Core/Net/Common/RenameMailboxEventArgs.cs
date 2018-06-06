using System;
using System.Collections.Generic;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the RenameMailbox event data.
    /// </summary>
    public class RenameMailboxEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.RenameMailbox class.
        /// </summary>
        /// <param name="renamedMailboxes">The renamed mailboxes.</param>
        public RenameMailboxEventArgs(Dictionary<Mailbox, Mailbox> renamedMailboxes)
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
