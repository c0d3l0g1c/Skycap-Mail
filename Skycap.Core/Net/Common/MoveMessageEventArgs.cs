using System;
using System.Collections.Generic;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the MoveMessage event data.
    /// </summary>
    public class MoveMessageEventArgs : MailboxEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MoveMessageEventArgs class.
        /// </summary>
        /// <param name="messagePaths">The message paths to move.</param>
        /// <param name="messages">The messages</param>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        public MoveMessageEventArgs(Dictionary<string, string> messagePaths, Dictionary<string, StructuredMessage> messages, Mailbox sourceMailbox, Mailbox destinationMailbox)
            : base(sourceMailbox)
        { 
            // Initialise local variables
            MessagePaths = messagePaths;
            Messages = messages;
            DestinationMailbox = destinationMailbox;
        }

        /// <summary>
        /// Gets the message paths to move.
        /// </summary>
        public Dictionary<string, string> MessagePaths
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        public Dictionary<string, StructuredMessage> Messages
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the destination mailbox.
        /// </summary>
        public Mailbox DestinationMailbox
        {
            get;
            private set;
        }
    }
}
