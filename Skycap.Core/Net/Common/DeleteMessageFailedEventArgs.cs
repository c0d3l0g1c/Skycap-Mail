using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the DeleteMessageFailed event data.
    /// </summary>
    public class DeleteMessageFailedEventArgs : FailedEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.DeleteMessageFailedEventArgs class.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="userErrorMessage">The user error message.</param>
        public DeleteMessageFailedEventArgs(DeleteMessageEventArgs deleteMessage, Exception errorMessage, string userErrorMessage)
            : base(errorMessage, userErrorMessage)
        { 
            // Initialise local variables
            DeleteMessage = deleteMessage;
        }

        /// <summary>
        /// Gets the delete message event data.
        /// </summary>
        public DeleteMessageEventArgs DeleteMessage
        {
            get;
            private set;
        }
    }
}
