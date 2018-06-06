using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Skycap.Controls;
using Skycap.Data;
using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Text.Converters;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Settings
{
    /// <summary>
    /// Represents the various enumerations of the ListAccountsSettingsControl mode.
    /// </summary>
    enum ListAccountsSettingsMode
    { 
        /// <summary>
        /// Control is in add mode.
        /// </summary>
        Add,
        /// <summary>
        /// Control is in edit mode.
        /// </summary>
        Edit,
    }

    /// <summary>
    /// The account settings for setting up email accounts to connect to.
    /// </summary>
    public sealed partial class ListAccountsSettingsControl : UserControl
    {
        /// <summary>
        /// Occurs when the account settings control changes from hidden to visible.
        /// </summary>
        public event EventHandler Opened;
        /// <summary>
        /// Occurs when the account settings control changes from visible to hidden.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Stores a value indicating if this dialog is open or not.
        /// </summary>
        private bool _isDialogOpen;

        /// <summary>
        /// Initialises a new instance of the Skycap.Settings.ListAccountsSettingsControl class.
        /// </summary>
        public ListAccountsSettingsControl()
        {
            this.InitializeComponent();
            this.ListAccountsSettingsMode = ListAccountsSettingsMode.Add;
            this.tbEmailSignature.Document.SetText(TextSetOptions.None, MailMessage.DefaultSignature);
            this.cbDownloadNewEmail.ItemsSource = Enum.GetNames(typeof(DownloadNewEmailOptions)).Select(o => new { Name = o.ToWords(), Value = o });
            this.cbDownloadNewEmail.DisplayMemberPath = "Name";
            this.cbDownloadNewEmail.SelectedValuePath = "Value";
            this.cbDownloadNewEmail.SelectedValue = DownloadNewEmailOptions.AsItemsArrive.ToString();
            this.cbDownloadEmailsFrom.ItemsSource = Enum.GetNames(typeof(DownloadEmailsFromOptions)).Select(o => new { Name = o.ToWords(), Value = o });
            this.cbDownloadEmailsFrom.DisplayMemberPath = "Name";
            this.cbDownloadEmailsFrom.SelectedValuePath = "Value";
            this.cbDownloadEmailsFrom.SelectedValue = DownloadEmailsFromOptions.Anytime.ToString();
        }

        /// <summary>
        /// Gets or sets the ListAccountsSettingsControl mode. 
        /// </summary>
        internal ListAccountsSettingsMode ListAccountsSettingsMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the selected account settings data.
        /// </summary>
        internal AccountSettingsData SelectedAccountSettingsData
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a dialog is open.
        /// </summary>
        public bool IsDialogOpen
        {
            get
            {
                return _isDialogOpen;
            }
        }

        /// <summary>
        /// Hides or shows the mailbox list.
        /// </summary>
        private void HideShowMailboxList()
        {
            // Bind the account settings data
            lbMailboxList.ItemsSource = null;
            lbMailboxList.ItemsSource = StorageSettings.AccountSettingsDataDictionary.Values;

            if (lbMailboxList.Items.Count == 0)
                lbMailboxList.Visibility = Visibility.Collapsed;
            else
                lbMailboxList.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides or shows controls depending on mode.
        /// </summary>
        private void HideShowControlsDependingOnMode()
        {
            if (ListAccountsSettingsMode == ListAccountsSettingsMode.Add)
            {
                bRemove.Visibility = Visibility.Collapsed;
                cfConfigureAccount.ParentFlyout = cfAddAnAccountFlyout;
                esrblEmailService.IsEnabled = true;
            }
            else if (ListAccountsSettingsMode == ListAccountsSettingsMode.Edit)
            {
                //spDownloadEmailsFrom.Visibility = (SelectedAccountSettingsData.EmailService == EmailService.Pop ? Visibility.Collapsed : Visibility.Visible);
                bRemove.Visibility = Visibility.Visible;
                cfConfigureAccount.ParentFlyout = cfAccountsFlyout;
                esrblEmailService.IsEnabled = false;
            }
        }

        /// <summary>
        /// Occurs when the charm flyout visibility changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (CharmFlyout).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void flyout_IsOpenChanged(object sender, EventArgs e)
        {
            // Hide/Show mailbox list
            HideShowMailboxList();

            // Raise appropriate events for when the flyout is opened and closed
            if (cfAccountsFlyout.IsOpen || cfAddAnAccountFlyout.IsOpen)
                if (Opened != null) Opened(this, EventArgs.Empty);
            else if (!cfAccountsFlyout.IsOpen && !cfAddAnAccountFlyout.IsOpen)
                if (Closed != null) Closed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when the "Add an account" button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (hbAddAnAccount).</param>
        /// <param name="e">The event arguments (RoutedEventArgs).</param>
        private void hbAddAnAccount_Click(object sender, RoutedEventArgs e)
        {
            ShowAddAccount();
        }

        /// <summary>
        /// Occurs when the currently selected "Mailbox List" changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (lbMailboxList).</param>
        /// <param name="e">The event arguments (RoutedEventArgs).</param>
        private void lbMailboxList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Make sure something is selected
            if (lbMailboxList.SelectedItem != null)
            {
                // Set control in Add mode
                ListAccountsSettingsMode = ListAccountsSettingsMode.Edit;

                // Set the selected account settings data
                SelectedAccountSettingsData = (AccountSettingsData)lbMailboxList.SelectedItem;

                // Set the configure account data context to accountSettingsData
                cfConfigureAccount.DataContext = SelectedAccountSettingsData;
                esrblEmailService.SelectedItem = SelectedAccountSettingsData.EmailService;

                // Hide or show controls depending on mode
                HideShowControlsDependingOnMode();

                // Show configure account flyout
                cfConfigureAccount.IsOpen = true;
                _isDialogOpen = true;
            }
        }

        /// <summary>
        /// Occurs when the currently selected "Email Service Provider" changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (lbEmailServiceProvider).</param>
        /// <param name="e">The event arguments (SelectionChangedEventArgs).</param>
        private void lbEmailServiceProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbEmailServiceProvider.SelectedItem != null)
            {
                // Prepare configure account flyout
                EmailServiceProvider selectedEmailServiceProvider = ((AddAccountsSettingsEmailServiceProvider)lbEmailServiceProvider.SelectedItem).EmailServiceProvider;
                EmailServiceDialog emailServiceDialog = new EmailServiceDialog();
                emailServiceDialog.EmailServiceProvider = selectedEmailServiceProvider;
                emailServiceDialog.Show();
                
                // Hide add an account flyout
                cfAddAnAccountFlyout.IsOpen = false;
            }
        }

        /// <summary>
        /// Occurs when the Use The Same Username And Password To Send And Receive Email CheckBox is checked.
        /// </summary>
        /// <param name="sender">The object that raised the event (CheckBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail_Clicked(object sender, RoutedEventArgs e)
        {
            spOutgoingMailServerCredentials.Visibility = (cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible);
        }

        /// <summary>
        /// Occurs when the "Connect" button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (bConnect).</param>
        /// <param name="e">The event arguments (RoutedEventArgs).</param>
        private async void bSave_Click(object sender, RoutedEventArgs e)
        {
            // If the mail clients is found
            MailClient mailClient = null;
            if (MainPage.Current.MailClientsDictionary.TryGetValue(SelectedAccountSettingsData.EmailAddress, out mailClient))
            {
                // Declare the variables
                bool requiresReconnect = false;
                string accountName;
                string displayName;
                EmailService emailService;
                EmailServiceProvider emailServiceProvider;
                string emailAddress;
                string username;
                string password;
                string incomingEmailServer;
                ushort incomingEmailServerPort;
                bool incomingEmailServerRequiresSsl;
                string outgoingEmailServer;
                ushort outgoingEmailServerPort;
                bool outgoingEmailServerRequiresSsl;
                bool outgoingMailServerRequiresAuthentication;
                bool useTheSameUsernameAndPasswordToSendAndReceiveEmail;
                string sendUsername;
                string sendPassword;
                bool useAnEmailSignature;
                string emailSignature;
                DownloadNewEmailOptions downloadNewEmail;
                DownloadEmailsFromOptions downloadEmailsFrom;
                bool keepEmailCopiesOnServer;
                bool contentToSyncEmail;

                // If the connection info is valid
                if (IsConnectionInfoValid(out requiresReconnect, out accountName, out displayName, out emailService, out emailServiceProvider, out emailAddress, out username, out password, out incomingEmailServer, out incomingEmailServerPort, out incomingEmailServerRequiresSsl, out outgoingEmailServer, out outgoingEmailServerPort, out outgoingEmailServerRequiresSsl, out outgoingMailServerRequiresAuthentication, out useTheSameUsernameAndPasswordToSendAndReceiveEmail, out sendUsername, out sendPassword, out useAnEmailSignature, out emailSignature, out downloadNewEmail, out downloadEmailsFrom, out keepEmailCopiesOnServer, out contentToSyncEmail))
                {
                    // Get the account settings dat
                    AccountSettingsData accountSettingsData = new AccountSettingsData(accountName, emailService, emailServiceProvider, displayName, emailAddress, username, password, incomingEmailServer, incomingEmailServerPort, incomingEmailServerRequiresSsl, outgoingEmailServer, outgoingEmailServerPort, outgoingEmailServerRequiresSsl, outgoingMailServerRequiresAuthentication, useTheSameUsernameAndPasswordToSendAndReceiveEmail, sendUsername, sendPassword, useAnEmailSignature, emailSignature, downloadNewEmail, downloadEmailsFrom, keepEmailCopiesOnServer, contentToSyncEmail);

                    // If new settings require reconnect
                    if (requiresReconnect)
                    {
                        // Logout
                        if (mailClient.State.HasFlag(MailClientState.Authenticated))
                            await mailClient.Logout();

                        // Get the mail client
                        mailClient = await MailClient.GetMailClient(accountSettingsData);

                        // If login
                        if (mailClient.Login().IsSuccessfull)
                            MainPage.Current.ConnectMailbox(accountSettingsData, mailClient);
                    }

                    // Save the settings
                    mailClient.AccountSettingsData = accountSettingsData;
                    MainPage.Current.MailClientsDictionary[accountSettingsData.EmailAddress] = mailClient;
                    StorageSettings.AccountSettingsDataDictionary[accountSettingsData.EmailAddress] = accountSettingsData;
                    try
                    {
                        if (MainPage.Current.SelectedAccount.Key.Equals(accountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase))
                            MainPage.Current.AccountName = accountSettingsData.AccountName;
                    }
                    catch { }
                    MainPage.Current.BindMailboxes(accountSettingsData);
                    await StorageSettings.SaveAccountSettingsDataDictionary();
                }
            }
        }

        /// <summary>
        /// Occurs when the "Remove" button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (bRemove).</param>
        /// <param name="e">The event arguments (RoutedEventArgs).</param>
        private async void bRemove_Click(object sender, RoutedEventArgs e)
        {
            // Create the confirm message dialog
            MessageDialog confirm = new MessageDialog("Are you sure you want to delete this mailbox?", "Mailbox Delete Confirmation");
            confirm.Commands.Add(new UICommand("OK"));
            confirm.Commands.Add(new UICommand("Cancel"));
            confirm.DefaultCommandIndex = 0;
            confirm.CancelCommandIndex = 1;

            // If OK was clicked
            if (await confirm.ShowAsync() == confirm.Commands[0])
            {
                // Delete the mailbox and save
                await MainPage.Current.RemoveAccount(SelectedAccountSettingsData);
            }
        }

        /// <summary>
        /// Shows the control in list account mode.
        /// </summary>
        public void ShowListAccount(string emailAddress = null)
        {
            // Show list accounts flyout
            cfAccountsFlyout.IsOpen = true;

            // If an email address is supplied
            if (!string.IsNullOrEmpty(emailAddress))
                lbMailboxList.SelectedItem = lbMailboxList.Items.Cast<AccountSettingsData>().FirstOrDefault(o => o.EmailAddress.Equals(emailAddress, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Shows the control in add account mode.
        /// </summary>
        public void ShowAddAccount()
        {
            if (!MainPage.Current.IsDialogOpen)
            {
                // Set control in Add mode
                ListAccountsSettingsMode = ListAccountsSettingsMode.Add;

                // Add the email service providers
                List<AddAccountsSettingsEmailServiceProvider> addAccountsSettingsEmailServiceProvider = new List<AddAccountsSettingsEmailServiceProvider>();
                foreach (string emailServiceProvider in Enum.GetNames(typeof(EmailServiceProvider)))
                    addAccountsSettingsEmailServiceProvider.Add(new AddAccountsSettingsEmailServiceProvider((EmailServiceProvider)Enum.Parse(typeof(EmailServiceProvider), emailServiceProvider), string.Format("/Assets/{0}.png", emailServiceProvider)));
                lbEmailServiceProvider.ItemsSource = addAccountsSettingsEmailServiceProvider;

                // Hide or show controls depending on mode
                HideShowControlsDependingOnMode();

                // Show add account flyout
                cfAddAnAccountFlyout.IsOpen = true;
            }
        }

        /// <summary>
        /// Determines if the supplied connection info is valid.
        /// </summary>
        /// <param name="requiresReconnect">true, if requires reconnect; otherwise, false.</param>
        /// <param name="accountName">The account name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="emailServiceProvider">The email service provider.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="incomingEmailServer">The incoming email server.</param>
        /// <param name="incomingEmailServerPort">The incoming email server port.</param>
        /// <param name="incomingEmailServerRequiresSsl">true, if the incoming email server requires ssl; otherwise, false.</param>
        /// <param name="outgoingEmailServer">The outgoing email server.</param>
        /// <param name="outgoingEmailServerPort">The outgoing email server port.</param>
        /// <param name="outgoingEmailServerRequiresSsl">true, if the outgoing email server requires ssl; otherwise, false.</param>
        /// <param name="outgoingMailServerRequiresAuthentication">true, if the outgoing email server requires authentication; otherwise false.</param>
        /// <param name="useTheSameUsernameAndPasswordToSendAndReceiveEmail">true, if the same username and password to send and receive email; otherwise false.</param>
        /// <param name="sendUsername">The smtp user name.</param>
        /// <param name="sendPassword">The smtp password.</param>
        /// <param name="useAnEmailSignature">true, if use an email signature; otherwise, false.</param>
        /// <param name="emailSignature">The email signature.</param>
        /// <param name="downloadNewEmail">The download new email options.</param>
        /// <param name="downloadEmailsFrom">The download emails from options.</param>
        /// <param name="keepEmailCopiesOnServer">true, if keep email copies on server.</param>
        /// <param name="contentToSyncEmail">true, if emails should be synced; otherwise, false.</param>
        /// <returns>true, if connection info is valid; otherwise, false.</returns>
        private bool IsConnectionInfoValid(out bool requiresReconnect, out string accountName, out string displayName, out EmailService emailService, out EmailServiceProvider emailServiceProvider, out string emailAddress, out string username, out string password, out string incomingEmailServer, out ushort incomingEmailServerPort, out bool incomingEmailServerRequiresSsl, out string outgoingEmailServer, out ushort outgoingEmailServerPort, out bool outgoingEmailServerRequiresSsl, out bool outgoingMailServerRequiresAuthentication, out bool useTheSameUsernameAndPasswordToSendAndReceiveEmail, out string sendUsername, out string sendPassword, out bool useAnEmailSignature, out string emailSignature, out DownloadNewEmailOptions downloadNewEmail, out DownloadEmailsFromOptions downloadEmailsFrom, out bool keepEmailCopiesOnServer, out bool contentToSyncEmail)
        {
            // Default variables
            requiresReconnect = false;
            accountName = txtAccountName.Text.Trim();
            displayName = txtDisplayName.Text.Trim();
            emailService = SelectedAccountSettingsData.EmailService;
            emailServiceProvider = SelectedAccountSettingsData.EmailServiceProvider;
            emailAddress = txtEmailAddress.Text.Trim();
            username = txtUsername.Text.Trim();
            password = pbPassword.Password.Trim();
            incomingEmailServer = txtIncomingEmailServer.Text.Trim();
            incomingEmailServerPort = 0;
            ushort.TryParse(txtIncomingEmailServerPort.Text.Trim(), out incomingEmailServerPort);
            incomingEmailServerRequiresSsl = cbIncomingEmailServerRequiresSsl.IsChecked.Value;
            outgoingEmailServer = txtOutgoingEmailServer.Text.Trim();
            outgoingEmailServerPort = 0;
            ushort.TryParse(txtOutgoingEmailServerPort.Text.Trim(), out outgoingEmailServerPort);
            outgoingEmailServerRequiresSsl = cbOutgoingEmailServerRequiresSsl.IsChecked.Value;
            outgoingMailServerRequiresAuthentication = cbOutgoingMailServerRequiresAuthentication.IsChecked.Value;
            useTheSameUsernameAndPasswordToSendAndReceiveEmail = cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.Value;
            sendUsername = txtSendUsername.Text.Trim();
            sendPassword = pbSendPassword.Password.Trim();
            useAnEmailSignature = tsUseAnEmailSignature.IsOn;
            tbEmailSignature.Document.GetText(TextGetOptions.FormatRtf, out emailSignature);
            downloadNewEmail = (DownloadNewEmailOptions)Enum.Parse(typeof(DownloadNewEmailOptions), (string)cbDownloadNewEmail.SelectedValue, true);
            downloadEmailsFrom = (DownloadEmailsFromOptions)Enum.Parse(typeof(DownloadEmailsFromOptions), (string)cbDownloadEmailsFrom.SelectedValue, true);
            keepEmailCopiesOnServer = cbKeepEmailCopiesOnServer.IsChecked.Value;
            contentToSyncEmail = cbContentToSyncEmail.IsChecked.Value;

            // If any connection info has changed
            if (!username.Equals(SelectedAccountSettingsData.UserName, StringComparison.OrdinalIgnoreCase)
             || !password.Equals(SelectedAccountSettingsData.Password, StringComparison.OrdinalIgnoreCase)
             || !incomingEmailServer.Equals(SelectedAccountSettingsData.IncomingMailServer, StringComparison.OrdinalIgnoreCase)
             || incomingEmailServerPort != SelectedAccountSettingsData.IncomingMailServerPort
             || incomingEmailServerRequiresSsl != SelectedAccountSettingsData.IsIncomingMailServerSsl
             || !outgoingEmailServer.Equals(SelectedAccountSettingsData.OutgoingMailServer, StringComparison.OrdinalIgnoreCase)
             || outgoingEmailServerPort != SelectedAccountSettingsData.OutgoingMailServerPort
             || outgoingEmailServerRequiresSsl != SelectedAccountSettingsData.IsOutgoingMailServerSsl
             || outgoingMailServerRequiresAuthentication != SelectedAccountSettingsData.OutgoingMailServerRequiresAuthentication
             || useTheSameUsernameAndPasswordToSendAndReceiveEmail != SelectedAccountSettingsData.IsSendAndReceiveUserNameAndPasswordSame
             || !sendUsername.Equals(SelectedAccountSettingsData.SendUserName, StringComparison.OrdinalIgnoreCase)
             || !sendPassword.Equals(SelectedAccountSettingsData.SendPassword, StringComparison.OrdinalIgnoreCase))
                requiresReconnect = true;

            // Validate email address
            if (string.IsNullOrEmpty(emailAddress)
             || !Regex.IsMatch(emailAddress, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                txtEmailAddress.Focus(FocusState.Programmatic);
                return false;
            }
            // Validate password
            else if (string.IsNullOrEmpty(password))
            {
                pbPassword.Focus(FocusState.Programmatic);
                return false;
            }
            // Validate incoming email server
            else if (string.IsNullOrEmpty(incomingEmailServer))
            {
                txtIncomingEmailServer.Focus(FocusState.Programmatic);
                return false;
            }
            // Validate incoming email server port
            else if (incomingEmailServerPort == 0)
            {
                txtIncomingEmailServerPort.Focus(FocusState.Programmatic);
                return false;
            }
            // Validate outgoing email server
            else if (string.IsNullOrEmpty(outgoingEmailServer))
            {
                txtOutgoingEmailServer.Focus(FocusState.Programmatic);
                return false;
            }
            // Validate outgoing email server port
            else if (outgoingEmailServerPort == 0)
            {
                txtOutgoingEmailServerPort.Focus(FocusState.Programmatic);
                return false;
            }
            else if (!cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.Value)
            {
                // Validate smtp user name
                if (string.IsNullOrEmpty(sendUsername))
                {
                    txtSendUsername.Focus(FocusState.Programmatic);
                    return false;
                }
                // Validate smtp password
                else if (string.IsNullOrEmpty(sendPassword))
                {
                    pbSendPassword.Focus(FocusState.Programmatic);
                    return false;
                }
            }

            // If we reach this point, all connection info is valid
            return true;
        }

        /// <summary>
        /// Occurs when the "Insert Signature" button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bInsertSignature_Click(object sender, RoutedEventArgs e)
        {
            // Create the file open picker
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.CommitButtonText = "Select File";
            fileOpenPicker.FileTypeFilter.Add(".htm");
            fileOpenPicker.FileTypeFilter.Add(".html");
            fileOpenPicker.FileTypeFilter.Add(".txt");
            fileOpenPicker.FileTypeFilter.Add(".rtf");
            fileOpenPicker.SettingsIdentifier = "SignaturePicker";
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fileOpenPicker.ViewMode = PickerViewMode.List;

            try
            {
                // Get the selected file
                StorageFile file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    // Get the file contents
                    string fileContents = await FileIO.ReadTextAsync(file, UnicodeEncoding.Utf8);
                    // Determine what type of file we are working with
                    switch (Path.GetExtension(file.Name).ToLower())
                    {
                        // Html
                        case ".htm":
                        case ".html":
                            HTML2RTF html2Rtf = new HTML2RTF(fileContents);
                            tbEmailSignature.Document.SetText(TextSetOptions.FormatRtf, html2Rtf.RTF);
                            break;

                        // Rtf
                        case ".rtf":
                            tbEmailSignature.Document.SetText(TextSetOptions.FormatRtf, fileContents);
                            break;

                        // Anything else
                        default:
                            tbEmailSignature.Document.SetText(TextSetOptions.None, fileContents);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }

            // Make sure configure account flyout is open
            cfConfigureAccount.IsOpen = true;
        }
    }
}
