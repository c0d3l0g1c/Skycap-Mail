using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the enumerations of the various pop and imap response codes.
    /// </summary>
    public enum CommandResponseType
    {
        /// <summary>
        /// Command was successfull.
        /// </summary>
        Ok,
        /// <summary>
        /// Command failed.
        /// </summary>
        No,
        /// <summary>
        /// Command errored.
        /// </summary>
        Bad
    }
}
