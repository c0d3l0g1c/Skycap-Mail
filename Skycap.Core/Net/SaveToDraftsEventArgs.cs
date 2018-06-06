using System;
using Skycap.Net.Common;

namespace Skycap.Net
{
    /// <summary>
    /// Represents the SaveToDrafts event data.
    /// </summary>
    public class SaveToDraftsEventArgs : MessageEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.SaveToDraftsEventArgs class.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message.</param>
        public SaveToDraftsEventArgs(Mailbox mailbox, StructuredMessage message)
            : base(mailbox, message.Uid, message)
        { 
        
        }
    }
}
