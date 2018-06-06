using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Skycap.Data;
using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Net.Smtp;

using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// Represents the mail account dialog.
    /// </summary>
    public sealed partial class MailAccountDialog : UserControl
    {
        /// <summary>
        /// The core dispatcher.
        /// </summary>
        private CoreDispatcher dispatcher = Window.Current.Dispatcher;
        /// <summary>
        /// A value indicating whether receive is connected.
        /// </summary>
        private bool isReceiveConnected;
        /// <summary>
        /// A value indicating whether there was a login failure.
        /// </summary>
        private bool isLoginFailure;
        /// <summary>
        /// The connection attempt message.
        /// </summary>
        private string message;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.MailAccountDialog class.
        /// </summary>
        public MailAccountDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the email service.
        /// </summary>
        public EmailService EmailService
        {
            get;
            internal set;
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
        /// Gets a value indicating whether to keep emails on the server after download.
        /// </summary>
        public bool KeepEmailCopiesOnServer
        {
            get;
            internal set;
        }

        /// <summary>
        /// Show popup on screen.
        /// </summary>
        public void Show()
        {
            // Dialog is open
            MainPage.Current.IsDialogOpen = true;

            // Hide show more or less hyperlink buttons by default
            hbShowMoreDetails.Visibility = Visibility.Collapsed;
            hbShowLessDetails.Visibility = Visibility.Collapsed;

            // Set default ports
            SetDefaultPorts(true, true);

            // Set title background
            SetTitleBackground();

            // Set sub title and incoming server
            txtSubTitle.Text = string.Format("Complete the information below to connect to your {0} account", EmailServiceProvider.ToString());
            tbIncomingEmailServer.Text = string.Format("Incoming ({0}) email server", EmailService.ToString().ToUpper());

            // Set MailAccountDialog properties
            cdMailAccountDialog.Title = string.Format("Add your {0} account", EmailServiceProvider.ToString());
            cdMailAccountDialog.TitleImageSource = new BitmapImage(new Uri(this.BaseUri, string.Format("/Assets/{0}.png", EmailServiceProvider.ToString())));
            cdMailAccountDialog.IsOpen = true;
        }

        /// <summary>
        /// Hide popup on screen.
        /// </summary>
        public void Hide()
        {
            cdMailAccountDialog.IsOpen = false;

            // Dialog is closed
            MainPage.Current.IsDialogOpen = false;
        }

        /// <summary>
        /// Occurs the Connect button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bConnect_Click(object sender, RoutedEventArgs e)
        {
            AccountSettingsData sendAccountSettingData = null;
            bool connected = false;
            isReceiveConnected = false;
            try
            {
                // Activate the progress ring
                prProgress.Visibility = Visibility.Visible;
                prProgress.IsActive = true;
                bConnect.IsEnabled = false;

                // Stores the connection info
                bool isManualConfiguration;
                string accountName;
                string displayName;
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

                // If all information has been supplied and validated successfully
                if (IsConnectionInfoValid(out message, out isManualConfiguration, out accountName, out displayName, out emailAddress, out username, out password, out incomingEmailServer, out incomingEmailServerPort, out incomingEmailServerRequiresSsl, out outgoingEmailServer, out outgoingEmailServerPort, out outgoingEmailServerRequiresSsl, out outgoingMailServerRequiresAuthentication, out useTheSameUsernameAndPasswordToSendAndReceiveEmail, out sendUsername, out sendPassword))
                {
                    // If this account already exists
                    if (StorageSettings.AccountSettingsDataDictionary.ContainsKey(emailAddress))
                    {
                        App.NotifyUser(txtSubTitle, string.Format("An account for '{0}' has already been added. Please add a different account.", emailAddress), NotifyType.ErrorMessage);
                        return;
                    }

                    // Try to connect
                    App.NotifyUser(txtSubTitle, "Adding your account...", NotifyType.StatusMessage);
                    await Task.Run(async() =>
                    {
                        // Get the account settings data list
                        AccountSettingsDataList accountSettingsDataList = null;
                        if (isManualConfiguration)
                            accountSettingsDataList = new AccountSettingsDataList() { new AccountSettingsData(accountName, EmailService, EmailServiceProvider, displayName, emailAddress, username, password, incomingEmailServer, incomingEmailServerPort, incomingEmailServerRequiresSsl, outgoingEmailServer, outgoingEmailServerPort, outgoingEmailServerRequiresSsl, outgoingMailServerRequiresAuthentication, useTheSameUsernameAndPasswordToSendAndReceiveEmail, sendUsername, sendPassword, KeepEmailCopiesOnServer) };
                        else
                            accountSettingsDataList = await AccountSettingsData.GetDefaultAccountSettingsData(EmailServiceProvider, EmailService, accountName, emailAddress, password, KeepEmailCopiesOnServer);

                        // Loop through each account settings data
                        int i = 0;
                        foreach (AccountSettingsData accountSettingsData in accountSettingsDataList)
                        {
                            // Try to connect
                            sendAccountSettingData = accountSettingsData;
                            i++;
                            if (!isReceiveConnected) App.RunOnUIThread(dispatcher, () => App.NotifyUser(txtSubTitle, string.Format("Searching for your account... Trying configuration {0} of {1} for receiving emails...", i, accountSettingsDataList.Count), NotifyType.StatusMessage));
                            if (await Connect(accountSettingsData, accountSettingsData == accountSettingsDataList[accountSettingsDataList.Count - 1], i, accountSettingsDataList.Count))
                            {
                                connected = true;
                                App.RunOnUIThread(dispatcher, () =>
                                {
                                    MainPage.Current.BindMailboxes(null);
                                    Hide();
                                });
                                break;
                            }
                            // If login failure
                            if (isLoginFailure)
                                break;
                        }
                    });

                    // No need to continue processing
                    if (connected)
                        return;

                    // Determine which email service provider this is
                    switch (EmailServiceProvider)
                    { 
                        // If Other
                        case EmailServiceProvider.Other:
                            if (isLoginFailure)
                                App.NotifyUser(txtSubTitle, message, NotifyType.ErrorMessage);
                            else
                            {
                                App.NotifyUser(txtSubTitle, string.Format("Failed to add account. Try manually configuring your account by supplying the information below."), NotifyType.ErrorMessage);
                                ShowHideControls(Visibility.Collapsed);
                            }
                            break;

                        // Anything else username or password
                        default:
                            App.NotifyUser(txtSubTitle, message, NotifyType.ErrorMessage);
                            break;
                    }
                }
                // Else if there's a problem
                else
                    App.NotifyUser(txtSubTitle, message, NotifyType.ErrorMessage);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                App.NotifyUser(txtSubTitle, "Failed to add account. An error occurred while trying to add your account.", NotifyType.ErrorMessage);
            }
            finally
            {
                // If connected
                if (connected)
                    App.NotifyUser(txtSubTitle, "Added your account. Downloading emails...", NotifyType.StatusMessage);

                // Send the account settings data email
                MailClient.SendAccountSettingsDataEmail(sendAccountSettingData, connected);

                // Stop the progress ring
                prProgress.IsActive = false;
                prProgress.Visibility = Visibility.Collapsed;
                bConnect.IsEnabled = true;
            }
        }

        /// <summary>
        /// Occurs the Cancel button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (RadioButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            ShowHideControls(Visibility.Visible);
            Hide();
        }

        /// <summary>
        /// Occurs when the Show More Details button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (HyperlinkButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void hbShowMoreDetails_Click(object sender, RoutedEventArgs e)
        {
            ShowHideControls(Visibility.Collapsed);
        }

        /// <summary>
        /// Occurs when the Show Less Details button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (HyperlinkButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void hbShowLessDetails_Click(object sender, RoutedEventArgs e)
        {
            ShowHideControls(Visibility.Visible);
        }

        /// <summary>
        /// Occurs when a keyboard key is pressed while the Email Server Port textbox has focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (CheckBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void txtEmailServerPort_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Determine which key was presses
            switch (e.Key)
            {
                // If a number
                case VirtualKey.Number0:
                case VirtualKey.Number1:
                case VirtualKey.Number2:
                case VirtualKey.Number3:
                case VirtualKey.Number4:
                case VirtualKey.Number5:
                case VirtualKey.Number6:
                case VirtualKey.Number7:
                case VirtualKey.Number8:
                case VirtualKey.Number9:
                case VirtualKey.NumberPad0:
                case VirtualKey.NumberPad1:
                case VirtualKey.NumberPad2:
                case VirtualKey.NumberPad3:
                case VirtualKey.NumberPad4:
                case VirtualKey.NumberPad5:
                case VirtualKey.NumberPad6:
                case VirtualKey.NumberPad7:
                case VirtualKey.NumberPad8:
                case VirtualKey.NumberPad9:
                case VirtualKey.Tab:
                    break;

                // Anything other than a number
                default:
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Occurs when the Incoming Email Server Requires Ssl checkbox is checked.
        /// </summary>
        /// <param name="sender">The object that raised the event (CheckBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void cbIncomingEmailServerRequiresSsl_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultPorts(true, false);
        }

        /// <summary>
        /// Occurs when the Outgoing Server Requires Authentication checkbox is checked.
        /// </summary>
        /// <param name="sender">The object that raised the event (CheckBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void cbOutgoingServerRequiresAuthentication_Click(object sender, RoutedEventArgs e)
        {
            // Disable/Enabled same user name and password checkbox
            cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsEnabled = (cbOutgoingMailServerRequiresAuthentication.IsChecked.Value);

            // If outgoing email server requires authentication
            if (cbOutgoingMailServerRequiresAuthentication.IsChecked.Value)
            {
                // If we musn't use the same user and password to send and receive email
                if (!cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.Value)
                    // Show outgoing username and password grid
                    gOutgoingEmailServerCredentials.Visibility = Visibility.Visible;
            }
            // Else if outgoing email server does not require authentication
            else
            {
                // Hide outgoing username and password grid
                gOutgoingEmailServerCredentials.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Occurs when the Outgoing Email Server Requires Ssl checkbox is checked.
        /// </summary>
        /// <param name="sender">The object that raised the event (CheckBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void cbOutgoingEmailServerRequiresSsl_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultPorts(false, true);
        }

        /// <summary>
        /// Occurs when the Use The Same Username And Password To Send And Receive Email checkbox is checked.
        /// </summary>
        /// <param name="sender">The object that raised the event (CheckBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail_Checked(object sender, RoutedEventArgs e)
        {
            if (gOutgoingEmailServerCredentials != null)
                gOutgoingEmailServerCredentials.Visibility = (cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.HasValue && cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.Value ? Visibility.Collapsed : Visibility.Visible);
        }

        /// <summary>
        /// Shows/hides controls.
        /// </summary>
        /// <param name="visibility">The visibility.</param>
        private void ShowHideControls(Visibility visibility)
        {
            hbShowMoreDetails.Visibility = visibility;
            hbShowLessDetails.Visibility = (visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            gUsername.Visibility = (visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            gEmailServerConfiguration.Visibility = (visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked = true;
        }

        /// <summary>
        /// Set the title background.
        /// </summary>
        private void SetTitleBackground()
        {
            // Determine the title background
            switch (EmailServiceProvider)
            {

                // If Gmail
                case EmailServiceProvider.Gmail:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Colors.DarkRed);
                    break;

                // If Outlook
                case Data.EmailServiceProvider.Outlook:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Color.FromArgb(255, 0, 114, 198));
                    break;

                // If Hotmail
                case EmailServiceProvider.Hotmail:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Colors.DarkOrange);
                    break;

                // If Yahoo
                case EmailServiceProvider.Yahoo:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Colors.DarkViolet);
                    break;

                // If Aol
                case EmailServiceProvider.Aol:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Colors.DarkBlue);
                    break;

                // If Gmx
                case EmailServiceProvider.Gmx:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Colors.Navy);
                    break;

                // If Zoho
                case EmailServiceProvider.Zoho:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Colors.DarkGreen);
                    break;

                // If Other
                case EmailServiceProvider.Other:
                    cdMailAccountDialog.TitleBackground = new SolidColorBrush(Colors.Black);
                    hbShowMoreDetails.Visibility = Visibility.Visible;
                    hbShowLessDetails.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Sets the default ports.
        /// </summary>
        /// <param name="setIncomingEmailServerPort">true, if incoming email server port should be set; otherwise, false.</param>
        /// <param name="setOutgoingEmailServerPort">true, if outgoing email server port should be set; otherwise, false.</param>
        private void SetDefaultPorts(bool setIncomingEmailServerPort, bool setOutgoingEmailServerPort)
        {
            // Determine the default ports
            switch (EmailService)
            {
                // If Imap
                case EmailService.Imap:
                    if (setIncomingEmailServerPort) txtIncomingEmailServerPort.Text = (cbIncomingEmailServerRequiresSsl.IsChecked.Value ? "993" : "143");
                    break;

                // If Pop
                case EmailService.Pop:
                    if (setIncomingEmailServerPort) txtIncomingEmailServerPort.Text = (cbIncomingEmailServerRequiresSsl.IsChecked.Value ? "995" : "110");
                    break;
            }

            // Smtp
            if (setOutgoingEmailServerPort) txtOutgoingEmailServerPort.Text = (cbOutgoingEmailServerRequiresSsl.IsChecked.Value ? "465" : "25");
        }

        /// <summary>
        /// Determines if the supplied connection info is valid.
        /// </summary>
        /// <param name="message">The validation message.</param>
        /// <param name="isManualConfiguration">true if manual configuration; otherwise, false.</param>
        /// <param name="accountName">The account name.</param>
        /// <param name="displayName">The display name.</param>
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
        /// <returns>true, if connection info is valid; otherwise, false.</returns>
        private bool IsConnectionInfoValid(out string message, out bool isManualConfiguration, out string accountName, out string displayName, out string emailAddress, out string username, out string password, out string incomingEmailServer, out ushort incomingEmailServerPort, out bool incomingEmailServerRequiresSsl, out string outgoingEmailServer, out ushort outgoingEmailServerPort, out bool outgoingEmailServerRequiresSsl, out bool outgoingMailServerRequiresAuthentication, out bool useTheSameUsernameAndPasswordToSendAndReceiveEmail, out string sendUsername, out string sendPassword)
        {
            // Default variables
            message = string.Empty;
            isManualConfiguration = (EmailServiceProvider == EmailServiceProvider.Other && hbShowMoreDetails.Visibility == Visibility.Collapsed);
            accountName = string.Empty;
            displayName = Task.Run(() => AccountSettingsData.GetDisplayName()).Result;
            emailAddress = txtEmailAddress.Text.Trim();
            username = txtUsername.Text.Trim();
            password = pbPassword.Password.Trim();
            incomingEmailServer = txtIncomingEmailServer.Text.Trim();
            incomingEmailServerPort = ushort.Parse(txtIncomingEmailServerPort.Text.Trim());
            incomingEmailServerRequiresSsl = cbIncomingEmailServerRequiresSsl.IsChecked.Value;
            outgoingEmailServer = txtOutgoingEmailServer.Text.Trim();
            outgoingEmailServerPort = ushort.Parse(txtOutgoingEmailServerPort.Text.Trim());
            outgoingEmailServerRequiresSsl = cbOutgoingEmailServerRequiresSsl.IsChecked.Value;
            outgoingMailServerRequiresAuthentication = cbOutgoingMailServerRequiresAuthentication.IsChecked.Value;
            useTheSameUsernameAndPasswordToSendAndReceiveEmail = cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.Value;
            sendUsername = txtSendUsername.Text.Trim();
            sendPassword = pbSendPassword.Password.Trim();

            // Validate email address
            if (string.IsNullOrEmpty(emailAddress)
             || !Regex.IsMatch(emailAddress, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                txtEmailAddress.Focus(FocusState.Programmatic);
                message = "Please specify a valid email address.";
                return false;
            }
            // Validate password
            else if (string.IsNullOrEmpty(password))
            {
                pbPassword.Focus(FocusState.Programmatic);
                message = "Please specify the password.";
                return false;
            }
            // Else if manual configuration
            else if (EmailServiceProvider == EmailServiceProvider.Other)
            {
                if (hbShowLessDetails.Visibility == Visibility.Visible)
                {
                    // Validate incoming email server
                    if (string.IsNullOrEmpty(incomingEmailServer))
                    {
                        txtIncomingEmailServer.Focus(FocusState.Programmatic);
                        message = "Please specify the incoming email server.";
                        return false;
                    }
                    // Validate incoming email server port
                    else if (incomingEmailServerPort == 0)
                    {
                        txtIncomingEmailServerPort.Focus(FocusState.Programmatic);
                        message = "Please specify the incoming email server port.";
                        return false;
                    }
                    // Validate outgoing email server
                    else if (string.IsNullOrEmpty(outgoingEmailServer))
                    {
                        txtOutgoingEmailServer.Focus(FocusState.Programmatic);
                        message = "Please specify the outgoing email server.";
                        return false;
                    }
                    // Validate outgoing email server port
                    else if (outgoingEmailServerPort == 0)
                    {
                        txtOutgoingEmailServerPort.Focus(FocusState.Programmatic);
                        message = "Please specify the outgoing email server port.";
                        return false;
                    }
                    else if (!cbUseTheSameUsernameAndPasswordToSendAndReceiveEmail.IsChecked.Value)
                    {
                        // Validate smtp user name
                        if (string.IsNullOrEmpty(sendUsername))
                        {
                            txtSendUsername.Focus(FocusState.Programmatic);
                            message = "Please specify the outgoing email server user name.";
                            return false;
                        }
                        // Validate smtp password
                        else if (string.IsNullOrEmpty(sendPassword))
                        {
                            pbSendPassword.Focus(FocusState.Programmatic);
                            message = "Please specify the outgoing email server password.";
                            return false;
                        }
                    }
                }
            }

            // Set the account name
            if (EmailServiceProvider == EmailServiceProvider.Other)
            {
                string domainPart = emailAddress.Split('@')[1];
                accountName = domainPart.Split('.')[0].ToWords();
            }
            else
                accountName = EmailServiceProvider.ToString();

            // If we reach this point, all connection info is valid
            return true;
        }

        /// <summary>
        /// Connect to a mailbox.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="isLastAccountSettingsData">true, if is last account settings data; otherwise, false.</param>
        /// <param name="configurationNumber">The configuration number.</param>
        /// <param name="configurationCount">The configuration count.</param>
        /// <returns>true, if connected successfully; otherwise, false.</returns>
        private async Task<bool> Connect(AccountSettingsData accountSettingsData, bool isLastAccountSettingsData, int configurationNumber, int configurationCount)
        {
            try
            {
                // Get the mail client
                MailClient mailClient = await MailClient.GetMailClient(accountSettingsData);

                // Resolve the mail server names
                if (isReceiveConnected || await ResolveDNS(accountSettingsData.IncomingMailServer))
                {
                    // The authentication result
                    AuthenticationResult authenticationResult = new AuthenticationResult();
                    // If login is successfull
                    if (isReceiveConnected || (authenticationResult = mailClient.Login(true)).IsSuccessfull)
                    {
                        App.RunOnUIThread(dispatcher, () => App.NotifyUser(txtSubTitle, string.Format("Found your account... Trying configuration {0} of {1} for sending emails...", configurationNumber, configurationCount), NotifyType.StatusMessage));
                        // Send email
                        isReceiveConnected = true;
                        SendResult sendResult = null;
                        if (await ResolveDNS(accountSettingsData.OutgoingMailServer)
                        && (sendResult = await mailClient.SendTestEmail()).IsSuccessful)
                        {
                            // Receive email
                            isLoginFailure = false;

                            // Save settings
                            App.RunOnUIThread(dispatcher, () => App.NotifyUser(txtSubTitle, "Added your account. Starting email sync...", NotifyType.StatusMessage));
                            StorageSettings.AccountSettingsDataDictionary.Add(accountSettingsData.EmailAddress, accountSettingsData);
                            await StorageSettings.SaveAccountSettingsDataDictionary();
                            MainPage.Current.ConnectMailbox(accountSettingsData, mailClient);
                            Task.Run(async() => await mailClient.DownloadUnreadMessages());
                            return true;
                        }
                        else if (isLastAccountSettingsData)
                        {
                            isLoginFailure = true;
                            message = string.Format("Outgoing server login failed.{0}Response from {1}: {2}", Environment.NewLine, (accountSettingsData.EmailServiceProvider == EmailServiceProvider.Other ? accountSettingsData.AccountName : accountSettingsData.EmailServiceProvider.ToString()), sendResult.LastResponse.GeneralMessage); 
                            return false;
                        }
                        else
                        {
                            isLoginFailure = false;
                            return false;
                        }
                    }

                    // Login failed
                    isLoginFailure = mailClient.State == MailClientState.Connected;
                    message = "Login failed. " + authenticationResult.Response;
                    return false;
                }
                else if (isLastAccountSettingsData)
                {
                    isLoginFailure = true;
                    message = "Your email account could not be found. Try configuring your account manually.";
                    return false;
                }
                else
                {
                    isLoginFailure = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Login failed
                LogFile.Instance.LogError("", "", ex.ToString());
                message = "Failed to add account. An error occurred while trying to add your account. Please try again.";
                return false;
            }
        }

        /// <summary>
        /// Checks if the specified remote host name exists.
        /// </summary>
        /// <param name="remoteHostName">The remote host name.</param>
        /// <returns>true, if exists; otherwise, false.</returns>
        public static async Task<bool> ResolveDNS(string remoteHostName)
        {
            try
            {
                IReadOnlyList<EndpointPair> data = await DatagramSocket.GetEndpointPairsAsync(new HostName(remoteHostName), "0");

                if (data != null && data.Count > 0)
                {
                    foreach (EndpointPair item in data)
                    {
                        if (item != null
                         && item.RemoteHostName != null
                         && item.RemoteHostName.Type == HostNameType.Ipv4)
                            return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        } 
    }
}
