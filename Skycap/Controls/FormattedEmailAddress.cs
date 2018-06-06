using System;
using System.Runtime.Serialization;

using Skycap.Net.Common;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Skycap.Controls
{
    /// <summary>
    /// Represents a formatted email address.
    /// </summary>
    public class FormattedEmailAddress : EmailAddress
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.FormattedEmailAddress class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        public FormattedEmailAddress(EmailAddress emailAddress)
            : this(emailAddress.Address, emailAddress.DisplayName)
        { 
        
        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.FormattedEmailAddress class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        public FormattedEmailAddress(string emailAddress)
            : this(emailAddress, null)
        { 
            
        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.FormattedEmailAddress class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="displayName">The display name.</param>
        public FormattedEmailAddress(string emailAddress, string displayName)
            : base(emailAddress, displayName)
        { 
            // Initialise local properties
            ReadOnlyMode = Visibility.Visible;
        }

        /// <summary>
        /// Gets the read only mode.
        /// </summary>
        public Visibility ReadOnlyMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the edit mode.
        /// </summary>
        public Visibility EditMode
        {
            get
            {
                return (ReadOnlyMode == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            }
        }
    }
}
