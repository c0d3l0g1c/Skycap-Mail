using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Skycap.IO;
using Skycap.Net.Common;
using Windows.System.UserProfile;
using Windows.UI.Xaml;

namespace Skycap.Data
{
    /// <summary>
    /// Represents the account settings data.
    /// </summary>
    [DataContract]
    public sealed class AccountSettingsData : INotifyPropertyChanged, IEquatable<AccountSettingsData>
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The image source.
        /// </summary>
        private string _imageSource;
        /// <summary>
        /// The account name.
        /// </summary>
        private string _accountName;
        /// <summary>
        /// The email service.
        /// </summary>
        private EmailService _emailService;
        /// <summary>
        /// The email service provider.
        /// </summary>
        private EmailServiceProvider _emailServiceProvider;
        /// <summary>
        /// The email display name.
        /// </summary>
        private string _displayName;
        /// <summary>
        /// The email address.
        /// </summary>
        private string _emailAddress;
        /// <summary>
        /// The user name.
        /// </summary>
        private string _userName;
        /// <summary>
        /// The email password.
        /// </summary>
        private string _password;
        /// <summary>
        /// The incoming mail server.
        /// </summary>
        private string _incomingMailServer;
        /// <summary>
        /// The incoming mail server port.
        /// </summary>
        private ushort _incomingMailServerPort;
        /// <summary>
        /// Gets or sets a value indicating whether the incoming mail server uses ssl.
        /// </summary>
        private bool _isIncomingMailServerSsl;
        /// <summary>
        /// The outgoing mail server.
        /// </summary>
        private string _outgoingMailServer;
        /// <summary>
        /// The outgoing mail server port.
        /// </summary>
        private ushort _outgoingMailServerPort;
        /// <summary>
        /// A value indicating whether the outgoing mail server uses ssl.
        /// </summary>
        private bool _isOutgoingMailServerSsl;
        /// <summary>
        /// A value indicating whether the outgoing server requires authentication.
        /// </summary>
        private bool _outgoingServerRequiresAuthentication;
        /// <summary>
        /// A value indicating whether the send and receive user name and password is the same. 
        /// </summary>
        private bool _isSendAndReceiveUserNameAndPasswordSame;
        /// <summary>
        /// The send email user name.
        /// </summary>
        private string _sendUserName;
        /// <summary>
        /// The send email password.
        /// </summary>
        private string _sendPassword;
        /// <summary>
        /// A value indicating whether an email signature should be used.
        /// </summary>
        private bool _useAnEmailSignature;
        /// <summary>
        /// The email signature.
        /// </summary>
        private string _emailSignature;
        /// <summary>
        /// The download new email options.
        /// </summary>
        private DownloadNewEmailOptions _downloadNewEmail;
        /// <summary>
        /// The download emails from options.
        /// </summary>
        private DownloadEmailsFromOptions _downloadEmailsFrom;
        /// <summary>
        /// A value indicating whether emails should be kept on the email server after download.
        /// </summary>
        private bool _keepEmailCopiesOnServer;
        /// <summary>
        /// A value indicating whether email content should be synced.
        /// </summary>
        private bool _contentToSyncEmail;
        /// <summary>
        /// The unread message count.
        /// </summary>
        private int _unreadMessageCount;

        /// <summary>
        /// Initialises a new instance of the Skycap.Data.AccountSettingsData class.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="emailServiceProvider">The email service provider.</param>
        /// <param name="displayName">The email display name.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="userName">The email user name.</param>
        /// <param name="password">The email password.</param>
        /// <param name="incomingMailServer">The incoming mail server.</param>
        /// <param name="incomingMailServerPort">The incoming mail server port.</param>
        /// <param name="outgoingMailServer">The incoming mail server.</param>
        /// <param name="outgoingMailServerPort">The incoming mail server port.</param>
        /// <param name="keepEmailsOnServerAfterDownload">true, if emails should be kept on the server after download.</param>
        public AccountSettingsData(string accountName, EmailService emailService, EmailServiceProvider emailServiceProvider, string displayName, string emailAddress, string userName, string password, string incomingMailServer, ushort incomingMailServerPort, string outgoingMailServer, ushort outgoingMailServerPort, bool keepEmailsOnServerAfterDownload)
            : this(accountName, emailService, emailServiceProvider, displayName, emailAddress, userName, password, incomingMailServer, incomingMailServerPort, true, outgoingMailServer, outgoingMailServerPort, true, true, true, userName, password, true, MailMessage.DefaultSignature, DownloadNewEmailOptions.AsItemsArrive, DownloadEmailsFromOptions.Anytime, keepEmailsOnServerAfterDownload, true)
        {

        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Data.AccountSettingsData class.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="emailServiceProvider">The email service provider.</param>
        /// <param name="displayName">The email display name.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The email password.</param>
        /// <param name="incomingMailServer">The incoming mail server.</param>
        /// <param name="incomingMailServerPort">The incoming mail server port.</param>
        /// <param name="isIncomingMailServerSsl">A value indicating whether the incoming mail server uses ssl.</param>
        /// <param name="outgoingMailServer">The incoming mail server.</param>
        /// <param name="outgoingMailServerPort">The incoming mail server port.</param>
        /// <param name="isOutgoingMailServerSsl">A value indicating whether the incoming mail server uses ssl.</param>
        /// <param name="outgoingMailServerRequiresAuthentication">A value indicating whether the outgoing mail server requires authentication.</param>
        /// <param name="isSendAndReceiveUserNameAndPasswordSame">true if send and receive user name and password are the same.</param>
        /// <param name="sendUserName">The send email user name.</param>
        /// <param name="sendPassword">The send email password.</param>
        /// <param name="keepEmailsOnServerAfterDownload">true, if emails should be kept on the server after download.</param>
        public AccountSettingsData(string accountName, EmailService emailService, EmailServiceProvider emailServiceProvider, string displayName, string emailAddress, string userName, string password, string incomingMailServer, ushort incomingMailServerPort, bool isIncomingMailServerSsl, string outgoingMailServer, ushort outgoingMailServerPort, bool isOutgoingMailServerSsl, bool outgoingMailServerRequiresAuthentication, bool isSendAndReceiveUserNameAndPasswordSame, string sendUserName, string sendPassword, bool keepEmailsOnServerAfterDownload)
            : this(accountName, emailService, emailServiceProvider, displayName, emailAddress, userName, password, incomingMailServer, incomingMailServerPort, isIncomingMailServerSsl, outgoingMailServer, outgoingMailServerPort, isOutgoingMailServerSsl, outgoingMailServerRequiresAuthentication, isSendAndReceiveUserNameAndPasswordSame, sendUserName, sendPassword, true, MailMessage.DefaultSignature, DownloadNewEmailOptions.AsItemsArrive, DownloadEmailsFromOptions.Anytime, keepEmailsOnServerAfterDownload, true)
        {

        }


        /// <summary>
        /// Initialises a new instance of the Skycap.Data.AccountSettingsData class.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="emailServiceProvider">The email service provider.</param>
        /// <param name="displayName">The email display name.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The email password.</param>
        /// <param name="incomingMailServer">The incoming mail server.</param>
        /// <param name="incomingMailServerPort">The incoming mail server port.</param>
        /// <param name="isIncomingMailServerSsl">A value indicating whether the incoming mail server uses ssl.</param>
        /// <param name="outgoingMailServer">The incoming mail server.</param>
        /// <param name="outgoingMailServerPort">The incoming mail server port.</param>
        /// <param name="isOutgoingMailServerSsl">A value indicating whether the incoming mail server uses ssl.</param>
        /// <param name="outgoingMailServerRequiresAuthentication">A value indicating whether the outgoing mail server requires authentication.</param>
        /// <param name="isSendAndReceiveUserNameAndPasswordSame">true if send and receive user name and password are the same.</param>
        /// <param name="sendUserName">The send email user name.</param>
        /// <param name="sendPassword">The send email password.</param>
        /// <param name="useAnEmailSignature">true if an email signature should be used; otherwise, false.</param>
        /// <param name="emailSignature">The email signature.</param>
        /// <param name="downloadNewEmail">The download new email options.</param>
        /// <param name="downloadEmailsFrom">The download emails from.</param>
        /// <param name="keepEmailCopiesOnServer">true, if emails should be kept on the server after download.</param>
        /// <param name="contentToSyncEmail">true, if email content should be synced; otherwise, false.</param>
        public AccountSettingsData(string accountName, EmailService emailService, EmailServiceProvider emailServiceProvider, string displayName, string emailAddress, string userName, string password, string incomingMailServer, ushort incomingMailServerPort, bool isIncomingMailServerSsl, string outgoingMailServer, ushort outgoingMailServerPort, bool isOutgoingMailServerSsl, bool outgoingMailServerRequiresAuthentication, bool isSendAndReceiveUserNameAndPasswordSame, string sendUserName, string sendPassword, bool useAnEmailSignature, string emailSignature, DownloadNewEmailOptions downloadNewEmail, DownloadEmailsFromOptions downloadEmailsFrom, bool keepEmailCopiesOnServer, bool contentToSyncEmail)
            : base()
        { 
            // Initialise local variables
            ImageSource = string.Format("/Assets/{0}.png", emailServiceProvider.ToString());
            AccountName = accountName;
            EmailService = emailService;
            EmailServiceProvider = emailServiceProvider;
            DisplayName = displayName;
            EmailAddress = emailAddress;
            UserName = (string.IsNullOrEmpty(userName) ? emailAddress : userName);
            Password = password;
            IncomingMailServer = incomingMailServer;
            IncomingMailServerPort = incomingMailServerPort;
            IsIncomingMailServerSsl = isIncomingMailServerSsl;
            OutgoingMailServer = outgoingMailServer;
            OutgoingMailServerPort = outgoingMailServerPort;
            IsOutgoingMailServerSsl = isOutgoingMailServerSsl;
            OutgoingMailServerRequiresAuthentication = outgoingMailServerRequiresAuthentication;
            IsSendAndReceiveUserNameAndPasswordSame = isSendAndReceiveUserNameAndPasswordSame;
            if (IsSendAndReceiveUserNameAndPasswordSame)
            {
                SendUserName = UserName;
                SendPassword = Password;
            }
            else
            {
                SendUserName = sendUserName;
                SendPassword = sendPassword;
            }
            UseAnEmailSignature = useAnEmailSignature;
            EmailSignature = emailSignature;
            DownloadNewEmail = downloadNewEmail;
            DownloadEmailsFrom = downloadEmailsFrom;
            KeepEmailCopiesOnServer = keepEmailCopiesOnServer;
            ContentToSyncEmail = contentToSyncEmail;
        }

        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        [DataMember]
        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                if (_imageSource == value)
                    return;

                _imageSource = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ImageSource"));
            }
        }

        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        [DataMember]
        public string AccountName
        {
            get
            {
                return _accountName;
            }
            set
            {
                if (_accountName == value)
                    return;

                _accountName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AccountName"));
            }
        }

        /// <summary>
        /// Gets or sets the email service.
        /// </summary>
        [DataMember]
        public EmailService EmailService
        {
            get
            {
                return _emailService;
            }
            set
            {
                if (_emailService == value)
                    return;

                _emailService = value;
                OnPropertyChanged(new PropertyChangedEventArgs("EmailService"));
            }
        }

        /// <summary>
        /// Gets or sets the email service provider.
        /// </summary>
        [DataMember]
        public EmailServiceProvider EmailServiceProvider
        {
            get
            {
                return _emailServiceProvider;
            }
            set
            {
                if (_emailServiceProvider == value)
                    return;

                _emailServiceProvider = value;
                OnPropertyChanged(new PropertyChangedEventArgs("EmailServiceProvider"));
            }
        }

        /// <summary>
        /// Gets or sets the email display name.
        /// </summary>
        [DataMember]
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                if (_displayName == value)
                    return;

                _displayName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DisplayName"));
            }
        }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        [DataMember]
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
            set
            {
                if (_emailAddress == value)
                    return;

                _emailAddress = value;
                OnPropertyChanged(new PropertyChangedEventArgs("EmailAddress"));
            }
        }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [DataMember]
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (_userName == value)
                    return;

                _userName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("UserName"));
            }
        }

        /// <summary>
        /// Gets or sets the email password.
        /// </summary>
        [DataMember]
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_password == value)
                    return;

                _password = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Password"));
            }
        }

        /// <summary>
        /// Gets or sets the incoming mail server.
        /// </summary>
        [DataMember]
        public string IncomingMailServer
        {
            get
            {
                return _incomingMailServer;
            }
            set
            {
                if (_incomingMailServer == value)
                    return;

                _incomingMailServer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IncomingMailServer"));
            }
        }

        /// <summary>
        /// Gets or sets the incoming mail server port.
        /// </summary>
        [DataMember]
        public ushort IncomingMailServerPort
        {
            get
            {
                return _incomingMailServerPort;
            }
            set
            {
                if (_incomingMailServerPort == value)
                    return;

                _incomingMailServerPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IncomingMailServerPort"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the incoming mail server uses ssl.
        /// </summary>
        [DataMember]
        public bool IsIncomingMailServerSsl
        {
            get
            {
                return _isIncomingMailServerSsl;
            }
            set
            {
                if (_isIncomingMailServerSsl == value)
                    return;

                _isIncomingMailServerSsl = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsIncomingMailServerSsl"));
            }
        }

        /// <summary>
        /// Gets or sets the outgoing mail server.
        /// </summary>
        [DataMember]
        public string OutgoingMailServer
        {
            get
            {
                return _outgoingMailServer;
            }
            set
            {
                if (_outgoingMailServer == value)
                    return;

                _outgoingMailServer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OutgoingMailServer"));
            }
        }

        /// <summary>
        /// Gets or sets the outgoing mail server port.
        /// </summary>
        [DataMember]
        public ushort OutgoingMailServerPort
        {
            get
            {
                return _outgoingMailServerPort;
            }
            set
            {
                if (_outgoingMailServerPort == value)
                    return;

                _outgoingMailServerPort = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OutgoingMailServerPort"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the outgoing mail server uses ssl.
        /// </summary>
        [DataMember]
        public bool IsOutgoingMailServerSsl
        {
            get
            {
                return _isOutgoingMailServerSsl;
            }
            set
            {
                if (_isOutgoingMailServerSsl == value)
                    return;

                _isOutgoingMailServerSsl = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsOutgoingMailServerSsl"));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the outgoing server requires authentication.
        /// </summary>
        public bool OutgoingMailServerRequiresAuthentication
        {
            get
            {
                return _outgoingServerRequiresAuthentication;
            }
            set
            {
                if (_outgoingServerRequiresAuthentication == value)
                    return;

                _outgoingServerRequiresAuthentication = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OutgoingServerRequiresAuthentication"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the send and receive user name and password is the same. 
        /// </summary>
        [DataMember]
        public bool IsSendAndReceiveUserNameAndPasswordSame
        {
            get
            {
                return _isSendAndReceiveUserNameAndPasswordSame;
            }
            set
            {
                if (_isSendAndReceiveUserNameAndPasswordSame == value)
                    return;

                _isSendAndReceiveUserNameAndPasswordSame = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsSendAndReceiveUserNameAndPasswordSame"));
            }
        }

        /// <summary>
        /// Gets or sets the send email user name.
        /// </summary>
        [DataMember]
        public string SendUserName
        {
            get
            {
                return _sendUserName;
            }
            set
            {
                if (_sendUserName == value)
                    return;

                _sendUserName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SendUserName"));
            }
        }

        /// <summary>
        /// Gets or sets the send email password.
        /// </summary>
        [DataMember]
        public string SendPassword
        {
            get
            {
                return _sendPassword;
            }
            set
            {
                if (_sendPassword == value)
                    return;

                _sendPassword = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SendPassword"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an email signature should be used.
        /// </summary>
        [DataMember]
        public bool UseAnEmailSignature
        {
            get
            {
                return _useAnEmailSignature;
            }
            set
            {
                if (_useAnEmailSignature == value)
                    return;

                _useAnEmailSignature = value;
                OnPropertyChanged(new PropertyChangedEventArgs("UseAnEmailSignature"));
            }
        }

        /// <summary>
        /// Gets or sets the email signature.
        /// </summary>
        [DataMember]
        public string EmailSignature
        {
            get
            {
                return _emailSignature;
            }
            set
            {
                if (_emailSignature == value)
                    return;

                _emailSignature = value;
                OnPropertyChanged(new PropertyChangedEventArgs("EmailSignature"));
            }
        }

        /// <summary>
        /// Gets or sets the download new email options.
        /// </summary>
        [DataMember]
        public DownloadNewEmailOptions DownloadNewEmail
        {
            get
            {
                return _downloadNewEmail;
            }
            set
            {
                if (_downloadNewEmail == value)
                    return;

                _downloadNewEmail = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DownloadNewEmail"));
            }
        }

        /// <summary>
        /// Gets or sets the download emails from options.
        /// </summary>
        [DataMember]
        public DownloadEmailsFromOptions DownloadEmailsFrom
        {
            get
            {
                return _downloadEmailsFrom;
            }
            set
            {
                if (_downloadEmailsFrom == value)
                    return;

                _downloadEmailsFrom = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DownloadEmailsFrom"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether emails should be kept on the email server after download.
        /// </summary>
        [DataMember]
        public bool KeepEmailCopiesOnServer
        {
            get
            {
                return _keepEmailCopiesOnServer;
            }
            set
            {
                if (_keepEmailCopiesOnServer == value)
                    return;

                _keepEmailCopiesOnServer = value;
                OnPropertyChanged(new PropertyChangedEventArgs("KeepEmailCopiesOnServer"));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether email content should be synced.
        /// </summary>
        [DataMember]
        public bool ContentToSyncEmail
        {
            get
            {
                return _contentToSyncEmail;
            }
            set
            {
                if (_contentToSyncEmail == value)
                    return;

                _contentToSyncEmail = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ContentToSyncEmail"));
            }
        }

        /// <summary>
        /// Gets or sets the unread message count.
        /// </summary>
        [IgnoreDataMember]
        public int UnreadEmailCount
        {
            get
            {
                return _unreadMessageCount;
            }
            set
            {
                if (_unreadMessageCount != value)
                {
                    _unreadMessageCount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("UnreadEmailCount"));
                    OnPropertyChanged(new PropertyChangedEventArgs("UnreadEmailCountVisibility"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the unread message count visibility.
        /// </summary>
        [IgnoreDataMember]
        public Visibility UnreadEmailCountVisibility
        {
            get
            {
                return (UnreadEmailCount == 0 ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="e">The event data (PropertyChangedEventArgs).</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        /// <summary>
        /// Gets the default account settings data for the specified email service provider and email service.
        /// </summary>
        /// <param name="emailService">The email service.</param>
        /// <param name="accountName">The account name.</param>
        /// <param name="emailAddress">The email service provider.</param>
        /// <param name="password">The email service.</param>
        /// <param name="keepEmailsOnServerAfterDownload">true, if emails should be kept on the email server after download; otherwise, false.</param>
        /// <returns>The default account settings data.</returns>
        public async static Task<AccountSettingsDataList> GetDefaultAccountSettingsData(EmailService emailService, string accountName, string emailAddress, string password, bool keepEmailsOnServerAfterDownload)
        {
            return await GetDefaultAccountSettingsData(EmailServiceProvider.Other, emailService, accountName, emailAddress, password, keepEmailsOnServerAfterDownload);
        }

        /// <summary>
        /// Gets the default account settings data for the specified email service provider and email service.
        /// </summary>
        /// <param name="emailServiceProvider">The email service provider.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="accountName">The account name.</param>
        /// <param name="emailAddress">The email service provider.</param>
        /// <param name="password">The email service.</param>
        /// <param name="keepEmailsOnServerAfterDownload">true, if emails should be kept on the email server after download; otherwise, false.</param>
        /// <returns>The default account settings data.</returns>
        public async static Task<AccountSettingsDataList> GetDefaultAccountSettingsData(EmailServiceProvider emailServiceProvider, EmailService emailService, string accountName, string emailAddress, string password, bool keepEmailsOnServerAfterDownload)
        {
            // Stores the account settings list
            AccountSettingsDataList accountSettingsList = new AccountSettingsDataList();

            // Make sure the email service provider is not known
            if (emailServiceProvider == EmailServiceProvider.Other)
            {
                if (emailAddress.Contains("@gmail."))
                {
                    emailServiceProvider = EmailServiceProvider.Gmail;
                    emailService = EmailService.Imap;
                }
                else if (emailAddress.Contains("@outlook."))
                {
                    emailServiceProvider = EmailServiceProvider.Outlook;
                    emailService = EmailService.Pop;
                }
                else if (emailAddress.Contains("@live.") || emailAddress.Contains("@hotmail."))
                {
                    emailServiceProvider = EmailServiceProvider.Hotmail;
                    emailService = EmailService.Pop;
                }
                else if (emailAddress.Contains("@yahoo."))
                {
                    emailServiceProvider = EmailServiceProvider.Yahoo;
                    emailService = EmailService.Imap;
                }
                else if (emailAddress.Contains("@aol."))
                {
                    emailServiceProvider = EmailServiceProvider.Aol;
                    emailService = EmailService.Imap;
                }
                else if (emailAddress.Contains("@gmx."))
                {
                    emailServiceProvider = EmailServiceProvider.Gmx;
                    emailService = EmailService.Imap;
                }
                else if (emailAddress.Contains("@zoho."))
                {
                    emailServiceProvider = EmailServiceProvider.Zoho;
                    emailService = EmailService.Imap;
                }
            }

            // Get the display name
            string displayName = await GetDisplayName();

            // Determine which email service provider was supplied
            switch (emailServiceProvider)
            {
                // Default Gmail settings
                case EmailServiceProvider.Gmail:
                    accountSettingsList.Add(
                        new AccountSettingsData
                        (
                            accountName,
                            emailService,
                            emailServiceProvider,
                            displayName,
                            emailAddress,
                            emailAddress,
                            password,
                            (emailService == EmailService.Pop ? "pop.gmail.com" : "imap.gmail.com"),
                            (ushort)(emailService == EmailService.Pop ? 995 : 993),
                            "smtp.gmail.com",
                            465,
                            keepEmailsOnServerAfterDownload
                        ));
                    break;

                // Default Hotmail settings
                case EmailServiceProvider.Outlook:
                case EmailServiceProvider.Hotmail:
                    if (emailService != EmailService.Pop)
                        throw new Exception(string.Format("{0} is not supported on {1}", emailService, emailServiceProvider));
                    else
                        accountSettingsList.Add(
                            new AccountSettingsData
                            (
                                accountName,
                                emailService,
                                emailServiceProvider,
                                displayName,
                                emailAddress,
                                emailAddress,
                                password,
                                "pop3.live.com",
                                995,
                                "smtp.live.com",
                                587,
                                keepEmailsOnServerAfterDownload
                            ));
                    break;

                // Default Yahoo settings
                case EmailServiceProvider.Yahoo:
                    if (emailService != EmailService.Pop)
                        throw new Exception(string.Format("{0} is not supported on {1}", emailService, emailServiceProvider));
                    else
                        accountSettingsList.Add(
                        new AccountSettingsData
                        (
                            accountName,
                            emailService,
                            emailServiceProvider,
                            displayName,
                            emailAddress,
                            emailAddress,
                            password,
                            (emailService == EmailService.Pop ? "pop.mail.yahoo.com" : "imap.mail.yahoo.com"),
                            (ushort)(emailService == EmailService.Pop ? 995 : 993),
                            "smtp.yahoo.com",
                            465,
                            keepEmailsOnServerAfterDownload
                        ));
                    break;

                // Default Aol settings
                case EmailServiceProvider.Aol:
                    accountSettingsList.Add(
                        new AccountSettingsData
                        (
                            accountName,
                            emailService,
                            emailServiceProvider,
                            displayName,
                            emailAddress,
                            emailAddress.Split('@')[0],
                            password,
                            (emailService == EmailService.Pop ? "pop.aol.com" : "imap.aol.com"),
                            (ushort)(emailService == EmailService.Pop ? 995 : 993),
                            "smtp.aol.com",
                            587,
                            keepEmailsOnServerAfterDownload
                        ));
                    break;

                // Default Gmx settings
                case EmailServiceProvider.Gmx:
                    accountSettingsList.Add(
                        new AccountSettingsData
                        (
                            accountName,
                            emailService,
                            emailServiceProvider,
                            displayName,
                            emailAddress,
                            emailAddress,
                            password,
                            (emailService == EmailService.Pop ? "pop.gmx.net" : "imap.gmx.net"),
                            (ushort)(emailService == EmailService.Pop ? 995 : 993),
                            "mail.gmx.net",
                            465,
                            keepEmailsOnServerAfterDownload
                        ));
                    break;

                // Default Zoho settings
                case EmailServiceProvider.Zoho:
                    accountSettingsList.Add(
                        new AccountSettingsData
                        (
                            accountName,
                            emailService,
                            emailServiceProvider,
                            displayName,
                            emailAddress,
                            emailAddress,
                            password,
                            (emailService == EmailService.Pop ? "pop.zoho.com" : "imap.zoho.com"),
                            (ushort)(emailService == EmailService.Pop ? 995 : 993),
                            "smtp.zoho.com",
                            465,
                            keepEmailsOnServerAfterDownload
                        ));
                    break;

                // Default custom settings
                default:
                    string domainName = emailAddress.Split('@')[1];
                    bool[] trueFalseFlags = { false, true };

                    // Loop through each well known incoming email server protocol
                    foreach(string wellKnownIncomingEmailServer in Enum.GetNames(typeof(WellKnownIncomingEmailServer)).Where(o => o.Contains(emailService.ToString())
                                                                                                                               || o.Contains(WellKnownIncomingEmailServer.Mail.ToString())))
                    {
                        // Loop through each security
                        foreach (bool isIncomingMailServerSsl in trueFalseFlags)
                        {
                            // Get mail server ports for the current email protocol and security
                            IEnumerable<uint> incomingMailServerPorts = GetWellKnownIncomingEmailServerPorts((WellKnownIncomingEmailServer)Enum.Parse(typeof(WellKnownIncomingEmailServer), wellKnownIncomingEmailServer), isIncomingMailServerSsl, emailService);

                            // Loop through each incoming mail server port
                            foreach (ushort incomingMailServerPort in incomingMailServerPorts)
                            {
                                // Loop through each well known incoming email server protocol
                                foreach (string wellKnownOutgoingEmailServer in Enum.GetNames(typeof(WellKnownOutgoingEmailServer)).Union(new string[] { wellKnownIncomingEmailServer }))
                                { 
                                    // Loop through each security
                                    foreach (bool isOutgoingMailServerSsl in trueFalseFlags)
                                    {
                                        // Get mail server ports for the current email protocol and security
                                        IEnumerable<uint> outgoingMailServerPorts = GetWellKnownOutgoingEmailServerPorts(isOutgoingMailServerSsl);
                                    
                                        // Loop through each outgoing mail server port
                                        foreach (ushort outgoingMailServerPort in outgoingMailServerPorts)
                                        {
                                            // Loop through each user name
                                            foreach (string userName in new string[] { emailAddress, emailAddress.Split('@')[0] })
                                            {
                                                accountSettingsList.Add(
                                                    new AccountSettingsData
                                                    (
                                                        accountName.ToWords(),
                                                        emailService,
                                                        emailServiceProvider,
                                                        displayName,
                                                        emailAddress,
                                                        userName,
                                                        password,
                                                        string.Format("{0}.{1}", wellKnownIncomingEmailServer.ToLower(), domainName),
                                                        incomingMailServerPort,
                                                        isIncomingMailServerSsl,
                                                        string.Format("{0}.{1}", wellKnownOutgoingEmailServer.ToLower(), domainName),
                                                        outgoingMailServerPort,
                                                        isOutgoingMailServerSsl,
                                                        true,
                                                        true,
                                                        userName,
                                                        password,
                                                        true,
                                                        MailMessage.DefaultSignature,
                                                        DownloadNewEmailOptions.AsItemsArrive,
                                                        DownloadEmailsFromOptions.Anytime,
                                                        keepEmailsOnServerAfterDownload,
                                                        true
                                                    ));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return accountSettingsList;
        }

        /// <summary>
        /// Gets the well known incoming email server ports.
        /// </summary>
        /// <param name="emailProtocol">The email protocol.</param>
        /// <param name="isSsl">true if ssl; otherwise false.</param>
        /// <param name="emailService">The email service.</param>
        /// <returns>The incoming port numbers.</returns>
        public static uint[] GetWellKnownIncomingEmailServerPorts(WellKnownIncomingEmailServer emailProtocol, bool isSsl, EmailService emailService)
        {
            emailProtocolChanged:
            switch (emailProtocol)
            {
                case WellKnownIncomingEmailServer.Imap:
                case WellKnownIncomingEmailServer.Imap4:
                    if (isSsl)
                        return new uint[] { 993, 585 };
                    else
                        return new uint[] { 143 };

                case WellKnownIncomingEmailServer.Pop:
                case WellKnownIncomingEmailServer.Pop3:
                    if (isSsl)
                        return new uint[] { 995, 443 };
                    else
                        return new uint[] { 110 };

                default:
                    if (emailService == EmailService.Imap)
                        emailProtocol = WellKnownIncomingEmailServer.Imap;
                    else
                        emailProtocol = WellKnownIncomingEmailServer.Pop;
                    goto emailProtocolChanged;
            }
        }

        /// <summary>
        /// Gets the well known outgoing email server ports.
        /// </summary>
        /// <param name="isSsl">true if ssl; otherwise false.</param>
        /// <returns>The incoming port numbers.</returns>
        public static uint[] GetWellKnownOutgoingEmailServerPorts(bool isSsl)
        {
            if (isSsl)
                return new uint[] { 25, 465, 587 };
            else
                return new uint[] { 25, 587 };
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <returns>The display name.</returns>
        public async static Task<string> GetDisplayName()
        {
            // Gets the display name
            return await UserInformation.GetDisplayNameAsync();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(AccountSettingsData other)
        {
            return (other.EmailAddress.Equals(EmailAddress, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the has code for this account settings data.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.EmailAddress.ToLower().GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the account settings data, except sensitive password information.
        /// </summary>
        /// <returns>A string that represents the account settings data, except sensitive password information.</returns>
        public override string ToString()
        {
            // Build email to send ourselves info that user is trying to connect with
            // NOTE: No password info is being sent - we only interested in the mail server info
            StringBuilder accountSettingsDataStringBuilder = new StringBuilder();
            accountSettingsDataStringBuilder.Append("<table border=\"1\" style=\"border-color: WhiteSmoke; font-family: Trebuchet MS, Verdana; Arial;\">");
            accountSettingsDataStringBuilder.Append("<tr><th colspan=\"2\">Account Settings Data</th></tr>");
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">EmailService:</th><td>{0}</td></tr>", EmailService);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">EmailServiceProvider:</th><td>{0}</td></tr>", EmailServiceProvider);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">DisplayName:</th><td>{0}</td></tr>", DisplayName);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">EmailAddress:</th><td>{0}</td></tr>", EmailAddress);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">UserName:</th><td>{0}</td></tr>", UserName);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">IncomingMailServer:</th><td>{0}</td></tr>", IncomingMailServer);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">IncomingMailServerPort:</th><td>{0}</td></tr>", IncomingMailServerPort);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">IsIncomingMailServerSsl:</th><td>{0}</td></tr>", IsIncomingMailServerSsl);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">OutgoingMailServer:</th><td>{0}</td></tr>", OutgoingMailServer);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">OutgoingMailServerPort:</th><td>{0}</td></tr>", OutgoingMailServerPort);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">IsOutgoingMailServerSsl:</th><td>{0}</td></tr>", IsOutgoingMailServerSsl);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">OutgoingMailServerRequiresAuthentication:</th><td>{0}</td></tr>", OutgoingMailServerRequiresAuthentication);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">IsSendAndReceiveUserNameAndPasswordSame:</th><td>{0}</td></tr>", IsSendAndReceiveUserNameAndPasswordSame);
            accountSettingsDataStringBuilder.AppendFormat("<tr><th style=\"text-align: left;\">SendUserName:</th><td>{0}</td></tr>", SendUserName);
            accountSettingsDataStringBuilder.Append("</table>");
            return accountSettingsDataStringBuilder.ToString();
        }
    }
}
