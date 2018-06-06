using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// Represents a control to insert a link.
    /// </summary>
    public sealed partial class InsertLinkDialog : UserControl
    {
        /// <summary>
        /// Occurs when a link is inserted.
        /// </summary>
        public event EventHandler LinkInserted;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.InsertLinkDialog class.
        /// </summary>
        public InsertLinkDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the address.
        /// </summary>
        public string Address
        {
            get
            {
                return txtAddress.Text;
            }
        }

        /// <summary>
        /// Gets the text to be displayed.
        /// </summary>
        public string TextToBeDisplayed
        {
            get
            {
                if (string.IsNullOrEmpty(txtTextToBeDisplayed.Text))
                    return Address;
                return txtTextToBeDisplayed.Text;
            }
        }

        /// <summary>
        /// Show popup on screen.
        /// </summary>
        public void Show()
        {
            pInsertLinkDialog.IsOpen = true;
            pInsertLinkDialog.HorizontalOffset = (Window.Current.Bounds.Width / 2) - (Width / 2);
            pInsertLinkDialog.VerticalOffset = (Window.Current.Bounds.Height / 2) - (Height / 2);
        }

        /// <summary>
        /// Hide popup on screen.
        /// </summary>
        public void Hide()
        {
            pInsertLinkDialog.IsOpen = false;
        }

        /// <summary>
        /// Occurs when the Insert Link button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bInsertLink_Click(object sender, RoutedEventArgs e)
        {
            // Check if address is supplied
            string validationMessages = null;
            if (string.IsNullOrEmpty(Address))
                validationMessages += "Please supply an address.";
            // Check if supplied address is valid
            else if (!Uri.IsWellFormedUriString(Address, UriKind.RelativeOrAbsolute))
                validationMessages += "Please supply a valid address.";

            // If validation is successfull
            if (string.IsNullOrEmpty(validationMessages))
            {
                // Raise the LinkInserted event
                if (LinkInserted != null)
                    LinkInserted(this, EventArgs.Empty);

                // Clear the fields
                txtAddress.Text = string.Empty;
                txtTextToBeDisplayed.Text = string.Empty;

                // Hide the popup
                Hide();
            }
        }
    }
}
