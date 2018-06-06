using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the various enumerations of the mail client state.
    /// </summary>
    [Flags]
    public enum MailClientState
    {
        /// <summary>
        /// Indicates the mail client is disconnected.
        /// </summary>
        Disconnected = 1,
        /// <summary>
        /// Indicates the mail client is connected.
        /// </summary>
        Connected = 2,
        /// <summary>
        /// Indicates the mail client is authenticated.
        /// </summary>
        Authenticated = 4,
        /// <summary>
        /// Indicates the mail client is awaiting.
        /// </summary>
        Awaiting = 8,
        /// <summary>
        /// Indicates the mail client is busy.
        /// </summary>
        Busy = 16,
    }
}
