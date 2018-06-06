using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the SendMessageFailed event data.
    /// </summary>
    public class SendMessageFailedEventArgs : FailedEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.SendMessageFailedEventArgs class.
        /// </summary>
        /// <param name="sendMessage">The send message event data.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="userErrorMessage">The user error message.</param>
        public SendMessageFailedEventArgs(SendMessageEventArgs sendMessage, Exception ex, string userErrorMessage)
            : base(ex, userErrorMessage)
        { 
            // Initialise local variables
            SendMessage = sendMessage;
        }

        /// <summary>
        /// Gets or sets the send message event data.
        /// </summary>
        public SendMessageEventArgs SendMessage
        {
            get;
            private set;
        }
    }
}
