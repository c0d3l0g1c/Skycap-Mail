using Skycap.Net.Common;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// Represents an email address item.
    /// </summary>
    public sealed partial class EmailAddressItem : UserControl
    {
        /// <summary>
        /// Initilaises a new instance of the Skycap.Controls.EmailAddressItem class.
        /// </summary>
        public EmailAddressItem()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when a right-tap input stimulus happens while the pointer is over the Email Address hyperlink button.
        /// </summary>
        /// <param name="sender">The object that raised the event (HyperlinkButton).</param>
        /// <param name="e">The event data (RightTappedRoutedEventArgs).</param>
        private void hbEmailAddress_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Don't show the appbar
            e.Handled = true;

            // Get the email address
            EmailAddress emailAddress = (EmailAddress)DataContext;
        }
    }
}
