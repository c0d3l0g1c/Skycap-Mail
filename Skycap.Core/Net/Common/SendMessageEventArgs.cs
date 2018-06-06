using System;
using System.Collections.Generic;
using System.Linq;
using Skycap.Net.Smtp;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the SendMessage event data.
    /// </summary>
    public class SendMessageEventArgs : MoveMessageEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.SendMessageEventArgs class.
        /// </summary>
        /// <param name="messagePaths">The message paths to move.</param>
        /// <param name="messages">The messages</param>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        public SendMessageEventArgs(Dictionary<string, string> messagePaths, Dictionary<string, StructuredMessage> messages, Mailbox sourceMailbox, Mailbox destinationMailbox)
            : this(messagePaths, messages, sourceMailbox, destinationMailbox, null)
        {

        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.SendMessageEventArgs class.
        /// </summary>
        /// <param name="messagePaths">The message paths to move.</param>
        /// <param name="messages">The messages</param>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        /// <param name="sendResult">The send result.</param>
        public SendMessageEventArgs(Dictionary<string, string> messagePaths, Dictionary<string, StructuredMessage> messages, Mailbox sourceMailbox, Mailbox destinationMailbox, SendResult sendResult)
            : base(messagePaths, messages, sourceMailbox, destinationMailbox)
        {
            // Initialise local variables
            SendResult = sendResult;
        }

        /// <summary>
        /// Gets or sets the send result.
        /// </summary>
        public SendResult SendResult
        {
            get;
            private set;
        }
    }
}
