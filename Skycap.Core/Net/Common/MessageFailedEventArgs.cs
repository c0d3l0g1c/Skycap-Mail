using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the MessageFailedFailed event data.
    /// </summary>
    public class MessageFailedEventArgs : FailedEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MessageFailedEventArgs class.
        /// </summary>
        /// <param name="messageEventArgs">The message event args.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="userErrorMessage">The user message.</param>
        public MessageFailedEventArgs(MessageEventArgs messageEventArgs, Exception ex, string userErrorMessage)
            : base(ex, userErrorMessage)
        { 
            // Initialises local variables
            MessageEventArgs = messageEventArgs;
        }

        /// <summary>
        /// Gets the message event args.
        /// </summary>
        public MessageEventArgs MessageEventArgs
        {
            get;
            private set;
        }
    }
}
