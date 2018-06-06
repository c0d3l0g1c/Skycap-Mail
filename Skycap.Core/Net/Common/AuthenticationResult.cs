using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents an authentication result.
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.AuthenticationResult class.
        /// </summary>
        public AuthenticationResult()
        {
            // Initialise local properties
            IsSuccessfull = false;
            Response = string.Empty;
        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.AuthenticationResult class.
        /// </summary>
        /// <param name="IsSuccessfull">isSuccessfull</param>
        /// <param name="Response">response</param>
        public AuthenticationResult(bool isSuccessfull, string response)
        { 
            // Initialise local properties
            IsSuccessfull = isSuccessfull;
            Response = response;
        }

        /// <summary>
        /// Gets a value indicating whether authentication was successfull.
        /// </summary>
        public bool IsSuccessfull
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the server authentication response.
        /// </summary>
        public string Response
        {
            get;
            private set;
        }
    }
}
