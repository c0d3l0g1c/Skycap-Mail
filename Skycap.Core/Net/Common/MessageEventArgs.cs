using System;
using Skycap.Data;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents a message event data.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MessageEventArgs class.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="uid">The unique message id.</param>
        /// <param name="message">The message.</param>
        public MessageEventArgs(Mailbox mailbox, string uid, StructuredMessage message)
        { 
            // Initialise local variables
            Mailbox = mailbox;
            Uid = uid;
            Message = message;
        }

        /// <summary>
        /// Gets the mailbox.
        /// </summary>
        public Mailbox Mailbox
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the unique message id.
        /// </summary>
        public string Uid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public StructuredMessage Message
        {
            get;
            private set;
        }
    }
}
