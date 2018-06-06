using System;
using System.Collections.Generic;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents Mailbox event data.
    /// </summary>
    public class MailboxEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MailboxEventArgs class.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        public MailboxEventArgs(Mailbox mailbox)
        { 
            // Initialise local variables
            Mailbox = mailbox;
        }

        /// <summary>
        /// Gets or sets the mailbox.
        /// </summary>
        public Mailbox Mailbox
        {
            get;
            private set;
        }
    }
}
