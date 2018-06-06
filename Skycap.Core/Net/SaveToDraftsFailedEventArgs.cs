using System;
using Skycap.Net.Common;

namespace Skycap.Net
{
    /// <summary>
    /// Represents the SaveToDraftsFailed event data.
    /// </summary>
    public class SaveToDraftsFailedEventArgs : FailedEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.SaveToDraftsFailedEventArgs class.
        /// </summary>
        /// <param name="saveToDrafts">The save to drafts event data.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="userErrorMessage">The user error message.</param>
        public SaveToDraftsFailedEventArgs(SaveToDraftsEventArgs saveToDrafts, Exception errorMessage, string userErrorMessage)
            : base(errorMessage, userErrorMessage)
        { 
            // Intialise local variables
            SaveToDraftsEvent = saveToDrafts;
        }

        /// <summary>
        /// Gets the save to drafts event data.
        /// </summary>
        public SaveToDraftsEventArgs SaveToDraftsEvent
        {
            get;
            private set;
        }
    }
}
