using System;
using Skycap.Data;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// Represents the Choose Email Service dialog.
    /// </summary>
    public sealed partial class EmailServiceDialog : UserControl
    {
        /// <summary>
        /// The mail account dialog.
        /// </summary>
        private MailAccountDialog mailAccountDialog;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.EmailServiceDialog class.
        /// </summary>
        public EmailServiceDialog()
        {
            this.InitializeComponent();
            this.mailAccountDialog = new MailAccountDialog();
            this.EmailServiceProvider = EmailServiceProvider.Other;
        }

        /// <summary>
        /// Gets the email service.
        /// </summary>
        public EmailService EmailService
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the email service provider.
        /// </summary>
        public EmailServiceProvider EmailServiceProvider
        {
            get;
            internal set;
        }

        /// <summary>
        /// Occurs when an email service radio button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void EmailService_Checked(object sender, RoutedEventArgs e)
        {
            // Enable the connect button
            bConnect.IsEnabled = true;

            // Get the email service radio button that was checked
            RadioButton rbEmailService = (RadioButton)sender;

            // Set the email service
            EmailService = (EmailService)Enum.Parse(typeof(EmailService), rbEmailService.Content.ToString());

            // Determine which email service was selected
            switch (EmailService)
            {
                // If Imap
                case EmailService.Imap:
                    cbKeepEmailCopiesOnServer.IsChecked = true;
                    cbKeepEmailCopiesOnServer.IsEnabled = false;
                    break;

                // If Pop
                case EmailService.Pop:
                    cbKeepEmailCopiesOnServer.IsChecked = true;
                    cbKeepEmailCopiesOnServer.IsEnabled = true;
                    break;
            }
        }

        /// <summary>
        /// Show popup on screen.
        /// </summary>
        public void Show()
        {
            // Dialog is open
            MainPage.Current.IsDialogOpen = true;

            // Determine the email service provider
            switch (EmailServiceProvider)
            {
                // If Gmail
                case EmailServiceProvider.Gmail:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Colors.DarkRed);
                    break;

                // If Outlook
                case EmailServiceProvider.Outlook:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Color.FromArgb(255, 0, 114, 198));
                    rbPop.IsChecked = true;
                    rbImap.IsEnabled = false;
                    break;

                // If Hotmail
                case EmailServiceProvider.Hotmail:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Colors.DarkOrange);
                    rbPop.IsChecked = true;
                    rbImap.IsEnabled = false;
                    break;

                // If Yahoo
                case EmailServiceProvider.Yahoo:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Colors.DarkViolet);
                    rbPop.IsChecked = true;
                    rbImap.IsEnabled = false;
                    break;

                // If Aol
                case EmailServiceProvider.Aol:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Colors.DarkBlue);
                    break;

                // If Gmx
                case EmailServiceProvider.Gmx:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Colors.Navy);
                    break;

                // If Zoho
                case EmailServiceProvider.Zoho:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Colors.DarkGreen);
                    break;

                // If Other
                case EmailServiceProvider.Other:
                    cdEmailServiceDialog.TitleBackground = new SolidColorBrush(Colors.Black);
                    break;
            }

            // Set title and image and show
            cdEmailServiceDialog.Title = string.Format("Add your {0} account", EmailServiceProvider.ToString());
            cdEmailServiceDialog.TitleImageSource = new BitmapImage(new Uri(this.BaseUri, string.Format("/Assets/{0}.png", EmailServiceProvider.ToString())));
            cdEmailServiceDialog.IsOpen = true;
        }

        /// <summary>
        /// Hide popup on screen.
        /// </summary>
        public void Hide()
        {
            ResetState();
            cdEmailServiceDialog.IsOpen = false;

            // Dialog is closed
            MainPage.Current.IsDialogOpen = false;
        }

        /// <summary>
        /// Resets the state of the controls.
        /// </summary>
        private void ResetState()
        {
            rbImap.IsChecked = false;
            rbImap.IsEnabled = true;
            rbPop.IsChecked = false;
            bConnect.IsEnabled = false;
        }

        /// <summary>
        /// Occurs the Connect button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bConnect_Click(object sender, RoutedEventArgs e)
        {
            mailAccountDialog.EmailService = EmailService;
            mailAccountDialog.EmailServiceProvider = EmailServiceProvider;
            mailAccountDialog.KeepEmailCopiesOnServer = cbKeepEmailCopiesOnServer.IsChecked.Value;
            bCancel_Click(sender, e);
            mailAccountDialog.Show();
        }

        /// <summary>
        /// Occurs the Cancel button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
