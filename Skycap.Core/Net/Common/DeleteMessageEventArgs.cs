using System;
using System.Collections.Generic;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the DeleteMessage event data.
    /// </summary>
    public class DeleteMessageEventArgs : MailboxEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.DeleteMessageEventArgs class.
        /// </summary>
        /// <param name="messagePaths">The message paths to delete.</param>
        /// <param name="mailbox">The mailbox to delete from.</param>
        public DeleteMessageEventArgs(Dictionary<string, string> messagePaths, Mailbox mailbox)
            : base(mailbox)
        { 
            // Initialise local variables
            MessagePaths = messagePaths;
        }

        /// <summary>
        /// Gets the message paths to delete.
        /// </summary>
        public Dictionary<string, string> MessagePaths
        {
            get;
            private set;
        }
    }
}
