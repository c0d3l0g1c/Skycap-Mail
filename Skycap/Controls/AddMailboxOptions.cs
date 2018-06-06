using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skycap.Controls
{
    /// <summary>
    /// Represents the various enumerations for adding a mailbox.
    /// </summary>
    public enum AddMailboxOptions
    {
        /// <summary>
        /// Indicates that the mailbox should be created inside this folder.
        /// </summary>
        InsideThisFolder,
        /// <summary>
        /// Indicates that the mailbox should be created below this folder.
        /// </summary>
        BelowThisFolder
    }
}
