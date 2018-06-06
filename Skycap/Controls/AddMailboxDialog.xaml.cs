using System;
using System.Threading.Tasks;
using Skycap.Data;
using Skycap.Net.Common;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// Represents the Add Mailbox dialog.
    /// </summary>
    public sealed partial class AddMailboxDialog : UserControl
    {
        /// <summary>
        /// Stores a value indicating if a button was clicked.
        /// </summary>
        private bool _isButtonClicked;
        /// <summary>
        /// Stores a value indicating the result.
        /// </summary>
        private bool _result;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.EmailServiceDialog class.
        /// </summary>
        public AddMailboxDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the add mailbox options.
        /// </summary>
        public AddMailboxOptions AddMailboxOptions
        {
            get;
            private set;
        }

        /// <summary>
        /// Occurs when a mailbox radio button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void Mailbox_Checked(object sender, RoutedEventArgs e)
        {
            // Get the email service radio button that was checked
            RadioButton rbAddMailboxOptions = (RadioButton)sender;

            // Set the email service
            AddMailboxOptions = (AddMailboxOptions)Enum.Parse(typeof(AddMailboxOptions), rbAddMailboxOptions.Name);
        }

        /// <summary>
        /// Show popup on screen.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        public async Task<bool> Show(Mailbox mailbox)
        {
            // Dialog is open
            MainPage.Current.IsDialogOpen = true;

            // If IsSystem or Reserved
            if (mailbox.IsSystem
             || mailbox.IsReserved)
            {
                InsideThisFolder.IsEnabled = false;
                BelowThisFolder.IsEnabled = true;
                BelowThisFolder.IsChecked = true;
            }

            // Set title and image and show
            InsideThisFolder.Content = string.Format("Inside {0}", mailbox.DisplayName);
            BelowThisFolder.Content = string.Format("Below {0}", mailbox.DisplayName);
            cdAddMailboxDialog.IsOpen = true;

            // Wait until a button is clicked
            while (!_isButtonClicked)
                await Task.Delay(1000);

            return _result;
        }

        /// <summary>
        /// Hide popup on screen.
        /// </summary>
        public void Hide()
        {
            cdAddMailboxDialog.IsOpen = false;

            // Dialog is closed
            MainPage.Current.IsDialogOpen = false;
        }

        /// <summary>
        /// Occurs the Ok button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            _result = true;
            _isButtonClicked = true;
            Hide();
        }

        /// <summary>
        /// Occurs the Cancel button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            _result = false;
            _isButtonClicked = true;
            Hide();
        }
    }
}
