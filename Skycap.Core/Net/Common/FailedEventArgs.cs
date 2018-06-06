using System;

using Skycap.Data;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the Failed event data.
    /// </summary>
    public class FailedEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.FailedEventArgs class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="userErrorMessage">The user error message.</param>
        public FailedEventArgs(Exception errorMessage, string userErrorMessage)
            : base()
        { 
            // Initialise local variables
            ErrorMessage = errorMessage;
            UserErrorMessage = userErrorMessage;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public Exception ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the user error message.
        /// </summary>
        public string UserErrorMessage
        {
            get;
            private set;
        }
    }
}
