using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Skycap.Controls;
using Skycap.Data;
using Skycap.IO;
using Skycap.Net;
using Skycap.Net.Common;
using Skycap.Net.Imap;
using Skycap.Notifications;
using Skycap.Pages;
using Skycap.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Search;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Skycap
{
    /// <summary>
    /// Represents the various enumerations for the notify type.
    /// </summary>
    public enum NotifyType
    { 
        /// <summary>
        /// Indicates a status message.
        /// </summary>
        StatusMessage,
        /// <summary>
        /// Indicates an error message.
        /// </summary>
        ErrorMessage,
    }

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : LayoutAwarePage
    {
        #region Private Variables

        /// <summary>
        /// The core dispatcher.
        /// </summary>
        private CoreDispatcher _dispatcher = Window.Current.Dispatcher;
        /// <summary>
        /// The license information.
        /// </summary>
        private LicenseInformation _licenseInformation;
        /// <summary>
        /// The trial timer.
        /// </summary>
        private DispatcherTimer _trialTimer;
        /// <summary>
        /// The temp mail headers for each account.
        /// </summary>
        private MailHeaderDictionary _tempMailHeaderDictionary;
        /// <summary>
        /// The last mailbox action.
        /// </summary>
        private bool _isLastMailboxActionAdd;
        /// <summary>
        /// A value indicating whether the email flag filter was applied.
        /// </summary>
        private bool _isEmailFlagFiltered;
        /// <summary>
        /// A value indicating whether the control key is pressed.
        /// </summary>
        private bool _isCtrlKeyDown;
        /// <summary>
        /// A value indicating whether the shift key is pressed.
        /// </summary>
        private bool _isShiftKeyDown;
        /// <summary>
        /// A value indicating whether the email dialog is open.
        /// </summary>
        private bool _isDialogOpen;
        /// <summary>
        /// A value indicating whether the move to folder button was clicked.
        /// </summary>
        private bool _moveToFolder;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of the Skycap.MainPage class.
        /// </summary>
        public MainPage()
        {
            // Add FontSize
            if (!App.Current.Resources.ContainsKey("FontSize"))
                App.Current.Resources.Add("FontSize", 16);

            // Initialise
            this.InitializeComponent();

            // Register 
            SyncMailBackgroundTask.Register();

            // Subscribe to CurrentStateChanged event
            ApplicationViewStates.CurrentStateChanged += ApplicationViewStates_CurrentStateChanged;

            // Subscribe to CommandsRequested event
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;

            // Subscribe to SuggestionsRequested event
            SearchPane.GetForCurrentView().SuggestionsRequested += MainPage_SuggestionsRequested;
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;

            // Subscribe to KeyDown and KeyUp
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

            // Initialise local properties
            Current = this;
            NavigationCacheMode = NavigationCacheMode.Required;
            MailClientsDictionary = new ConcurrentDictionary<string, MailClient>();

            // Initialise local variables
            cbEmailFlags.ItemsSource = Enum.GetNames(typeof(EmailFlags)).Select(o => new { Name = o.ToWords(), Value = o });
            cbEmailFlags.DisplayMemberPath = "Name";
            cbEmailFlags.SelectedValuePath = "Value";
            cbEmailFlags.SelectedIndex = 0;
            _tempMailHeaderDictionary = new MailHeaderDictionary();
#if DEBUG
            _licenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            licenseInformation = CurrentApp.LicenseInformation;
            //licenseInformation = CurrentAppSimulator.LicenseInformation;
#endif
            _licenseInformation.LicenseChanged += licenseInformation_LicenseChanged;
            _trialTimer = new DispatcherTimer();
            _trialTimer.Tick += trialTimer_Tick;
            _trialTimer.Interval = new TimeSpan(0, 1, 0);
            _trialTimer.Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the main page.
        /// </summary>
        internal static MainPage Current
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating if the application is running for the first time.
        /// </summary>
        internal bool IsRunningFirstTime
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("IsRunningFirstTime"))
                {
                    ApplicationData.Current.LocalSettings.Values["IsRunningFirstTime"] = false;
                    return true;
                }
                return (bool)ApplicationData.Current.LocalSettings.Values["IsRunningFirstTime"];
            }
        }

        /// <summary>
        /// Gets a value indicating whether a dialog is open on the main page.
        /// </summary>
        internal bool IsDialogOpen
        {
            get
            {
                return _isDialogOpen;
            }
            set
            {
                _isDialogOpen = value;
                if (value) BottomAppBar.IsOpen = false;
                emailBodyText.Visibility = (value ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        /// <summary>
        /// Gets the mail clients dictionary.
        /// </summary>
        internal ConcurrentDictionary<string, MailClient> MailClientsDictionary
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the selected account.
        /// </summary>
        internal KeyValuePair<string, AccountSettingsData> SelectedAccount
        {
            get
            {
                if (lvMailbox.SelectedItem != null && lvMailbox.SelectedItem is KeyValuePair<string, AccountSettingsData>)
                    return (KeyValuePair<string, AccountSettingsData>)lvMailbox.SelectedItem;
                return new KeyValuePair<string, AccountSettingsData>();
            }
            set
            {
                lvMailbox.SelectedItem = value;
            }
        }

        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        internal string AccountName
        {
            get
            {
                return tbAccountName.Text;
            }
            set
            {
                tbAccountName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the mailbox tree data context.
        /// </summary>
        internal KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>? MailboxTreeDataContext
        {
            get
            {
                try
                {
                    if (gMailboxTree.DataContext is KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>)
                        return (KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>)gMailboxTree.DataContext;
                    return null;
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError("", "", ex.ToString());
                    return null;
                }
            }
            set
            {
                gMailboxTree.DataContext = value;
            }
        }

        /// <summary>
        /// Gets the selected mail client.
        /// </summary>
        internal MailClient SelectedMailClient
        {
            get
            {
                MailClient mailClient = null;
                MailClientsDictionary.TryGetValue(tbEmailAddress.Text, out mailClient);
                return mailClient;
            }
        }

        /// <summary>
        /// Gets the selected mailbox list view item.
        /// </summary>
        internal MailboxListViewItem SelectedMailboxListViewItem
        {
            get
            { 
                MailboxListViewItem mailboxListViewItem = (MailboxListViewItem)lvMailboxTree.SelectedItem;
                return (mailboxListViewItem == null ? null : mailboxListViewItem);
            }
            set
            {
                lvMailboxTree.SelectedItem = value;
            }
        }

        /// <summary>
        /// Gets the selected mailbox.
        /// </summary>
        internal Mailbox SelectedMailbox
        {
            get
            {
                return (SelectedMailboxListViewItem == null ? null : SelectedMailboxListViewItem.Mailbox);
            }
        }

        /// <summary>
        /// Gets or sets the selected mail header.
        /// </summary>
        internal MailHeader SelectedMailHeader
        {
            get
            {
                return (MailHeader)lvMailHeaders.SelectedItem;
            }
            set
            {
                lvMailHeaders.SelectedItem = value;
            }
        }

        /// <summary>
        /// Gets the selected message.
        /// </summary>
        internal StructuredMessage SelectedMessage
        {
            get
            {
                return emailBodyLayout.DataContext as StructuredMessage;
            }
            set
            {
                emailBodyLayout.DataContext = value;
            }
        }

        /// <summary>
        /// Gets the query text.
        /// </summary>
        internal string QueryText
        {
            get
            {
                return tbQueryText.Text;
            }
        }

        /// <summary>
        /// Gets the message navigation context.
        /// </summary>
        internal MessageNavigationContext MessageNavigationContext
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Occurs after a control changes into a different state.
        /// </summary>
        /// <param name="sender">The object that raised the event (ApplicationViewStates).</param>
        /// <param name="e">The event data (VisualStateChangedEventArgs).</param>
        private void ApplicationViewStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            // Determine the current view state
            switch ((ApplicationViewState)Enum.Parse(typeof(ApplicationViewState), e.NewState.Name))
            {
                // If Snapped
                case ApplicationViewState.Snapped:
                    gContentLayout.ColumnDefinitions[0].Width = new GridLength(0);
                    gContentLayout.ColumnDefinitions[1].Width = new GridLength(372);
                    gContentLayout.ColumnDefinitions[2].Width = new GridLength(0);
                    gFilter.Visibility = Visibility.Collapsed;
                    break;

                // If Filled
                case ApplicationViewState.Filled:
                    gContentLayout.ColumnDefinitions[0].Width = new GridLength(0);
                    gContentLayout.ColumnDefinitions[1].Width = new GridLength(372);
                    gContentLayout.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    gFilter.Visibility = Visibility.Collapsed;
                    break;

                // If FullScreenPortrait
                case ApplicationViewState.FullScreenPortrait:
                    gContentLayout.ColumnDefinitions[0].Width = new GridLength(0);
                    gContentLayout.ColumnDefinitions[1].Width = new GridLength(0);
                    gContentLayout.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    gFilter.Visibility = Visibility.Collapsed;
                    break;

                // Anything else
                default:
                    gContentLayout.ColumnDefinitions[0].Width = new GridLength(186);
                    gContentLayout.ColumnDefinitions[1].Width = new GridLength(372);
                    gContentLayout.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                    gFilter.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Occurs when the trial timer interval elapses.
        /// </summary>
        /// <param name="sender">The object that raised the event (CoreWindow).</param>
        /// <param name="e">The event data (KeyEventArgs).</param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            // If Escape
            if (e.VirtualKey == VirtualKey.Escape)
            {
                pMoveToFolder.IsOpen = false;
                App.UnfillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
            }
            // Else if Control key
            else if (e.VirtualKey == VirtualKey.Control)
                _isCtrlKeyDown = true;
            // Else if Shift key
            else if (e.VirtualKey == VirtualKey.Shift)
                _isShiftKeyDown = true;
            // Else if Control key was already pressed
            else if (_isCtrlKeyDown)
            {
                // Determine what combination Ctrl+Key was pressed
                switch (e.VirtualKey)
                {
                    // If Edit
                    case VirtualKey.E:
                        if (bEdit.Visibility == Visibility.Visible)
                            bEdit_Click(this, new RoutedEventArgs());
                        break;

                    // If New
                    case VirtualKey.N:
                        if (bNew.Visibility == Visibility.Visible)
                            bNew_Click(this, new RoutedEventArgs());
                        break;

                    // If Reply
                    case VirtualKey.R:
                        if (bReply.Visibility == Visibility.Visible)
                            bReply_Click(this, new RoutedEventArgs());
                        break;

                    // If Reply To All
                    case VirtualKey.T:
                        if (bReplyAll.Visibility == Visibility.Visible)
                            bReplyAll_Click(this, new RoutedEventArgs());
                        break;

                    // If Forward
                    case VirtualKey.F:
                        if (bForward.Visibility == Visibility.Visible)
                            bForward_Click(this, new RoutedEventArgs());
                        break;

                    // If Delete
                    case VirtualKey.Delete:
                    case VirtualKey.D:
                        if (bDelete.Visibility == Visibility.Visible)
                            bDelete_Click(this, new RoutedEventArgs());
                        break;
                }
            }
            // Else if Shift key was already pressed
            else if (_isShiftKeyDown)
            {
                // Determine what combination Shift+Key was pressed
                switch (e.VirtualKey)
                {
                    // If Delete
                    case VirtualKey.Delete:
                    case VirtualKey.D:
                        if (bDelete.Visibility == Visibility.Visible)
                            bDelete_Click(sender, new RoutedEventArgs());
                        break;
                }
            }
        }

        /// <summary>
        /// Occurs when the trial timer interval elapses.
        /// </summary>
        /// <param name="sender">The object that raised the event (CoreWindow).</param>
        /// <param name="e">The event data (KeyEventArgs).</param>
        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs e)
        {
            // If Control key was released
            if (e.VirtualKey == VirtualKey.Control)
                _isCtrlKeyDown = false;
            // If Shift key was released
            else if (e.VirtualKey == VirtualKey.Shift)
                _isShiftKeyDown = false;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            try
            {
#if DEBUG
                if (_licenseInformation.IsTrial)
                {
                    StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Data");
                    StorageFile proxyFile = await proxyDataFolder.GetFileAsync("WindowsStoreProxy.xml");
                    await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
                }
#endif

                // If page state is available
                if (SuspensionManager.SessionState != null)
                {
                    // If EmailFlags
                    if (SuspensionManager.SessionState.ContainsKey("EmailFlags"))
                        cbEmailFlags.SelectedValue = SuspensionManager.SessionState["EmailFlags"];

                    // If QueryText
                    if (SuspensionManager.SessionState.ContainsKey("QueryText"))
                    {
                        tbQueryText.Text = (string)SuspensionManager.SessionState["QueryText"];
                        ResetEmailHeaderFilterState(!string.IsNullOrEmpty(tbQueryText.Text), tbQueryText.Text);
                    }

                    // If MailboxTreeDataContext
                    if (SuspensionManager.SessionState.ContainsKey("MailboxTreeDataContext"))
                    {
                        MailboxTreeDataContext = SuspensionManager.SessionState["MailboxTreeDataContext"] as KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>?;
                        if (MailboxTreeDataContext == null)
                            App.IsSessionStateCorrupted = true;
                    }

                    // If no MailboxTreeDataContext
                    if (MailboxTreeDataContext != null)
                    {
                        // If MailboxTreeSelectedItem
                        if (SuspensionManager.SessionState.ContainsKey("MailboxTreeSelectedItem"))
                            EnsureItemIsSelected(lvMailboxTree, new object[] { SuspensionManager.SessionState["MailboxTreeSelectedItem"] ?? MailboxTreeDataContext.Value.Value.FirstOrDefault() });

                        // If MailHeadersItemsSource
                        BindMailHeader(StorageSettings.AccountSettingsDataDictionary[MailboxTreeDataContext.Value.Key.EmailAddress], ((MailboxListViewItem)SuspensionManager.SessionState["MailboxTreeSelectedItem"]).Mailbox, tbQueryText.Text);

                        // If MailHeadersSelectedItems
                        if (SuspensionManager.SessionState.ContainsKey("MailHeadersSelectedItems"))
                        {
                            IList<object> mailHeaders = (IList<object>)SuspensionManager.SessionState["MailHeadersSelectedItems"] ?? new List<object>() { StorageSettings.MailHeaderDictionary[MailboxTreeDataContext.Value.Key.EmailAddress][SelectedMailbox.FullName].FirstOrDefault() };
                            if (mailHeaders.Count == 1)
                                lvMailHeaders.SelectedItem = mailHeaders[0];
                            else if (mailHeaders.Count > 1)
                            {
                                foreach (object selectedItem in mailHeaders)
                                    lvMailHeaders.SelectedItems.Add(selectedItem);
                            }
                            lvMailHeaders.ScrollIntoView(mailHeaders.LastOrDefault());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
            finally
            {
                // Loads and connects the mailboxes
                LoadAndConnectMailboxes();
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            try
            {
                // Save all the page state data
                SuspensionManager.SessionState["EmailFlags"] = cbEmailFlags.SelectedValue;
                SuspensionManager.SessionState["QueryText"] = tbQueryText.Text;
                SuspensionManager.SessionState["MailboxTreeDataContext"] = gMailboxTree.DataContext;
                SuspensionManager.SessionState["MailboxTreeSelectedItem"] = lvMailboxTree.SelectedItem;
                SuspensionManager.SessionState["MailHeadersSelectedItems"] = (IList<object>)new List<object>(lvMailHeaders.SelectedItems.ToArray());
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the status of the app's license changes.
        /// </summary>
        private async void licenseInformation_LicenseChanged()
        {
            try
            {
                // If Active Trial app
                if (_licenseInformation.IsTrial
                || !_licenseInformation.IsActive)
                {
                    // Notify user of trial
                    tbTrialStatus.Visibility = Visibility.Visible;
                    TimeSpan timeLeft = (_licenseInformation.ExpirationDate - DateTime.Now);

                    // If license is expired
                    if (timeLeft.Days <= 0
                     && timeLeft.Hours <= 0
                     && timeLeft.Minutes <= 0)
                    {
                        App.NotifyUser(tbTrialStatus, "Your license is expired.", NotifyType.ErrorMessage);

                        MessageDialog trialExpiredMessageDialog = new MessageDialog("Your trial license for this application has expired.\r\nIf you do not purchase a full license, the application will exit.\r\nWould you like to buy the full version?", "Trial License Expired");
                        trialExpiredMessageDialog.Commands.Add(new UICommand("OK"));
                        trialExpiredMessageDialog.Commands.Add(new UICommand("Cancel"));
                        trialExpiredMessageDialog.DefaultCommandIndex = 0;
                        trialExpiredMessageDialog.CancelCommandIndex = 1;
                        _trialTimer.Stop();

                        try
                        {
                            // If Buy
                            if (await trialExpiredMessageDialog.ShowAsync() == trialExpiredMessageDialog.Commands[0])
                            {
                                string purchaseXml;
#if DEBUG
                                purchaseXml = await CurrentAppSimulator.RequestAppPurchaseAsync(true);
#else
                                purchaseXml = await CurrentApp.RequestAppPurchaseAsync(true);
#endif
                                if (purchaseXml.IndexOf("LicenseType=\"Full\"", StringComparison.OrdinalIgnoreCase) == -1)
                                {
                                    try
                                    {
                                        await SuspensionManager.SaveAsync();
                                        UnloadAndDisconnectMailboxes();
                                        Application.Current.Exit();
                                    }
                                    catch { }
                                }
                            }
                            // Else if Cancel exit
                            else
                            {
                                try
                                {
                                    await SuspensionManager.SaveAsync();
                                    UnloadAndDisconnectMailboxes();
                                    Application.Current.Exit();
                                }
                                catch { }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogFile.Instance.LogError("", "", ex.ToString());
                        }
                    }
                    else
                    {
                        // Get the message time left
                        string messageTimeLeft = (timeLeft.Days > 0 ? string.Format("{0} day", timeLeft.Days) : string.Empty);
                        messageTimeLeft += (timeLeft.Days > 1 ? "s, " : " ");
                        messageTimeLeft += (timeLeft.Hours > 0 ? string.Format("{0} hour", timeLeft.Hours) : string.Empty);
                        messageTimeLeft += (timeLeft.Hours > 1 ? "s and " : string.Empty);
                        messageTimeLeft += (timeLeft.Minutes > 0 ? string.Format("{0} minute", timeLeft.Minutes) : string.Empty);
                        messageTimeLeft += (timeLeft.Minutes > 1 ? "s" : string.Empty);
                        App.NotifyUser(tbTrialStatus, string.Format("You can use this app for {0} before the trial period ends.", messageTimeLeft), NotifyType.ErrorMessage);
                    }
                }
                // Else if NOT trial
                else
                {
                    // Hide trial status
                    tbTrialStatus.Visibility = Visibility.Collapsed;
                    _trialTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the trial timer interval elapses.
        /// </summary>
        /// <param name="sender">The object that raised the event (DispatchTimer).</param>
        /// <param name="e">The event data (Object).</param>
        private void trialTimer_Tick(object sender, object e)
        {
            licenseInformation_LicenseChanged();
        }

        /// <summary>
        /// This the event handler for the "Defaults" button added to the settings charm. This method
        /// is responsible for creating the Popup window will use as the container for our settings Flyout.
        /// The reason we use a Popup is that it gives us the "light dismiss" behavior that when a user clicks away 
        /// from our custom UI it just dismisses.  This is a principle in the Settings experience and you see the
        /// same behavior in other experiences like AppBar. 
        /// </summary>
        /// <param name="command"></param>
        private void OnSettingsCommand(IUICommand command)
        {
            try
            {
                // Determine which settings menu to flyout
                switch (command.Id.ToString())
                {
                    // If Account Settings
                    case "listAccountsSettingsControl":
                        if (StorageSettings.AccountSettingsDataDictionary.Count == 0)
                        {
                            // Reset the application
                            ResetApplication();
                            lascListAccountsSettingsControl.ShowAddAccount();
                        }
                        else
                            lascListAccountsSettingsControl.ShowListAccount();
                        break;

                    // If Privacy Settings
                    case "privacySettingsControl":
                        pscPrivacySettingsControl.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a FrameworkElement has been constructed and added to the object tree.
        /// </summary>
        /// <param name="sender">The object that raised the event (MainPage).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Store the selected item
                object selectedItem = lvMailHeaders.SelectedItem;
                // Reset the bind data
                lvMailHeaders.ItemsSource = null;
                // Make sure an account is selected
                if (SelectedAccount.Value == null && lvMailbox.Items.Count > 0)
                    lvMailbox.SelectedItem = lvMailbox.Items[0];
                // If an account and mailbox is selected
                if (SelectedAccount.Value != null && SelectedMailbox != null)
                {
                    // Bind mail header
                    BindMailHeader(SelectedAccount.Value, SelectedMailbox, QueryText);
                    // Ensure item is selected
                    lvMailHeaders.SelectedItem = selectedItem;
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the main page receives focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (MainPage).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MainPage_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // If an account hasn't been loaded
                if (StorageSettings.AccountSettingsDataDictionary.Count == 0)
                {
                    ResetApplication();
                    ShowHideFilters(Visibility.Collapsed);
                    lascListAccountsSettingsControl.ShowAddAccount();
                }
                else
                    ShowHideFilters(Visibility.Visible);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// This event is generated when the user opens the settings pane. During this event, append your
        /// SettingsCommand objects to the available ApplicationCommands vector to make them available to the
        /// SettingsPange UI.
        /// </summary>
        /// <param name="settingsPane">Instance that triggered the event.</param>
        /// <param name="eventArgs">Event data describing the conditions that led to the event.</param>
        private void MainPage_CommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            try
            {
                UICommandInvokedHandler handler = new UICommandInvokedHandler(OnSettingsCommand);

                SettingsCommand accountsCommand = new SettingsCommand("listAccountsSettingsControl", "Accounts", handler);
                eventArgs.Request.ApplicationCommands.Add(accountsCommand);

                //SettingsCommand helpCommand = new SettingsCommand("helpSettingsControl", "Help", handler);
                //eventArgs.Request.ApplicationCommands.Add(helpCommand);

                //SettingsCommand aboutCommand = new SettingsCommand("aboutSettingsControl", "About", handler);
                //eventArgs.Request.ApplicationCommands.Add(aboutCommand);

                //SettingsCommand feedbackCommand = new SettingsCommand("feedbackSettingsControl", "Feedback", handler);
                //eventArgs.Request.ApplicationCommands.Add(feedbackCommand);

                SettingsCommand privacyCommand = new SettingsCommand("privacySettingsControl", "Privacy", handler);
                eventArgs.Request.ApplicationCommands.Add(privacyCommand);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Fires when the user's query text changes and the app needs to provide new suggestions to display in the search pane.
        /// </summary>
        /// <param name="sender">The object that raised the event (SearchPane).</param>
        /// <param name="args">The event data (SearchPaneSuggestionsRequestedEventArgs).</param>
        private void MainPage_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            try
            {
                // Make sure a query text was supplied
                if (!string.IsNullOrWhiteSpace(args.QueryText)
                 && args.QueryText.Length >= 3)
                {
                    // Get the selected data
                    MailClient mailClient = SelectedMailClient;
                    Mailbox mailbox = SelectedMailbox;

                    // Make sure the require data is available
                    if (mailClient != null
                     && mailbox != null)
                    {
                        // Loop through each mail header
                        StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][mailbox.FullName].Filter = GetQueryFilter(args.QueryText, true);
                        foreach (MailHeader mailHeader in StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][mailbox.FullName])
                        {
                            // Add suggestion to Search Pane
                            args.Request.SearchSuggestionCollection.AppendQuerySuggestion(mailHeader.Subject);

                            // Break since the Search Pane can show at most 5 suggestions
                            if (args.Request.SearchSuggestionCollection.Size >= 5)
                                return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Loads and connects the mailboxes.
        /// </summary>
        internal void LoadAndConnectMailboxes()
        {
            try
            {
                // Bind the mailboxes
                AccountSettingsData defaultAccountSettingsData = (MailboxTreeDataContext == null ? StorageSettings.AccountSettingsDataDictionary.Values.FirstOrDefault() : MailboxTreeDataContext.Value.Key);
                BindMailboxes(defaultAccountSettingsData);

                // Loop though each account
                foreach (AccountSettingsData accountSettingsData in StorageSettings.AccountSettingsDataDictionary.Values)
                {
                    // Connect mailbox
                    ConnectMailbox(accountSettingsData, null);
                }

                // Update the badge
                EmailBadgeNotification.UpdateBadge(StorageSettings.MailHeaderDictionary.AllUnreadEmailCount);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Unloads and disconnects the mailboxes.
        /// </summary>
        internal void UnloadAndDisconnectMailboxes()
        {
            try
            {
                Task.Run(async() =>
                {
                    // Loop through each mail client
                    foreach (MailClient mailClient in MailClientsDictionary.Values)
                    {
                        // Disconnect the mail client
                        if (mailClient.State.HasFlag(MailClientState.Authenticated))
                            await mailClient.Logout();
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Connects a mailbox for the specified account settings data.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailClient">The mail client.</param>
        internal void ConnectMailbox(AccountSettingsData accountSettingsData, MailClient mailClient)
        {
            Task.Run(async() =>
            {
                try
                {
                    // Get the mail client
                    if (mailClient == null)
                        mailClient = await MailClient.GetMailClient(accountSettingsData);

                    // Store the mail client
                    MailClientsDictionary.TryAdd(mailClient.AccountSettingsData.EmailAddress, mailClient);

                    // Update account unread email count
                    if (StorageSettings.MailHeaderDictionary.ContainsKey(accountSettingsData.EmailAddress))
                        UpdateAccountUnreadEmailCount(accountSettingsData.EmailAddress, StorageSettings.MailHeaderDictionary.GetAccountUnreadEmailCount(accountSettingsData.EmailAddress));

                    // Unsubscribe to all the events
                    mailClient.Connected -= mailClient_Connected;
                    mailClient.LoggingIn -= mailClient_LoggingIn;
                    mailClient.LoggedIn -= mailClient_LoggedIn;
                    mailClient.LoginFailed -= mailClient_LoginFailed;
                    mailClient.UpdatingMailboxTree -= mailClient_UpdatingMailboxTree;
                    mailClient.UpdatedMailboxTree -= mailClient_UpdatedMailboxTree;
                    mailClient.UpdateMailboxTreeFailed -= mailClient_UpdateMailboxTreeFailed;
                    mailClient.RestoreStarted -= mailClient_RestoreStarted;
                    mailClient.RestoringMessage -= mailClient_RestoringMessage;
                    mailClient.RestoredMessage -= mailClient_RestoredMessage;
                    mailClient.RestoreMessageFailed -= mailClient_RestoreMessageFailed;
                    mailClient.RestoreCompleted -= mailClient_RestoreCompleted;
                    mailClient.DownloadStarted -= mailClient_DownloadStarted;
                    //mailClient.DownloadingMessage -= mailClient_DownloadingMessage;
                    mailClient.DownloadedMessage -= mailClient_DownloadedMessage;
                    //mailClient.DownloadMessageFailed -= mailClient_DownloadMessageFailed;
                    //mailClient.DownloadCompleted -= mailClient_DownloadCompleted;
                    mailClient.UpdatedMessage -= mailClient_UpdatedMessage;
                    mailClient.MarkingMessageAsRead -= mailClient_MarkingMessageAsRead;
                    //mailClient.MarkMessageAsRead -= mailClient_MarkedMessageAsRead;
                    mailClient.MarkMessageAsReadFailed -= mailClient_MarkMessageAsReadFailed;
                    mailClient.MarkingMessageAsUnread -= mailClient_MarkingMessageAsUnread;
                    //mailClient.MarkMessageAsUnread -= mailClient_MarkedMessageAsUnread;
                    mailClient.MarkMessageAsUnreadFailed -= mailClient_MarkMessageAsUnreadFailed;
                    mailClient.MarkingMessageAsDeleted -= mailClient_MarkingMessageAsDeleted;
                    mailClient.MarkMessageAsDeleted -= mailClient_MarkedMessageAsDeleted;
                    mailClient.MarkMessageAsDeletedFailed -= mailClient_MarkMessageAsDeletedFailed;
                    mailClient.MarkingMessageAsUndeleted -= mailClient_MarkingMessageAsUndeleted;
                    //mailClient.MarkMessageAsUndeleted -= mailClient_MarkedMessageAsUndeleted;
                    mailClient.MarkMessageAsUndeletedFailed -= mailClient_MarkMessageAsUndeletedFailed;
                    mailClient.MarkingMessageAsFlagged -= mailClient_MarkingMessageAsFlagged;
                    //mailClient.MarkMessageAsFlagged -= mailClient_MarkedMessageAsFlagged;
                    mailClient.MarkMessageAsFlaggedFailed -= mailClient_MarkMessageAsFlaggedFailed; mailClient.MovingMessage -= mailClient_MovingMessage;
                    mailClient.MarkingMessageAsUnflagged -= mailClient_MarkingMessageAsUnflagged;
                    //mailClient.MarkMessageAsUnflagged -= mailClient_MarkedMessageAsUnflagged;
                    mailClient.MarkMessageAsUnflaggedFailed -= mailClient_MarkMessageAsUnflaggedFailed;
                    mailClient.MovingMessage -= mailClient_MovingMessage;
                    mailClient.MovedMessage -= mailClient_MovedMessage;
                    mailClient.MoveMessageFailed -= mailClient_MoveMessageFailed;
                    mailClient.DeletingMessage -= mailClient_DeletingMessage;
                    //mailClient.DeletedMessage -= mailClient_DeletedMessage;
                    mailClient.DeleteMessageFailed -= mailClient_DeleteMessageFailed;
                    mailClient.AddingMailbox -= mailClient_AddingMailbox;
                    mailClient.AddedMailbox -= mailClient_AddedMailbox;
                    mailClient.AddMailboxFailed -= mailClient_AddMailboxFailed;
                    mailClient.RenamingMailbox -= mailClient_RenamingMailbox;
                    mailClient.RenamedMailbox -= mailClient_RenamedMailbox;
                    mailClient.RenameMailboxFailed -= mailClient_RenameMailboxFailed;
                    mailClient.RemovingMailbox -= mailClient_RemovingMailbox;
                    mailClient.RemovedMailbox -= mailClient_RemovedMailbox;
                    mailClient.RemoveMailboxFailed -= mailClient_RemoveMailboxFailed;
                    mailClient.SavingToDrafts -= mailClient_SavingToDrafts;
                    mailClient.SavedToDrafts -= mailClient_SavedToDrafts;
                    mailClient.SaveToDraftsFailed -= mailClient_SaveToDraftsFailed;
                    mailClient.SendingMessage -= mailClient_SendingMessage;
                    mailClient.SentMessage -= mailClient_SentMessage;
                    mailClient.SendMessageFailed -= mailClient_SendMessageFailed;
                    mailClient.LoggingOut -= mailClient_LoggingOut;
                    mailClient.LoggedOut -= mailClient_LoggedOut;
                    mailClient.LogoutFailed -= mailClient_LogoutFailed;
                    mailClient.Disconnected -= mailClient_Disconnected;

                    // Subscribe to all the events
                    mailClient.Connected += mailClient_Connected;
                    mailClient.LoggingIn += mailClient_LoggingIn;
                    mailClient.LoggedIn += mailClient_LoggedIn;
                    mailClient.LoginFailed += mailClient_LoginFailed;
                    mailClient.UpdatingMailboxTree += mailClient_UpdatingMailboxTree;
                    mailClient.UpdatedMailboxTree += mailClient_UpdatedMailboxTree;
                    mailClient.UpdateMailboxTreeFailed += mailClient_UpdateMailboxTreeFailed;
                    mailClient.RestoreStarted += mailClient_RestoreStarted;
                    mailClient.RestoringMessage += mailClient_RestoringMessage;
                    mailClient.RestoredMessage += mailClient_RestoredMessage;
                    mailClient.RestoreMessageFailed += mailClient_RestoreMessageFailed;
                    mailClient.RestoreCompleted += mailClient_RestoreCompleted;
                    mailClient.DownloadStarted += mailClient_DownloadStarted;
                    //mailClient.DownloadingMessage += mailClient_DownloadingMessage;
                    mailClient.DownloadedMessage += mailClient_DownloadedMessage;
                    //mailClient.DownloadMessageFailed += mailClient_DownloadMessageFailed;
                    //mailClient.DownloadCompleted += mailClient_DownloadCompleted;
                    mailClient.UpdatedMessage += mailClient_UpdatedMessage;
                    mailClient.MarkingMessageAsRead += mailClient_MarkingMessageAsRead;
                    //mailClient.MarkMessageAsRead += mailClient_MarkedMessageAsRead;
                    mailClient.MarkMessageAsReadFailed += mailClient_MarkMessageAsReadFailed;
                    mailClient.MarkingMessageAsUnread += mailClient_MarkingMessageAsUnread;
                    //mailClient.MarkMessageAsUnread += mailClient_MarkedMessageAsUnread;
                    mailClient.MarkMessageAsUnreadFailed += mailClient_MarkMessageAsUnreadFailed;
                    mailClient.MarkingMessageAsDeleted += mailClient_MarkingMessageAsDeleted;
                    mailClient.MarkMessageAsDeleted += mailClient_MarkedMessageAsDeleted;
                    mailClient.MarkMessageAsDeletedFailed += mailClient_MarkMessageAsDeletedFailed;
                    mailClient.MarkingMessageAsUndeleted += mailClient_MarkingMessageAsUndeleted;
                    //mailClient.MarkMessageAsUndeleted += mailClient_MarkedMessageAsUndeleted;
                    mailClient.MarkMessageAsUndeletedFailed += mailClient_MarkMessageAsUndeletedFailed;
                    mailClient.MarkingMessageAsFlagged += mailClient_MarkingMessageAsFlagged;
                    //mailClient.MarkMessageAsFlagged += mailClient_MarkedMessageAsFlagged;
                    mailClient.MarkMessageAsFlaggedFailed += mailClient_MarkMessageAsFlaggedFailed; mailClient.MovingMessage += mailClient_MovingMessage;
                    mailClient.MarkingMessageAsUnflagged += mailClient_MarkingMessageAsUnflagged;
                    //mailClient.MarkMessageAsUnflagged += mailClient_MarkedMessageAsUnflagged;
                    mailClient.MarkMessageAsUnflaggedFailed += mailClient_MarkMessageAsUnflaggedFailed;
                    mailClient.MovingMessage += mailClient_MovingMessage;
                    mailClient.MovedMessage += mailClient_MovedMessage;
                    mailClient.MoveMessageFailed += mailClient_MoveMessageFailed;
                    mailClient.DeletingMessage += mailClient_DeletingMessage;
                    //mailClient.DeletedMessage += mailClient_DeletedMessage;
                    mailClient.DeleteMessageFailed += mailClient_DeleteMessageFailed;
                    mailClient.AddingMailbox += mailClient_AddingMailbox;
                    mailClient.AddedMailbox += mailClient_AddedMailbox;
                    mailClient.AddMailboxFailed += mailClient_AddMailboxFailed;
                    mailClient.RenamingMailbox += mailClient_RenamingMailbox;
                    mailClient.RenamedMailbox += mailClient_RenamedMailbox;
                    mailClient.RenameMailboxFailed += mailClient_RenameMailboxFailed;
                    mailClient.RemovingMailbox += mailClient_RemovingMailbox;
                    mailClient.RemovedMailbox += mailClient_RemovedMailbox;
                    mailClient.RemoveMailboxFailed += mailClient_RemoveMailboxFailed;
                    mailClient.SavingToDrafts += mailClient_SavingToDrafts;
                    mailClient.SavedToDrafts += mailClient_SavedToDrafts;
                    mailClient.SaveToDraftsFailed += mailClient_SaveToDraftsFailed;
                    mailClient.SendingMessage += mailClient_SendingMessage;
                    mailClient.SentMessage += mailClient_SentMessage;
                    mailClient.SendMessageFailed += mailClient_SendMessageFailed;
                    mailClient.LoggingOut += mailClient_LoggingOut;
                    mailClient.LoggedOut += mailClient_LoggedOut;
                    mailClient.LogoutFailed += mailClient_LogoutFailed;
                    mailClient.Disconnected += mailClient_Disconnected;

                    // Bind the mailbox tree
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // If this is the first mail client
                        if ((App.EmailToastNotificationContext != null && App.EmailToastNotificationContext.EmailAddress.Equals(mailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase))
                         || (MailboxTreeDataContext == null && mailClient == MailClientsDictionary.First().Value))
                        {
                            BindMailboxTree(mailClient.AccountSettingsData, mailClient.MailboxTree);

                            // If EmailToastNotificationContext is available
                            if (App.EmailToastNotificationContext != null)
                            {
                                try
                                {
                                    lvMailboxTree.SelectedItem = MailboxTreeDataContext.Value.Value.Single(o => o.Mailbox.FullName.Equals(App.EmailToastNotificationContext.MailboxFullName, StringComparison.OrdinalIgnoreCase));
                                    lvMailHeaders.SelectedItem = StorageSettings.MailHeaderDictionary[App.EmailToastNotificationContext.EmailAddress][App.EmailToastNotificationContext.MailboxFullName].Single(o => o.Uid.Equals(App.EmailToastNotificationContext.Uid, StringComparison.OrdinalIgnoreCase));
                                }
                                catch (Exception ex)
                                {
                                    LogFile.Instance.LogError("", "", ex.ToString());
                                }
                            }
                        }
                    });

                    // If App session state is corrupted
                    if (App.IsSessionStateCorrupted)
                        // Do a restore
                        mailClient.RestoreMessages();
                    // Else if not corrupted
                    else
                    {
                        // Login if necessary
                        if (MailClient.IsInternetAvailable()
                        && !mailClient.State.HasFlag(MailClientState.Authenticated))
                            mailClient.Login();
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError("", "", ex.ToString());
                }
            });
        }

        #region MailClient - Event Handlers

        /// <summary>
        /// Occurs when a mail client is connected to a mail server.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        internal void mailClient_Connected(object sender, EventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Connected.");
        }

        /// <summary>
        /// Occurs when a mail client starts a restore of all messages in a mailbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void mailClient_RestoreStarted(object sender, MailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, "Restore started...");

            try
            {
                App.RunOnUIThread(_dispatcher, () =>
                {
                    // Get the selected data
                    MailClient selectedMailClient = SelectedMailClient;
                    Mailbox selectedMailbox = SelectedMailbox;

                    // Set the email headers list
                    if (selectedMailClient != null
                        && selectedMailbox != null
                        && selectedMailClient.AccountSettingsData.EmailAddress.Equals(mailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase)
                        && selectedMailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Reset the restore message progress visibility
                        ResetRestoreMessageProgressVisibility(selectedMailClient, selectedMailbox);
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, "Restore started...");
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to restore a message.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageProgressEventArgs).</param>
        private void mailClient_RestoringMessage(object sender, MessageProgressEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                App.RunOnUIThread(_dispatcher, () =>
                {
                    // Get the selected data
                    MailClient selectedMailClient = SelectedMailClient;
                    Mailbox selectedMailbox = SelectedMailbox;

                    // Set the email headers list
                    if (selectedMailClient != null
                        && selectedMailbox != null
                        && selectedMailClient.AccountSettingsData.EmailAddress.Equals(mailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase)
                        && selectedMailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        pbDownloadProgress.Maximum = e.TotalMessageCount;
                        downloadProgressMessageTotal.Text = string.Format("{0} Messages", e.TotalMessageCount);
                        downloadProgressMessageTotalSize.Text = e.TotalMessageSizeFormat;
                        pbDownloadProgress.Value = e.RemainingMessageCount;
                        downloadProgressMessageNumber.Text = string.Format("{0} Remaining", e.RemainingMessageCount);
                        downloadProgressMessageNumberSize.Text = string.Format("{0} Remaining", e.RemainingMessageSizeFormat);
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client message restore attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_RestoredMessage(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Add the temp mail header
                AddTempMailHeader(mailClient.AccountSettingsData, e.Mailbox, e.Message);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client message restore attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        private void mailClient_RestoreMessageFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.MessageEventArgs.Uid, string.Format("Restore message failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client completes a restore of all messages in a mailbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_RestoreCompleted(object sender, MailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            // If all mailboxes for this client have been restored
            if (e.Mailbox == null)
            {
                // Login if necessary
                if (MailClient.IsInternetAvailable()
                && !mailClient.State.HasFlag(MailClientState.Authenticated))
                    mailClient.Login();
            }
            // Else if this mailbox has been restored
            else
            {
                LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, "Restore completed.");

                try
                {
                    // Copy the temp mail header to the mail header
                    StorageSettings.MailHeaderDictionary.EnsureMailHeader(mailClient.AccountSettingsData, e.Mailbox);
                    _tempMailHeaderDictionary.EnsureMailHeader(mailClient.AccountSettingsData, e.Mailbox);
                    StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName] = new MailHeaderObservableCollectionView(_tempMailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].OrderBy(o => o, new MailHeaderComparer()));
                    ConcurrentDictionary<string, MailHeaderObservableCollectionView> removed;
                    _tempMailHeaderDictionary.TryRemove(mailClient.AccountSettingsData.EmailAddress, out removed);
                    UpdateAccountUnreadEmailCount(mailClient.AccountSettingsData.EmailAddress, StorageSettings.MailHeaderDictionary.GetAccountUnreadEmailCount(mailClient.AccountSettingsData.EmailAddress));

                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Subscribe to mail header dictionary CollectionChanged event
                        MailboxListViewItem mailboxListViewItem = MailboxTreeDataContext.Value.Value.FirstOrDefault(o => o.Mailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase));
                        if (mailboxListViewItem != null)
                            StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].CollectionChanged += mailboxListViewItem.NotifyCollectionChangedEventHandler;

                        // Get the selected data
                        MailClient selectedMailClient = SelectedMailClient;
                        Mailbox selectedMailbox = SelectedMailbox;

                        // Set the email headers list
                        if (selectedMailClient != null
                         && selectedMailbox != null
                         && selectedMailClient.AccountSettingsData.EmailAddress.Equals(mailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase)
                         && selectedMailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Bind mail header
                            BindMailHeader(mailClient.AccountSettingsData, e.Mailbox);

                            // Stop the email headers progress ring
                            ResetEmailHeaderProgressRingState(selectedMailClient, selectedMailbox, false);

                            // Reset the restore message progress visibility
                            ResetRestoreMessageProgressVisibility(selectedMailClient, selectedMailbox);
                        }
                    });
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to log in.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        internal void mailClient_LoggingIn(object sender, EventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Logging in...");
        }

        /// <summary>
        /// Occurs when a mail client log in attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        internal async void mailClient_LoggedIn(object sender, EventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Logged in.");

            try
            {
                // Send unsent messages
                await mailClient.SendUnsentMessages();

                // Download all unread messages
                await mailClient.DownloadUnreadMessages();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client log in attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (FailedEventArgs).</param>
        internal void mailClient_LoginFailed(object sender, FailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Login failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                App.RunOnUIThread(_dispatcher, async () =>
                {
                    try
                    {
                        MessageDialog loginFailedMessageDialog = new MessageDialog(e.UserErrorMessage, string.Format("Login Failed: {0}", mailClient.AccountSettingsData.EmailAddress));
                        await loginFailedMessageDialog.ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to update mailbox tree.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxTreeEventArgs).</param>
        private void mailClient_UpdatingMailboxTree(object sender, MailboxTreeEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Updating mailbox tree...");
        }

        /// <summary>
        /// Occurs when a mail client update mailbox tree attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxTreeEventArgs).</param>
        private void mailClient_UpdatedMailboxTree(object sender, MailboxTreeEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Mailbox tree updated.");

            try
            {
                App.RunOnUIThread(_dispatcher, () =>
                {
                    // Get the selected data
                    MailClient selectedMailClient = SelectedMailClient;

                    // If the selected data is available
                    if (selectedMailClient != null
                     && mailClient.AccountSettingsData.EmailAddress.Equals(selectedMailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        // Bind mailbox tree
                        BindMailboxTree(mailClient.AccountSettingsData, e.MailboxTree);
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client update mailbox tree attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxTreeEventArgs).</param>
        private void mailClient_UpdateMailboxTreeFailed(object sender, MailboxTreeFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Update mailbox tree failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client starts a download of all messages in a mailbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_DownloadStarted(object sender, MailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, "Download started...");

            try
            {
                App.RunOnUIThread(_dispatcher, async () =>
                {
                    // If the progress ring is visible
                    if (prRestoreProgress.IsActive)
                    {
                        // Get the selected data
                        MailClient selectedMailClient = SelectedMailClient;
                        Mailbox selectedMailbox = SelectedMailbox;

                        // Set the email headers list
                        if (selectedMailClient != null
                         && selectedMailbox != null
                         && selectedMailClient.AccountSettingsData.EmailAddress.Equals(mailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase)
                         && selectedMailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Bind mail header
                            BindMailHeader(mailClient.AccountSettingsData, e.Mailbox);
                            await Task.Delay(50);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client attempts to download a message.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (DownloadingMessageEventArgs).</param>
        //private void mailClient_DownloadingMessage(object sender, MessageProgressEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    App.RunOnUIThread(dispatcher, () =>
        //    {
        //        // Reset message progress state
        //        ResetMessageProgressState(mailClient, e);
        //    });
        //}

        /// <summary>
        /// Occurs when a mail client message download attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private async void mailClient_DownloadedMessage(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                App.RunOnUIThread(_dispatcher, async () =>
                {
                    // Add the mail header
                    AddMailHeader(mailClient.AccountSettingsData, e.Mailbox, e.Message);

                    // If the progress ring is visible
                    if (prRestoreProgress.IsActive)
                    {
                        // Get the selected data
                        MailClient selectedMailClient = SelectedMailClient;
                        Mailbox selectedMailbox = SelectedMailbox;

                        // Set the email headers list
                        if (selectedMailClient != null
                         && selectedMailbox != null
                         && selectedMailClient.AccountSettingsData.EmailAddress.Equals(mailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase)
                         && selectedMailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Bind mail header
                            BindMailHeader(mailClient.AccountSettingsData, e.Mailbox);
                            await Task.Delay(100);

                            // Stop the email headers progress ring
                            ResetEmailHeaderProgressRingState(selectedMailClient, selectedMailbox, false);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client message download attempt fails.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (MessageFailedEventArgs).</param>
        //private void mailClient_DownloadMessageFailed(object sender, MessageFailedEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.MessageEventArgs.Uid, string.Format("Download message failed. {0}", e.ErrorMessage.ToString()));
        //}

        ///// <summary>
        ///// Occurs when a mail client completes a download of all messages.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailboxEventArgs).</param>
        ///// <param name="e">The event data (EventArgs).</param>
        //private async void mailClient_DownloadCompleted(object sender, MailboxEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, "Download started...");
        //}

        /// <summary>
        /// Occurs when a mail client message update attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_UpdatedMessage(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid.Equals(e.Uid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Remove message from mail header
                        int mailHeaderIndex = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].IndexOf(mailHeader);
                        StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName][mailHeaderIndex].UpdateFlags(e.Message);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as read.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkingMessageAsRead(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid == e.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as read in control
                        mailHeader.MarkAsRead();
                        MailboxTreeDataContext.Value.Value.Single(o => o.Mailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)).MailHeaderDictionaryCollectionChanged(this, null);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client mark a message as read attempt succeeds.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (MessageEventArgs).</param>
        //private async void mailClient_MarkedMessageAsRead(object sender, MessageEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Uid, "Marked message as read...");
        //}

        /// <summary>
        /// Occurs when a mail client mark a message as read attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkMessageAsReadFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Mark message as read failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.MessageEventArgs.Mailbox.FullName].Where(o => o.Uid == e.MessageEventArgs.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as unread in control
                        mailHeader.MarkAsUnread();
                        MailboxTreeDataContext.Value.Value.Single(o => o.Mailbox.FullName.Equals(e.MessageEventArgs.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)).MailHeaderDictionaryCollectionChanged(this, null);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as unread.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkingMessageAsUnread(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid == e.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as unread in control
                        mailHeader.MarkAsUnread();
                        MailboxTreeDataContext.Value.Value.Single(o => o.Mailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)).MailHeaderDictionaryCollectionChanged(this, null);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client mark a message as unread attempt succeeds.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (MessageEventArgs).</param>
        //private async void mailClient_MarkedMessageAsUnread(object sender, MessageEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Uid, "Marked message as unread...");
        //}

        /// <summary>
        /// Occurs when a mail client mark a message as unread attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkMessageAsUnreadFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Mark message as unread failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.MessageEventArgs.Mailbox.FullName].Where(o => o.Uid == e.MessageEventArgs.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as unread in control
                        mailHeader.MarkAsRead();
                        MailboxTreeDataContext.Value.Value.Single(o => o.Mailbox.FullName.Equals(e.MessageEventArgs.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)).MailHeaderDictionaryCollectionChanged(this, null);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as deleted.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkingMessageAsDeleted(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid == e.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    // Mark as deleted in control
                    mailHeader.MarkAsDeleted();

                    App.RunOnUIThread(_dispatcher, async () =>
                    {
                        await MoveMailHeaders(new MailHeader[] { mailHeader }, mailClient.AccountSettingsData, e.Mailbox, mailClient.DeletedItems);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client mark a message as deleted attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkedMessageAsDeleted(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the deleted items folder
                Mailbox deletedItems = mailClient.DeletedItems;

                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][deletedItems.FullName].FirstOrDefault(o => o.Uid == e.Uid);
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][deletedItems.FullName][StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][deletedItems.FullName].IndexOf(mailHeader)] = new MailHeader(mailClient.AccountSettingsData.EmailAddress, e.Message.Uid, e.Message, deletedItems);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client mark a message as deleted attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkMessageAsDeletedFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Mark message as deleted failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.MessageEventArgs.Mailbox.FullName].Where(o => o.Uid == e.MessageEventArgs.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    // Mark as undeleted in control
                    mailHeader.MarkAsUndeleted();

                    App.RunOnUIThread(_dispatcher, async () =>
                    {
                        await MoveMailHeaders(new MailHeader[] { mailHeader }, mailClient.AccountSettingsData, mailClient.DeletedItems, e.MessageEventArgs.Mailbox);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as undeleted.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkingMessageAsUndeleted(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid == e.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    // Mark as undeleted in control
                    mailHeader.MarkAsUndeleted();
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client mark a message as undeleted attempt succeeds.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (MessageEventArgs).</param>
        //private async void mailClient_MarkedMessageAsUndeleted(object sender, MessageEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Uid, "Marked message as undeleted...");
        //}

        /// <summary>
        /// Occurs when a mail client mark a message as undeleted attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkMessageAsUndeletedFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Mark message as undeleted failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.MessageEventArgs.Mailbox.FullName].Where(o => o.Uid == e.MessageEventArgs.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    // Mark as undeleted in control
                    mailHeader.MarkAsDeleted();
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as flagged.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkingMessageAsFlagged(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid == e.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as flagged in control
                        mailHeader.MarkAsFlagged();
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client mark a message as flagged attempt succeeds.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (MessageEventArgs).</param>
        //private async void mailClient_MarkedMessageAsFlagged(object sender, MessageEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Uid, "Marked message as flagged...");
        //}

        /// <summary>
        /// Occurs when a mail client mark a message as flagged attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkMessageAsFlaggedFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Mark message as flagged failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.MessageEventArgs.Mailbox.FullName].Where(o => o.Uid == e.MessageEventArgs.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as unflagged in control
                        mailHeader.MarkAsUnflagged();
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as unflagged.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkingMessageAsUnflagged(object sender, MessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid == e.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as unflagged in control
                        mailHeader.MarkAsUnflagged();
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client mark a message as unflagged attempt succeeds.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (MessageEventArgs).</param>
        //private async void mailClient_MarkedMessageAsUnflagged(object sender, MessageEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Uid, "Marked message as unflagged...");
        //}

        /// <summary>
        /// Occurs when a mail client mark a message as unflagged attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private void mailClient_MarkMessageAsUnflaggedFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Mark message as unflagged failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.MessageEventArgs.Mailbox.FullName].Where(o => o.Uid == e.MessageEventArgs.Uid).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Mark as flagged in control
                        mailHeader.MarkAsFlagged();
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to move a message.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void mailClient_MovingMessage(object sender, MoveMessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Moving message...");

            try
            {
                App.RunOnUIThread(_dispatcher, async () =>
                {
                    await MoveMailHeaders(lvMailHeaders.Items.Cast<MailHeader>().Where(o => e.MessagePaths.ContainsKey(o.Uid)), mailClient.AccountSettingsData, e.Mailbox, e.DestinationMailbox);
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client move message attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void mailClient_MovedMessage(object sender, MoveMessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, string.Join(", ", e.MessagePaths.Keys), "Moved message(s).");

            try
            {
                // Loop through each message
                foreach (KeyValuePair<string, string> messagePath in e.MessagePaths)
                {
                    // Get the mail header
                    MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.DestinationMailbox.FullName].FirstOrDefault(o => o.Uid == messagePath.Key);
                    if (mailHeader != null
                     && e.Messages.ContainsKey(messagePath.Key))
                    {
                        StructuredMessage message = e.Messages[messagePath.Key];
                        App.RunOnUIThread(_dispatcher, () =>
                        {
                            StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.DestinationMailbox.FullName][StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.DestinationMailbox.FullName].IndexOf(mailHeader)] = new MailHeader(mailClient.AccountSettingsData.EmailAddress, message.Uid, message, e.DestinationMailbox);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client move message attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void mailClient_MoveMessageFailed(object sender, MessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Move message failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client attempts to delete a message.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (DeleteMessageEventArgs).</param>
        private void mailClient_DeletingMessage(object sender, DeleteMessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            try
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => o.Uid == e.MessagePaths.Keys.First()).FirstOrDefault();

                // If the mail header is found
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        // Remove message from mail header
                        int mailHeaderIndex = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].IndexOf(mailHeader);
                        StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Remove(mailHeader);
                        EnsureItemIsSelected(lvMailHeaders, new object[] { GetNextMailHeader(mailClient, e.Mailbox, mailHeaderIndex) });
                        ResetHtmlDocumentState();
                        ResetEmailHeaderProgressRingState(mailClient, e.Mailbox, false);
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Occurs when a mail client delete message attempt succeeds.
        ///// </summary>
        ///// <param name="sender">The object that raised the event (MailClient).</param>
        ///// <param name="e">The event data (MessageEventArgs).</param>
        //private async void mailClient_DeletedMessage(object sender, DeleteMessageEventArgs e)
        //{
        //    // Get the mail client
        //    MailClient mailClient = (MailClient)sender;

        //    LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, string.Join(",", e.MessagePaths.Keys), "Message deleted.");

        //    App.RunOnUIThread(dispatcher, () =>
        //    {
        //        EnsureItemIsSelected(lvMailHeaders, new object[] { lvMailHeaders.Items.FirstOrDefault() }, MessageHeader_SelectionChanged);
        //    });
        //}

        /// <summary>
        /// Occurs when a mail client delete message attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (DeleteMessageFailedEventArgs).</param>
        private async void mailClient_DeleteMessageFailed(object sender, DeleteMessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.DeleteMessage.Mailbox.Name, string.Format("Delete message failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                // Add mail header back
                StorageFile messageFile = await IOUtil.FileExists(e.DeleteMessage.MessagePaths.Values.First());
                if (messageFile != null)
                {
                    StructuredMessage message = await mailClient.LoadMessage(messageFile);
                    StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.DeleteMessage.Mailbox.FullName].Remove(new MailHeader(mailClient.AccountSettingsData.EmailAddress, e.DeleteMessage.MessagePaths.Keys.First(), message, e.DeleteMessage.Mailbox));
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.DeleteMessage.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client attempts to add a mailbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_AddingMailbox(object sender, MailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Adding mailbox...");

            try
            {
                // Set all dictionaries
                StorageSettings.MailHeaderDictionary.EnsureMailHeader(mailClient.AccountSettingsData, e.Mailbox);

                App.RunOnUIThread(_dispatcher, () =>
                {
                    // Close bottom app bar if open
                    if (BottomAppBar.IsOpen)
                        BottomAppBar.IsOpen = false;
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client add mailbox attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_AddedMailbox(object sender, MailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Added mailbox.");
        }

        /// <summary>
        /// Occurs when a mail client add mailbox attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_AddMailboxFailed(object sender, MailboxFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, string.Format("Add mailbox failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client attempts to rename a mailbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (RenameMailboxEventArgs).</param>
        private void mailClient_RenamingMailbox(object sender, RenameMailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.RenamedMailboxes.First().Key.Name, "Renaming mailbox...");

            try
            {
                App.RunOnUIThread(_dispatcher, async () =>
                {
                    // Move the mail headers
                    await MoveMailHeaders(StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.RenamedMailboxes.First().Key.FullName], mailClient.AccountSettingsData, e.RenamedMailboxes.First().Key, e.RenamedMailboxes.First().Value);

                    // Bind the mail headers
                    BindMailHeader(mailClient.AccountSettingsData, e.RenamedMailboxes.First().Value);

                    // Close bottom app bar if open
                    if (BottomAppBar.IsOpen)
                        BottomAppBar.IsOpen = false;
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client rename mailbox attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (RenameMailboxEventArgs).</param>
        private void mailClient_RenamedMailbox(object sender, RenameMailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.RenamedMailboxes.First().Key.Name, "Renamed mailbox.");
        }

        /// <summary>
        /// Occurs when a mail client rename mailbox attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (RenameMailboxEventArgs).</param>
        private void mailClient_RenameMailboxFailed(object sender, RenameMailboxFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.RenamedMailboxes.First().Key.Name, string.Format("Rename mailbox failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client attempts to remove a mailbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_RemovingMailbox(object sender, MailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Removing mailbox...");
        }

        /// <summary>
        /// Occurs when a mail client remove mailbox attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_RemovedMailbox(object sender, MailboxEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Remove mailbox.");

            try
            {
                App.RunOnUIThread(_dispatcher, () =>
                {
                    MailboxTreeDataContext.Value.Value.Remove(MailboxTreeDataContext.Value.Value.First(o => o.Mailbox.FullName.Equals(e.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)));
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client remove mailbox attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        private void mailClient_RemoveMailboxFailed(object sender, MailboxFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, string.Format("Remove mailbox failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client attempts to save to drafts.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SaveToDraftsEventArgs).</param>
        private void mailClient_SavingToDrafts(object sender, SaveToDraftsEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Saving to drafts...");

            App.RunOnUIThread(_dispatcher, () =>
            {
                // Add the mail header
                AddMailHeader(mailClient.AccountSettingsData, mailClient.Drafts, e.Message);
            });
        }

        /// <summary>
        /// Occurs when a mail client save to drafts attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SaveToDraftsEventArgs).</param>
        private void mailClient_SavedToDrafts(object sender, SaveToDraftsEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Saved to drafts.");

            if (mailClient is ImapMailClient)
            {
                // Get the mail header
                MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].FirstOrDefault(o => o.Comments.Equals(e.Message.Header.Comments, StringComparison.OrdinalIgnoreCase));
                if (mailHeader != null)
                {
                    App.RunOnUIThread(_dispatcher, () =>
                    {
                        StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName][StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].IndexOf(mailHeader)] = new MailHeader(mailClient.AccountSettingsData.EmailAddress, e.Message.Uid, e.Message, e.Mailbox);
                    });
                    ComposeMailPage.MessageNavigationContext.Message = e.Message;
                }
            }
        }

        /// <summary>
        /// Occurs when a mail client save to drafts attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SaveToDraftsFailedEventArgs).</param>
        private void mailClient_SaveToDraftsFailed(object sender, SaveToDraftsFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.SaveToDraftsEvent.Mailbox.Name, string.Format("Save to drafts failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client attempts to send a message.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SendMessageEventArgs).</param>
        private void mailClient_SendingMessage(object sender, SendMessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Sending message...");

            try
            {
                App.RunOnUIThread(_dispatcher, () =>
                {
                    // Add the mail header to outbox
                    AddMailHeader(mailClient.AccountSettingsData, e.Mailbox, e.Messages.First().Value);

                    // Navigate from compose mail page to main page
                    GoBack(this, new RoutedEventArgs());
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client send message attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SendMessageEventArgs).</param>
        private void mailClient_SentMessage(object sender, SendMessageEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.Name, "Sent message.");

            try
            {
                App.RunOnUIThread(_dispatcher, async () =>
                {
                    // Move the mail headers
                    await MoveMailHeaders(StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Where(o => e.MessagePaths.ContainsKey(o.Uid)), mailClient.AccountSettingsData, e.Mailbox, e.DestinationMailbox);

                    // Loop through each message
                    foreach (KeyValuePair<string, string> messagePath in e.MessagePaths)
                    {
                        // Get the mail header
                        MailHeader mailHeader = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.DestinationMailbox.FullName].FirstOrDefault(o => o.Uid == messagePath.Key);
                        if (mailHeader != null
                         && e.Messages.ContainsKey(messagePath.Key))
                        {
                            StructuredMessage message = e.Messages[messagePath.Key];
                            App.RunOnUIThread(_dispatcher, () =>
                            {
                                StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.DestinationMailbox.FullName][StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.DestinationMailbox.FullName].IndexOf(mailHeader)] = new MailHeader(mailClient.AccountSettingsData.EmailAddress, message.Uid, message, e.DestinationMailbox);
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client send message attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SendMessageFailedEventArgs).</param>
        private void mailClient_SendMessageFailed(object sender, SendMessageFailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.SendMessage.Mailbox.Name, string.Format("Send message failed. {0}", e.ErrorMessage.ToString()));
        }

        /// <summary>
        /// Occurs when a mail client attempts to log out.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void mailClient_LoggingOut(object sender, EventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Logging out...");
        }

        /// <summary>
        /// Occurs when a mail client log out attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void mailClient_LoggedOut(object sender, EventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Logged out.");
        }

        /// <summary>
        /// Occurs when a mail client log out attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (FailedEventArgs).</param>
        private void mailClient_LogoutFailed(object sender, FailedEventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, string.Format("Logout failed. {0}", e.ErrorMessage.ToString()));

            try
            {
                App.RunOnUIThread(_dispatcher, async () =>
                {
                    try
                    {
                        MessageDialog loginFailedMessageDialog = new MessageDialog(e.UserErrorMessage, string.Format("Logout Failed: {0}", mailClient.AccountSettingsData.EmailAddress));
                        await loginFailedMessageDialog.ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
                    }
                });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client is disconnected from a mail server.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void mailClient_Disconnected(object sender, EventArgs e)
        {
            // Get the mail client
            MailClient mailClient = (MailClient)sender;

            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, null, "Disconnected.");
        }

        #endregion

        #region Main Page Control - Event Handlers

        /// <summary>
        /// Occurs when the AppBar changes from hidden to visible.
        /// </summary>
        /// <param name="sender">The object that raised the event (BottomAppBar).</param>
        /// <param name="e">The event data (object).</param>
        private void BottomAppBar_Opened(object sender, object e)
        {
            // Make sure bottom app bar is closed if a dialog is open
            if (MainPage.Current.IsDialogOpen || pMoveToFolder.IsOpen)
            {
                BottomAppBar.IsOpen = false;
                return;
            }

            App.FillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
        }

        /// <summary>
        /// Occurs when the AppBar changes from visible to hidden.
        /// </summary>
        /// <param name="sender">The object that raised the event (BottomAppBar).</param>
        /// <param name="e">The event data (object).</param>
        private void BottomAppBar_Closed(object sender, object e)
        {
            // Make sure bottom app bar is closed if a dialog is open
            if (MainPage.Current.IsDialogOpen || pMoveToFolder.IsOpen)
                return;

            App.UnfillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
        }

        /// <summary>
        /// Occurs when the account settings control changes from hidden to visible.
        /// </summary>
        /// <param name="sender">The object that raised the event (AccountsSettingsControl).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void listAccountsSettingsControl_Opened(object sender, EventArgs e)
        {
            App.FillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;
        }

        /// <summary>
        /// Occurs when the account settings control changes from visible to hidden.
        /// </summary>
        /// <param name="sender">The object that raised the event (AccountsSettingsControl).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void listAccountsSettingsControl_Closed(object sender, EventArgs e)
        {
            App.UnfillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;
        }

        /// <summary>
        /// Occurs when the privacy settings control changes from hidden to visible.
        /// </summary>
        /// <param name="sender">The object that raised the event (PrivacySettingsControl).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void pscPrivacySettingsControl_Opened(object sender, EventArgs e)
        {
            App.FillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
        }

        /// <summary>
        /// Occurs when the privacy settings control changes from visible to hidden.
        /// </summary>
        /// <param name="sender">The object that raised the event (PrivacySettingsControl).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void pscPrivacySettingsControl_Closed(object sender, EventArgs e)
        {
            App.UnfillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
        }

        /// <summary>
        /// Occurs when the currently selected mail state changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (SelectionChangedEventArgs).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void cbEmailFlags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Make sure the controls are initialised
                if (MailClientsDictionary == null) return;
                else if (tbEmailAddress == null) return;
                else if (lvMailboxTree == null) return;

                // Try to get the mail client
                MailClient mailClient = SelectedMailClient;
                if (mailClient != null)
                {
                    // Try to get the mailbox
                    MailboxListViewItem mailboxListViewItem = SelectedMailboxListViewItem;
                    if (mailboxListViewItem != null)
                    {
                        // Bind the mail header
                        _isEmailFlagFiltered = true;
                        BindMailHeader(mailClient.AccountSettingsData, mailboxListViewItem.Mailbox);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the search button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchPane.GetForCurrentView().Show();
        }

        /// <summary>
        /// Occurs when the cancel query filter button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bCancelQueryFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Make sure the app bar is hidden if query is cancelled
                BottomAppBar.IsOpen = false;

                // Reset the email header filter state
                ResetEmailHeaderFilterState(false, string.Empty);

                // Get the selected data
                MailClient mailClient = SelectedMailClient;
                Mailbox mailbox = SelectedMailbox;

                // If the data is available
                if (mailClient != null
                 && mailbox != null)
                {
                    // Bind mail headers
                    BindMailHeader(mailClient.AccountSettingsData, mailbox, null);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when an item in the Mailbox receives an interaction, and the IsItemClickEnabled property is true.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListView).</param>
        /// <param name="e">The event data (ItemClickEventArgs).</param>
        private void lvMailbox_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                // Get the selected data
                AccountSettingsData accountSettingsData = ((KeyValuePair<string, AccountSettingsData>)e.ClickedItem).Value;

                // Get the mail client
                MailClient mailClient = null;
                if (MailClientsDictionary.TryGetValue(accountSettingsData.EmailAddress, out mailClient))
                {
                    // Ensure mail header dictionary entry
                    StorageSettings.MailHeaderDictionary.EnsureMailHeader(accountSettingsData, mailClient.Inbox);
                    // Reset the email header filter state
                    ResetEmailHeaderFilterState(false, string.Empty);
                    // Bind mailbox tree
                    BindMailboxTree(mailClient.AccountSettingsData, mailClient.MailboxTree);
                    // Bind mail headers
                    BindMailHeader(accountSettingsData, mailClient.Inbox);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when an otherwise unhandled Tap interaction occurs over the hit test area of this mailbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (Mailbox).</param>
        /// <param name="e">The event data (TappedRoutedEventArgs).</param>
        private async void MailboxTree_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // If a move is in progress
            if (pMoveToFolder.IsOpen)
            {
                try
                {
                    // Move the mail headers
                    lvMailHeaders.Focus(FocusState.Pointer);
                    IEnumerable<MailHeader> mailHeaders = lvMailHeaders.SelectedItems.Cast<MailHeader>();
                    await MoveMailHeaders(mailHeaders, StorageSettings.AccountSettingsDataDictionary[mailHeaders.First().EmailAddress], mailHeaders.First().Mailbox, SelectedMailbox);
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError("", "", ex.ToString());
                }
                finally
                {
                    pMoveToFolder.IsOpen = false;
                    IList<object> list = new List<object>();
                    MailboxTree_SelectionChanged(sender, new SelectionChangedEventArgs(list, list));
                }
            }
        }

        /// <summary>
        /// Occurs when the currently selected mail box folder changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (Mailbox).</param>
        /// <param name="e">The event data (SelectionChangedEventArgs).</param>
        private void MailboxTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // If a move is in progress
                if (!pMoveToFolder.IsOpen)
                {
                    // Ensure one item is selected
                    EnsureItemIsSelected(lvMailboxTree, e.RemovedItems, MailboxTree_SelectionChanged);

                    // Sets the delete button glyph
                    SetDeleteButtonGlyph();

                    // Get the selected data
                    MailClient selectedMailClient = SelectedMailClient;
                    Mailbox selectedMailbox = SelectedMailbox;

                    // Get the mail client
                    if (selectedMailClient != null)
                    {
                        // Set the selected account
                        SelectedAccount = lvMailbox.Items.Cast<KeyValuePair<string, AccountSettingsData>>().SingleOrDefault(o => o.Key.Equals(selectedMailClient.AccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase));

                        // Get the selected mailbox folder
                        if (selectedMailbox != null)
                        {
                            // Enable or disable drag and drop depending on mailbox
                            EnabledDisableDragAndDrop(selectedMailbox);

                            // Reset the email header filter state
                            ResetEmailHeaderFilterState(false, string.Empty);

                            // Reset restore message progress visibility
                            ResetRestoreMessageProgressVisibility(selectedMailClient, selectedMailbox);

                            // Reset message progress state
                            ResetRestoreMessageProgressState(selectedMailClient, selectedMailClient.MailboxProgressStatus[selectedMailbox.FullName]);

                            // Set the task processor mailbox
                            selectedMailClient.TaskProcessorMailbox = selectedMailbox;

                            // Show/hide the appropriate bottom app bar buttons
                            ShowHideBottomAppBarButtons(Visibility.Visible, Visibility.Visible, () => { ShowHideMailboxAppBarButtons(SelectedMailboxListViewItem); }, Visibility.Collapsed, () => { });

                            // If the bottom app bar is open
                            if (BottomAppBar.IsOpen)
                            {
                                // Close it
                                BottomAppBar.IsOpen = false;
                            }
                            // If the bottom app bar is closed
                            else
                            {
                                // If it's not the default selection
                                if (e.AddedItems.Count != 1
                                 && e.RemovedItems.Count != 0)
                                {
                                    // Open it
                                    BottomAppBar.IsSticky = true;
                                    BottomAppBar.IsOpen = true;
                                }
                            }

                            // If the mailbox list view item is not empty
                            if (selectedMailbox != null
                             && MailClientsDictionary.Count > 0)
                            {
                                // If the mailbox currently being restored is the same as the one that was selected
                                if (selectedMailClient.MailboxRestoreStatus[selectedMailbox.FullName] == MailboxActionState.Completed
                                 && selectedMailClient.MailboxDownloadStatus[selectedMailbox.FullName] == MailboxActionState.Busy)
                                {
                                    // Start the email headers progress ring
                                    ResetEmailHeaderProgressRingState(selectedMailClient, selectedMailbox, true);
                                }
                                else
                                {
                                    // Ensure the mail header dictionary entry exists
                                    EnsureTempMailHeader(StorageSettings.AccountSettingsDataDictionary[selectedMailClient.AccountSettingsData.EmailAddress], selectedMailbox);

                                    // Bind the email headers to the selected account and folder
                                    BindMailHeader(StorageSettings.AccountSettingsDataDictionary[selectedMailClient.AccountSettingsData.EmailAddress], selectedMailbox);

                                    // Stop the email headers progress ring
                                    ResetEmailHeaderProgressRingState(selectedMailClient, selectedMailbox, false);

                                    // If there are mail headers for this account
                                    if (StorageSettings.MailHeaderDictionary[selectedMailClient.AccountSettingsData.EmailAddress][selectedMailbox.FullName].Count > 0
                                     && lvMailHeaders.SelectedItem == null)
                                        lvMailHeaders.SelectedIndex = 0;
                                    // Else if the folder for this account has no emails
                                    else if (StorageSettings.MailHeaderDictionary[selectedMailClient.AccountSettingsData.EmailAddress][selectedMailbox.FullName].Count == 0)
                                        ResetHtmlDocumentState();
                                }
                            }
                        }
                        // Else if the mailbox list view item is empty
                        else if (lvMailHeaders.SelectedItem == null)
                            ResetHtmlDocumentState();
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the input system reports an underlying drag event with the Mailbox Tree Folder as the target.
        /// </summary>
        /// <param name="sender">The object that raised the event (Grid).</param>
        /// <param name="e">The event data (DragEventArgs).</param>
        private void MailboxTreeFolder_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                Grid grid = (Grid)sender;
                grid.Background = AppResources.SecondaryColourBrush;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the input system reports an underlying drag event with the Mailbox Tree Folder as the origin.
        /// </summary>
        /// <param name="sender">The object that raised the event (Grid).</param>
        /// <param name="e">The event data (DragEventArgs).</param>
        private void MailboxTreeFolder_DragLeave(object sender, DragEventArgs e)
        {
            try
            {
                Grid grid = (Grid)sender;
                grid.Background = new SolidColorBrush(Colors.Transparent);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the input system reports an underlying drop event with the Mailbox Tree Folder as the drop target.
        /// </summary>
        /// <param name="sender">The object that raised the event (Grid).</param>
        /// <param name="e">The event data (DragEventArgs).</param>
        private async void MailboxTreeFolder_Drop(object sender, DragEventArgs e)
        {
            try
            {
                // Get the mail client
                MailClient selectedMailClient = SelectedMailClient;

                // If the mail client exists
                if (selectedMailClient != null)
                {
                    // Get the source mailbox list view item
                    MailboxListViewItem sourceMailboxListViewItem = SelectedMailboxListViewItem;

                    // Get the destination mailbox list view item
                    MailboxListViewItem destinationMailboxListViewItem = (MailboxListViewItem)((FrameworkElement)sender).DataContext;

                    // Get the messages to move
                    Dictionary<string, string> messages = IOUtil.Deserialize<Dictionary<string, string>>(await e.Data.GetView().GetTextAsync());

                    // Move the messages
                    selectedMailClient.MoveMessage(messages, sourceMailboxListViewItem.Mailbox, destinationMailboxListViewItem.Mailbox);
                }

                Grid grid = (Grid)sender;
                grid.Background = new SolidColorBrush(Colors.Transparent);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a right-tap input stimulus happens while the pointer is over the element.
        /// </summary>
        /// <param name="sender">The object that raised the event (Grid).</param>
        /// <param name="e">The event data (RightTappedRoutedEventArgs).</param>
        private void MailboxTreeFolder_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;

            // Show/hide the appropriate bottom app bar buttons
            ShowHideBottomAppBarButtons(Visibility.Visible, Visibility.Visible, () => { ShowHideMailboxAppBarButtons(SelectedMailboxListViewItem); }, Visibility.Collapsed, () => { });

            // If BottomAppBar is Closed
            if (!BottomAppBar.IsOpen)
                BottomAppBar.IsOpen = true;
        }

        /// <summary>
        /// Occurs when the Mailbox Text Box receives focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MailboxTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;
        }

        /// <summary>
        /// Occurs when a keyboard key is pressed while the Mailbox TextBox has focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MailboxTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Return if Enter key was not pressed
            if (e.Key == VirtualKey.Enter)
            {
                // Remove focus
                lvMailboxTree.Focus(FocusState.Pointer);
            }
        }

        /// <summary>
        /// Occurs when the Mailbox Text Box looses focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void MailboxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get the selected mail client
            MailClient selectedMailClient = SelectedMailClient;

            // Get the mailbox textbox
            TextBox mailboxTextBox = (TextBox)sender;

            // Process mailbox textbox
            await ProcessMailboxTextBox(selectedMailClient, mailboxTextBox);

            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;
        }

        /// <summary>
        /// Occurs when a keyboard key is pressed while the Message Header has focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListView).</param>
        /// <param name="e">The event data (KeyRoutedEventArgs).</param>
        private void MessageHeader_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                // If Delete
                if (e.Key == VirtualKey.Delete)
                    bDelete_Click(sender, e);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the currently selected email header changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListBox).</param>
        /// <param name="e">The event data (SelectionChangedEventArgs).</param>
        private async void MessageHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                lvMailHeaders.Focus(FocusState.Pointer);
                if (!IsDialogOpen) emailBodyText.Visibility = Visibility.Visible;
                EnsureItemIsSelected(lvMailHeaders, e.RemovedItems, MessageHeader_SelectionChanged);

                // Default junk visibility
                Visibility junkVisibility = Visibility.Collapsed;
                Visibility notJunkVisibility = Visibility.Collapsed;

                // If the mail client is available
                MailClient mailClient = SelectedMailClient;
                if (mailClient != null)
                {
                    // Get the selected mailbox
                    Mailbox selectedMailbox = SelectedMailbox;

                    // If a mailbox is selected
                    if (selectedMailbox != null)
                    {
                        // Show junk visibility if not junk mail
                        junkVisibility = (selectedMailbox.FullName.Equals(mailClient.JunkMail.FullName, StringComparison.OrdinalIgnoreCase) ? Visibility.Collapsed : Visibility.Visible);
                        // Show not junk visibility if junk mail
                        notJunkVisibility = (selectedMailbox.FullName.Equals(mailClient.JunkMail.FullName, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed);
                    }
                }

                // Get mark as read visibility
                Visibility markAsReadVisibility = (lvMailHeaders.SelectedItems.Cast<MailHeader>()
                                                  .Any(o => !o.IsSeen) ? Visibility.Visible : Visibility.Collapsed);

                // Get mark as seen visibility
                Visibility markAsUnreadVisibility = (markAsReadVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);

                // Set the flag button automation name
                string flagButtonAutomationName = (lvMailHeaders.SelectedItems.Cast<MailHeader>()
                                                  .Any(o => !o.IsFlagged) ? "Flag" : "Remove flag");
                Flag.SetValue(AutomationProperties.NameProperty, flagButtonAutomationName);

                // Show/hide the appropriate bottom app bar buttons
                ShowHideBottomAppBarButtons(Visibility.Visible, Visibility.Collapsed, () => { }, Visibility.Visible, () => { Junk.Visibility = junkVisibility; NotJunk.Visibility = notJunkVisibility; MarkAsRead.Visibility = markAsReadVisibility; MarkAsUnread.Visibility = markAsUnreadVisibility; });

                // If one item is selected
                if (lvMailHeaders.SelectedItems.Count == 1)
                {
                    // If the bottom app bar is open
                    if (BottomAppBar.IsOpen)
                    {
                        // Close it
                        BottomAppBar.IsOpen = false;
                        App.UnfillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
                    }
                    // If the bottom app bar is closed
                    else
                    {
                        // If it's not the default selection
                        if (e.AddedItems.Count != 1
                         && e.RemovedItems.Count != 0
                         && !_isEmailFlagFiltered)
                        {
                            // Open it
                            BottomAppBar.IsSticky = true;
                            BottomAppBar.IsOpen = true;
                            App.FillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
                        }
                    }
                }
                // Else if more than one item is selected
                else if (lvMailHeaders.SelectedItems.Count > 1)
                {
                    // Open it
                    BottomAppBar.IsSticky = true;
                    BottomAppBar.IsOpen = true;
                    App.FillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
                }

                // Get the mail header
                MailHeader mailHeader = (MailHeader)lvMailHeaders.SelectedItem;

                // Make sure the mail header is not empty
                if (mailHeader != null)
                {
                    // Get the current bound message
                    StructuredMessage bodyMessage = emailBodyLayout.DataContext as StructuredMessage;

                    // If we are dealing with a new message
                    StorageFile mailMessageFile;
                    if ((bodyMessage == null
                     || !bodyMessage.Uid.Equals(mailHeader.Uid, StringComparison.OrdinalIgnoreCase))
                     && (mailMessageFile = await IOUtil.FileExists(mailHeader.MessagePath)) != null)
                    {
                        // Get the new message
                        StructuredMessage mailMessage = await IOUtil.LoadMessage(mailHeader.IsImapMessage, mailMessageFile);

                        // Show/hide mail app buttons
                        ShowHideMailAppButtons(Visibility.Visible, mailMessage);

                        // Bind the message
                        emailBodyLayout.DataContext = mailMessage;

                        // Display the html
                        string html = await IOUtil.FormattedHtml(mailMessage);
                        emailBodyText.NavigateToString(html);
                        lvMailHeaders.Focus(FocusState.Pointer);

                        // Set the attachments list
                        alvAttachments.ItemsSource = mailMessage.Attachments;
                        alvAttachments.Visibility = (mailMessage.Attachments.Count > 0 ? Visibility.Visible : Visibility.Collapsed);

                        // Mark message as read
                        MarkMessageAsRead(mailHeader);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(null, null, ex.ToString());
            }
            finally
            {
                // Reset is email flag filtered
                _isEmailFlagFiltered = false;
            }
        }

        /// <summary>
        /// Occurs when a drag operation that involves one of the items in the view is initiated.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListBox).</param>
        /// <param name="e">The event data (DragItemsStartingEventArgs).</param>
        private void MessageHeader_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            try
            {
                e.Data.SetText(IOUtil.Serialise(e.Items.Cast<MailHeader>().ToDictionary(o => o.Uid, o => o.MessagePath)));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when top-level navigation completes and the content loads into the WebView control or when an error occurs during loading.
        /// </summary>
        /// <param name="sender">The object that raised the event (WebView).</param>
        /// <param name="e">The event data (NavigationEventArgs).</param>
        private void emailBodyText_LoadCompleted(object sender, NavigationEventArgs e)
        {
            lvMailHeaders.Focus(FocusState.Pointer);
        }

        /// <summary>
        /// Occurs when a pointer related event is raised.
        /// </summary>
        /// <param name="sender">The object that raised the event (WebView).</param>
        /// <param name="e">The event data (PointerRoutedEventArgs).</param>
        private void emailBodyText_Pointer(object sender, PointerRoutedEventArgs e)
        {
            lvMailHeaders.Focus(FocusState.Pointer);
        }

        #region Bottom App Bar Button - Event Handlers

        /// <summary>
        /// Occurs when the Sync button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close the bottom app bar
                BottomAppBar.IsOpen = false;

                // Show the accounts
                lascListAccountsSettingsControl.ShowListAccount();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Sync button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void Sync_Click(object sender, RoutedEventArgs e)
        {
            // Close bottom app bar
            BottomAppBar.IsOpen = false;

            // Get the mail client
            MailClient mailClient = SelectedMailClient;

            if (mailClient != null)
            {
                try
                {
                    // If internet is available
                    if (MailClient.IsInternetAvailable())
                    {
                        // Sync
                        mailClient.Sync();
                    }
                    // Else if internet is not available
                    else
                    {
                        try
                        {
                            MessageDialog loginFailedMessageDialog = new MessageDialog("Your connection to the internet is not available at this time.", "Login Failed: Internet Connection Unavailable");
                            await loginFailedMessageDialog.ShowAsync();
                        }
                        catch (Exception ex)
                        {
                            LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError("", "", ex.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when the Folder options button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void FolderOptions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a menu and add commands specifying a callback delegate for each.
                // Since command delegates are unique, no need to specify command Ids.
                PopupMenu menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Empty folder", (command) =>
                {
                    try
                    {
                        // Close the bottom app bar
                        BottomAppBar.IsOpen = false;

                        // Select all mailheaders
                        SelectAll_Click(sender, e);

                        // Delete all selected mail headers
                        Delete_Click(sender, e);

                        // Clear the selected mail headers
                        lvMailHeaders.SelectedItems.Clear();
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError("", "", ex.ToString());
                    }
                }));
                menu.Commands.Add(new UICommand("Mark as unread", (command) =>
                {
                    try
                    {
                        BottomAppBar.IsOpen = false;
                        SelectAll_Click(sender, e);
                        MarkAsUnread_Click(sender, e);
                        lvMailHeaders.SelectedItems.Clear();
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError("", "", ex.ToString());
                    }
                }));
                menu.Commands.Add(new UICommandSeparator());
                if (AddMailbox.Visibility == Visibility.Visible)
                {
                    menu.Commands.Add(new UICommand("New folder", (command) =>
                    {
                        AddMailbox_Click(sender, e);
                    }));
                }
                if (RenameMailbox.Visibility == Visibility.Visible)
                {
                    menu.Commands.Add(new UICommand("Rename folder", (command) =>
                    {
                        RenameMailbox_Click(sender, e);
                    }));
                }
                if (DeleteMailbox.Visibility == Visibility.Visible)
                {
                    menu.Commands.Add(new UICommand("Delete folder", (command) =>
                    {
                        DeleteMailbox_Click(sender, e);
                    }));
                }
                // Show the menu
                await menu.ShowForSelectionAsync(App.GetElementRect(FolderOptions), Placement.Above);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Select all button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvMailHeaders.SelectionMode == ListViewSelectionMode.Multiple ||
                    lvMailHeaders.SelectionMode == ListViewSelectionMode.Extended)
                {
                    lvMailHeaders.Focus(FocusState.Pointer);
                    lvMailHeaders.SelectAll();
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Add Mailbox button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void AddMailbox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close bottom app bar
                BottomAppBar.IsOpen = false;

                MailboxListViewItem mailboxListViewItem = lvMailboxTree.SelectedItem as MailboxListViewItem;
                if (mailboxListViewItem != null)
                {
                    _isLastMailboxActionAdd = true;
                    int position = MailboxTreeDataContext.Value.Value.IndexOf(mailboxListViewItem);

                    // If IsSystem or IsReserved
                    if (mailboxListViewItem.Mailbox.IsSystem
                     || mailboxListViewItem.Mailbox.IsReserved)
                        MailboxTreeDataContext.Value.Value.Insert(position + 1, new MailboxListViewItem(SelectedMailClient.AccountSettingsData, Mailbox.NewMailbox("New Mailbox", mailboxListViewItem.Mailbox.Parent), (int)mailboxListViewItem.Padding.Left, UpdateAccountUnreadEmailCount, Visibility.Collapsed));
                    else
                    {
                        // Show add mailbox dialog
                        AddMailboxDialog addMailboxDialog = new AddMailboxDialog();
                        bool resultOk = await addMailboxDialog.Show(mailboxListViewItem.Mailbox);

                        // If Ok button was clicked
                        if (resultOk)
                        {
                            if (addMailboxDialog.AddMailboxOptions == AddMailboxOptions.InsideThisFolder)
                                MailboxTreeDataContext.Value.Value.Insert(position + 1, new MailboxListViewItem(SelectedMailClient.AccountSettingsData, Mailbox.NewMailbox("New Mailbox", mailboxListViewItem.Mailbox), (int)mailboxListViewItem.Padding.Left + 20, UpdateAccountUnreadEmailCount, Visibility.Collapsed));
                            else
                                MailboxTreeDataContext.Value.Value.Insert(position + 1, new MailboxListViewItem(SelectedMailClient.AccountSettingsData, Mailbox.NewMailbox("New Mailbox", mailboxListViewItem.Mailbox.Parent), (int)mailboxListViewItem.Padding.Left, UpdateAccountUnreadEmailCount, Visibility.Collapsed));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Rename Mailbox button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void RenameMailbox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close bottom app bar
                BottomAppBar.IsOpen = false;

                MailboxListViewItem mailboxListViewItem = lvMailboxTree.SelectedItem as MailboxListViewItem;
                if (mailboxListViewItem != null)
                {
                    _isLastMailboxActionAdd = false;
                    MailboxTreeDataContext.Value.Value[MailboxTreeDataContext.Value.Value.IndexOf(mailboxListViewItem)].TextBlockVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Delete Mailbox button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void DeleteMailbox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close bottom app bar
                BottomAppBar.IsOpen = false;

                MailboxListViewItem mailboxListViewItem = lvMailboxTree.SelectedItem as MailboxListViewItem;
                if (mailboxListViewItem != null)
                {
                    MessageDialog deleteMailboxMessageDialog = new MessageDialog(string.Format("Are you sure you want to delete the '{0}' mailbox?{1}NOTE: All contents of this mailbox will be deleted as well.", mailboxListViewItem.Mailbox.Name, Environment.NewLine), "Remove Mailbox");
                    deleteMailboxMessageDialog.Commands.Add(new UICommand("Yes"));
                    deleteMailboxMessageDialog.Commands.Add(new UICommand("No"));
                    IUICommand command = await deleteMailboxMessageDialog.ShowAsync();
                    if (command.Label == "Yes")
                    {
                        await RemoveExistingMailbox(mailboxListViewItem);
                    }
                }

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Print button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ApplicationView.Value != ApplicationViewState.Snapped)
                    await PrintManager.ShowPrintUIAsync();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Mark As Flagged button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MarkAsFlagged_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected mail headers
                lvMailHeaders.Focus(FocusState.Pointer);
                IEnumerable<MailHeader> mailHeaders = lvMailHeaders.SelectedItems.Cast<MailHeader>();
                // If any are marked as not flagged
                if (mailHeaders.Where(o => !o.IsFlagged).Count() > 0)
                {
                    // Flag them
                    foreach (MailHeader mailHeader in lvMailHeaders.SelectedItems)
                        MarkMessageAsFlagged(mailHeader);

                    Flag.SetValue(AutomationProperties.NameProperty, "Remove flag");
                }
                // Else if all are marked as flagged
                else
                {
                    // Unflag them
                    foreach (MailHeader mailHeader in lvMailHeaders.SelectedItems)
                        MarkMessageAsUnflagged(mailHeader);

                    Flag.SetValue(AutomationProperties.NameProperty, "Flag");
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Junk button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void Junk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close bottom app bar
                BottomAppBar.IsOpen = false;

                // Get the mail client
                MailClient mailClient = SelectedMailClient;

                // If the mail client is available
                if (mailClient != null)
                {
                    // Get the messages to move
                    lvMailHeaders.Focus(FocusState.Pointer);
                    Dictionary<string, string> messages = lvMailHeaders.SelectedItems.Cast<MailHeader>().ToDictionary(o => o.Uid, o => o.MessagePath);

                    // Move the messages to junk mail
                    mailClient.MoveMessage(messages, SelectedMailbox, mailClient.JunkMail);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Not Junk button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void NotJunk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close bottom app bar
                BottomAppBar.IsOpen = false;

                // Get the mail client
                MailClient mailClient = SelectedMailClient;

                // If the mail client is available
                if (mailClient != null)
                {
                    // Get the messages to move
                    lvMailHeaders.Focus(FocusState.Pointer);
                    Dictionary<string, string> messages = lvMailHeaders.SelectedItems.Cast<MailHeader>().ToDictionary(o => o.Uid, o => o.MessagePath);

                    // Move the messages to junk mail
                    mailClient.MoveMessage(messages, mailClient.JunkMail, mailClient.Inbox);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Move To Folder button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MoveToFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close bottom app bar
                BottomAppBar.IsOpen = false;

                // Size the overlay grid
                gMoveToFolder.Height = Window.Current.Bounds.Height;
                gMoveToFolder.Width = Window.Current.Bounds.Width - 186;

                // Position and open the popup
                pMoveToFolder.HorizontalOffset = 186;
                pMoveToFolder.IsOpen = true;

                // Fill web view brush
                App.FillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Mark As Unread button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MarkAsUnread_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Mark each selected mail header as unread
                lvMailHeaders.Focus(FocusState.Pointer);
                foreach (MailHeader mailHeader in lvMailHeaders.SelectedItems)
                    MarkMessageAsUnread(mailHeader);

                MarkAsRead.Visibility = Visibility.Visible;
                MarkAsUnread.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Mark As Read button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void MarkAsRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Mark each selected mail header as read
                lvMailHeaders.Focus(FocusState.Pointer);
                foreach (MailHeader mailHeader in lvMailHeaders.SelectedItems)
                    MarkMessageAsRead(mailHeader);

                MarkAsRead.Visibility = Visibility.Collapsed;
                MarkAsUnread.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Delete button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bDelete_Click(bDelete, e);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        #endregion

        #region Message App Bar Button - Event Handlers

        /// <summary>
        /// Occurs when the Edit button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(NavigationContext.Edit);
        }

        /// <summary>
        /// Occurs when the New button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bNew_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(NavigationContext.New);
        }

        /// <summary>
        /// Occurs when the Reply button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bReply_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(NavigationContext.Reply);
        }

        /// <summary>
        /// Occurs when the Reply All button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bReplyAll_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(NavigationContext.ReplyToAll);
        }

        /// <summary>
        /// Occurs when the Forward button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bForward_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(NavigationContext.Forward);
        }

        /// <summary>
        /// Occurs when the Delete button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the mail headers
                lvMailHeaders.Focus(FocusState.Pointer);
                IList<object> mailHeaders = (IList<object>)lvMailHeaders.SelectedItems;
                // If available
                if (mailHeaders != null
                 && mailHeaders.Count > 0)
                {
                    // Loop through each mail header
                    foreach (MailHeader mailHeader in mailHeaders)
                    {
                        // Get the mailbox list view item
                        MailboxListViewItem mailboxListViewItem = SelectedMailboxListViewItem;
                        if (mailboxListViewItem != null)
                        {
                            // If marked as deleted or outbox message
                            if (mailHeader.IsDeleted
                             || sender is CoreWindow
                             || mailboxListViewItem.Mailbox.Folder == MailboxFolders.Outbox
                             || mailboxListViewItem.Mailbox.Folder == MailboxFolders.DeletedItems)
                            {
                                // Delete permanently
                                Dictionary<string, string> messagePath = new Dictionary<string, string>();
                                messagePath.Add(mailHeader.Uid, mailHeader.MessagePath);
                                DeleteMessages(messagePath, mailboxListViewItem.Mailbox, !(sender is CoreWindow));
                            }
                            // Else if not marked as deleted
                            else
                            {
                                // Mark as deleted
                                MarkAsDeleted(mailHeader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                try
                {
                    MessageDialog messageDialog = new MessageDialog("A problem occured while trying to delete the selected message.", "Delete Message Failed");
                    messageDialog.ShowAsync();
                }
                catch (Exception ex2)
                {
                    LogFile.Instance.LogError("", "", ex2.ToString());
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Updates the unread email count on the account with the specified email address.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="unreadEmailAccount">The unread email account.</param>
        private void UpdateAccountUnreadEmailCount(string emailAddress, int unreadEmailAccount)
        {
            try
            {
                // Update Account UnreadEmailCount
                App.RunOnUIThread(_dispatcher, () =>
                {
                    StorageSettings.AccountSettingsDataDictionary[emailAddress].UnreadEmailCount = unreadEmailAccount;
                });

                // Update badge only if necessary
                int allUnreadEmailCount = StorageSettings.MailHeaderDictionary.AllUnreadEmailCount;
                if (allUnreadEmailCount <= 100)
                    EmailBadgeNotification.UpdateBadge(allUnreadEmailCount);
            }
            catch (Exception ex2)
            {
                LogFile.Instance.LogError("", "", ex2.ToString());
            }
        }

        /// <summary>
        /// Ensures that one item is always selected in the specified list view.
        /// </summary>
        /// <param name="listView">The list view.</param>
        /// <param name="removedItems">The list view removed items.</param>
        /// <param name="selectionChangedEventHandler">The selection changed event handler.</param>
        private void EnsureItemIsSelected(ListView listView, IList<object> removedItems, SelectionChangedEventHandler selectionChangedEventHandler = null)
        {
            try
            {
                // If nothing is selected
                // Make sure the last selected item is selected
                if (listView.Items.Count > 0
                 && listView.SelectedItems.Count == 0
                 && removedItems.Count > 0)
                {
                    if (selectionChangedEventHandler != null)
                        listView.SelectionChanged -= selectionChangedEventHandler;

                    listView.SelectedItem = removedItems[0];
                    listView.ScrollIntoView(listView.SelectedItem);

                    if (selectionChangedEventHandler != null)
                        listView.SelectionChanged += selectionChangedEventHandler;
                }

                // Reset html document
                if (listView == lvMailHeaders
                 && listView.Items.Count == 0)
                    ResetHtmlDocumentState();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Binds the mailboxes.
        /// </summary>
        /// <param name="selectedAccountSettingsData">The selected account settings data.</param>
        internal void BindMailboxes(AccountSettingsData selectedAccountSettingsData)
        {
            try
            {
                // Bind mailbox
                lvMailbox.ItemsSource = null;
                lvMailbox.ItemsSource = StorageSettings.AccountSettingsDataDictionary;
                if (selectedAccountSettingsData != null)
                {
                    KeyValuePair<string, AccountSettingsData> selectedItem = StorageSettings.AccountSettingsDataDictionary.Where(o => o.Key.Equals(selectedAccountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase)).Single();
                    lvMailbox.SelectedIndex = lvMailbox.Items.Cast<KeyValuePair<string, AccountSettingsData>>().ToList().IndexOf(selectedItem);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Gets the mailbox list view items.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailboxTree">The mailbox tree.</param>
        private ObservableCollection<MailboxListViewItem> GetMailboxListViewItems(AccountSettingsData accountSettingsData, ObservableCollection<Mailbox> mailboxTree)
        {
            // Get the mailbox list view items
            return new ObservableCollection<MailboxListViewItem>(mailboxTree.Select(o => new MailboxListViewItem(accountSettingsData, o, (o.Folder == MailboxFolders.Folder && o.FullName.Contains(Mailbox.DefaultHierarchyDelimiter) ? o.FullName.Split(Mailbox.DefaultHierarchyDelimiter[0]).Length - 1 : 0) * 20, UpdateAccountUnreadEmailCount)));
        }

        /// <summary>
        /// Binds the mailbox tree.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailboxTree">The mailbox tree.</param>
        internal void BindMailboxTree(AccountSettingsData accountSettingsData, ObservableCollection<Mailbox> mailboxTree)
        {
            try
            {
                lvMailboxTree.SelectionChanged -= MailboxTree_SelectionChanged;
                // Get the selected item
                object selectedItem = lvMailboxTree.SelectedItem;
                // Stores the data context
                KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>> dataContext;
                // Get the mailbox list view items
                ObservableCollection<MailboxListViewItem> value = GetMailboxListViewItems(accountSettingsData, mailboxTree);
                // If the data context is what we are expecting
                if (MailboxTreeDataContext == null
                 || SelectedMailClient == null
                 || accountSettingsData.EmailAddress != SelectedMailClient.AccountSettingsData.EmailAddress)
                {
                    // Create the correct data context binding with data
                    dataContext = new KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>(accountSettingsData, value);
                    MailboxTreeDataContext = dataContext;
                    selectedItem = MailboxTreeDataContext.Value.Value.FirstOrDefault(o => o.Mailbox.Folder == MailboxFolders.Inbox);
                }
                // Else if the data context is not what we are expecting
                else
                {
                    // Get a reference to the data context
                    dataContext = (KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>)gMailboxTree.DataContext;
                    // Reset it's values
                    dataContext.Value.Clear();
                    foreach (MailboxListViewItem item in value)
                        dataContext.Value.Add(item);
                }
                lvMailboxTree.SelectionChanged += MailboxTree_SelectionChanged;

                // Ensure item is selected
                EnsureItemIsSelected(lvMailboxTree, new List<object>() { selectedItem ?? lvMailboxTree.Items.DefaultIfEmpty().FirstOrDefault() });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, "", ex.ToString());
            }
        }

        /// <summary>
        /// Binds the mail header list view.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="queryText">The query text.</param>
        internal void BindMailHeader(AccountSettingsData accountSettingsData, Mailbox mailbox, string queryText = null)
        {
            try
            {
                // Ensure mail header dictionary entry exists
                StorageSettings.MailHeaderDictionary.EnsureMailHeader(accountSettingsData, mailbox);
                // Bind mail header list view
                object selectedItem = lvMailHeaders.SelectedItem;
                lvMailHeaders.Visibility = Visibility.Collapsed;
                lvMailHeaders.ItemsSource = null;
                lvMailHeaders.ItemsSource = GetFilteredView(accountSettingsData, mailbox, queryText);
                lvMailHeaders.UpdateLayout();
                lvMailHeaders.Visibility = Visibility.Visible;
                // Ensure item is selected
                EnsureItemIsSelected(lvMailHeaders, new List<object>() { selectedItem ?? lvMailHeaders.Items.DefaultIfEmpty().FirstOrDefault() });
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Binds the mail header list view.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="queryText">The query text.</param>
        internal async Task BindMailHeaderAsync(AccountSettingsData accountSettingsData, Mailbox mailbox, string queryText = null)
        {
            try
            {
                await Task.Delay(1000);
                BindMailHeader(accountSettingsData, mailbox, queryText);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Gets the filtered view for the specified account and mailbox.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="queryText">The query text.</param>
        /// <returns>The sorted and filtered view.</returns>
        private ObservableCollectionView<MailHeader> GetFilteredView(AccountSettingsData accountSettingsData, Mailbox mailbox, string queryText = null)
        {
            try
            {
                // Stores the filter
                Predicate<MailHeader> filter = null;

                // If query text was supplied
                if (!string.IsNullOrWhiteSpace(queryText))
                    // Get query filter
                    filter = GetQueryFilter(queryText, false);
                // Else if query text was not supplied
                else
                    // Get email; flags filter
                    filter = GetEmailFlagsFilter();

                // Set the filter
                StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][mailbox.FullName].Filter = filter;

                // Return the view
                return StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][mailbox.FullName].View;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the email flags filter.
        /// </summary>
        /// <returns>The email flags filter.</returns>
        private Predicate<MailHeader> GetEmailFlagsFilter()
        {
            try
            {
                // Gets the email flags
                EmailFlags emailFlags = (EmailFlags)Enum.Parse(typeof(EmailFlags), cbEmailFlags.SelectedValue.ToString());

                // If Read
                if (emailFlags == EmailFlags.Read)
                    return new Predicate<MailHeader>((mailHeader) => mailHeader.IsSeen);
                // Else if unread
                else if (emailFlags == EmailFlags.Unread)
                    return new Predicate<MailHeader>((mailHeader) => !mailHeader.IsSeen);
                // Else if flagged
                else if (emailFlags == EmailFlags.Flagged)
                    return new Predicate<MailHeader>((mailHeader) => mailHeader.IsFlagged);
                // Else if high importance
                else if (emailFlags == EmailFlags.HighImportance)
                    return new Predicate<MailHeader>((mailHeader) => mailHeader.ImportanceForeground == "Red");
                // Else if low importance
                else if (emailFlags == EmailFlags.LowImportance)
                    return new Predicate<MailHeader>((mailHeader) => mailHeader.ImportanceForeground == "Blue");
                // Else if All
                else
                    return null;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the query filter.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="isSuggestion">true if is suggestion; otherwise, false.</param>
        /// <returns>The query filter predicate.</returns>
        private Predicate<MailHeader> GetQueryFilter(string queryText, bool isSuggestion)
        {
            try
            {
                // If suggestion
                if (isSuggestion)
                {
                    // Return the query filter predicate
                    return new Predicate<MailHeader>((mailHeader) =>
                    {
                        return (mailHeader.From != null && mailHeader.From.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1
                            || (!string.IsNullOrWhiteSpace(mailHeader.Subject)) && mailHeader.Subject.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1);
                    });
                }
                else
                {
                    // Return the query filter predicate
                    return new Predicate<MailHeader>((mailHeader) =>
                    {
                        bool result = false;
                        Task.Run(async () =>
                        {
                            // Get the message file
                            StorageFile messageFile = await IOUtil.FileExists(mailHeader.MessagePath);
                            if (messageFile != null)
                            {
                                // Get the message
                                StructuredMessage message = await IOUtil.LoadMessage(mailHeader.IsImapMessage, messageFile);
                                // If message is found
                                if (message != null)
                                {
                                    // Check subject and body
                                    result = (message.From != null && message.From.DisplayNameAlternate.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1)
                                          || (message.To.Any(o => o.DisplayNameAlternate.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1)
                                          || (message.CarbonCopies.Any(o => o.DisplayNameAlternate.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1)
                                          || (!string.IsNullOrWhiteSpace(message.Subject)) && message.Subject.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1
                                          || (!string.IsNullOrWhiteSpace(message.Text)) && message.Text.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1
                                          || (!string.IsNullOrWhiteSpace(message.PlainText)) && message.PlainText.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) > -1));
                                }
                            }
                        }).Wait();
                        return result;
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return new Predicate<MailHeader>((mailHeader) => { return false; });
            }
        }

        /// <summary>
        /// Ensures a temp mail header dictionary entry exists for the specified account and folder.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        private void EnsureTempMailHeader(AccountSettingsData accountSettingsData, Mailbox mailbox)
        {
            try
            {
                // Make sure the account exists
                if (!_tempMailHeaderDictionary.ContainsKey(accountSettingsData.EmailAddress))
                    _tempMailHeaderDictionary.TryAdd(accountSettingsData.EmailAddress, new ConcurrentDictionary<string, MailHeaderObservableCollectionView>(StringComparer.OrdinalIgnoreCase));

                // Make sure the folder for the account exists
                if (!_tempMailHeaderDictionary[accountSettingsData.EmailAddress].ContainsKey(mailbox.FullName))
                    _tempMailHeaderDictionary[accountSettingsData.EmailAddress].TryAdd(mailbox.FullName, new MailHeaderObservableCollectionView());
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.Name, ex.ToString());
            }
        }

        /// <summary>
        /// Adds a mail header for the specified mail client.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message.</param>
        private void AddMailHeader(AccountSettingsData accountSettingsData, Mailbox mailbox, StructuredMessage message)
        {
            try
            {
                // Ensure the mail header dictionary entry exists
                StorageSettings.MailHeaderDictionary.EnsureMailHeader(accountSettingsData, mailbox);

                // Create the mail header
                MailHeader mailHeader = new MailHeader(accountSettingsData.EmailAddress, message.Uid, message, mailbox);

                // If the mail header exists
                if (StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][mailbox.FullName].Contains(mailHeader))
                {
                    // Update it
                    StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][mailbox.FullName][StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][mailbox.FullName].IndexOf(mailHeader)] = mailHeader;
                }
                // Else if it doesn't exist
                else
                {
                    // Add the header to the accounts folder
                    StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][mailbox.FullName].Add(mailHeader);

                    // Ensure this mail header is selected if nothing is
                    if (lvMailHeaders.Items.Count > 0
                     && lvMailHeaders.SelectedItems.Count == 0)
                    {
                        MailboxListViewItem mailboxListViewItem = (MailboxListViewItem)lvMailboxTree.SelectedItem;
                        if (mailboxListViewItem != null
                         && mailboxListViewItem.Mailbox == mailbox)
                        {
                            lvMailHeaders.SelectedItem = mailHeader;
                            ResetEmailHeaderProgressRingState(MailClientsDictionary[accountSettingsData.EmailAddress], mailbox, false);
                        }
                    }
                }

                // Get from, subject, body and context
                string from = (message.From == null ? "(Unknown)" : message.From.DisplayNameAlternate);
                string subject = message.Subject;
                string body = message.PlainText;

                // Only notify todays messages
                if (message.Date >= DateTime.Today
                 && StorageSettings.IsUISuspended
                 && mailbox.Folder != MailboxFolders.Drafts)
                {
                    string context = null;
                    try
                    {
                        // Create the toast context
                        context = IOUtil.Serialise(new EmailToastNotificationContext(accountSettingsData.EmailAddress, mailbox.FullName, message.Uid));

                        // Show email notification
                        EmailToastNotification.Show(from, subject, body, context);
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError("", "", ex.ToString());
                    }
                }

                // If this is an unread email
                if (message.Date >= DateTime.Today
                && !mailHeader.IsSeen
                && mailbox.Folder != MailboxFolders.Drafts)
                {
                    // Update tile
                    EmailTileNotification.UpdateTile(mailHeader.Uid, from, subject, body);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Adds a temp mail header for the specified mail client.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message.</param>
        private void AddTempMailHeader(AccountSettingsData accountSettingsData, Mailbox mailbox, StructuredMessage message)
        {
            try
            {
                // Ensure the mail header dictionary entry exists
                EnsureTempMailHeader(accountSettingsData, mailbox);

                // Add the header to the accounts folder
                _tempMailHeaderDictionary[accountSettingsData.EmailAddress][mailbox.FullName].Add(new MailHeader(accountSettingsData.EmailAddress, message.Uid, message, mailbox));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Processes the mailbox textbox content.
        /// </summary>
        /// <param name="selectedMailClient">The selected mail client.</param>
        /// <param name="mailboxTextBox">The mailbox text.</param>
        private async Task ProcessMailboxTextBox(MailClient selectedMailClient, TextBox mailboxTextBox)
        {
            try
            {
                // If no text was input
                if (string.IsNullOrEmpty(mailboxTextBox.Text.Trim()))
                    mailboxTextBox.Focus(FocusState.Programmatic);
                // Else if text was input
                else
                {
                    // Get the mailbox list view item
                    MailboxListViewItem mailboxListViewItem = mailboxTextBox.DataContext as MailboxListViewItem;
                    if (mailboxListViewItem != null)
                    {
                        // If the mailbox changed
                        if (mailboxListViewItem.Mailbox.DisplayName != mailboxTextBox.Text.Trim())
                        {
                            string newFullname = (mailboxListViewItem.Mailbox.FullName.EndsWith(mailboxListViewItem.Mailbox.DisplayName, StringComparison.OrdinalIgnoreCase) ? mailboxListViewItem.Mailbox.FullName.Substring(0, mailboxListViewItem.Mailbox.FullName.IndexOf(mailboxListViewItem.Mailbox.DisplayName, StringComparison.OrdinalIgnoreCase)) + mailboxTextBox.Text : null);
                            if (!string.IsNullOrEmpty(newFullname)
                             && MailboxTreeDataContext.Value.Value.FirstOrDefault(o => o.Mailbox.FullName.Equals(newFullname, StringComparison.OrdinalIgnoreCase)) != null)
                            {
                                try
                                {
                                    MessageDialog exists = new MessageDialog(string.Format("A mailbox with the name '{0}' already exists.", mailboxTextBox.Text), "Duplicate Mailbox");
                                    await exists.ShowAsync();
                                }
                                catch (Exception ex)
                                {
                                    LogFile.Instance.LogError(selectedMailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
                                }
                            }
                            else
                            {
                                // If Add
                                if (_isLastMailboxActionAdd)
                                {
                                    mailboxListViewItem.Mailbox.DisplayName = mailboxTextBox.Text.Trim();
                                    await AddNewMailbox(mailboxListViewItem);
                                }
                                // Else if Rename
                                else
                                {
                                    Mailbox oldMailbox = mailboxListViewItem.Mailbox.Copy();
                                    mailboxListViewItem.Mailbox.DisplayName = mailboxTextBox.Text.Trim();
                                    await RenameExistingMailbox(oldMailbox, mailboxListViewItem);
                                }
                                mailboxListViewItem.TextBlockVisibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(selectedMailClient.AccountSettingsData.EmailAddress, null, ex.ToString());
            }
        }

        /// <summary>
        /// Marks a message as read.
        /// </summary>
        /// <param name="mailHeader">The mail header.</param>
        private void MarkMessageAsRead(MailHeader mailHeader)
        {
            Task.Run(async() =>
            {
                // Make sure the mail client exists
                while (!MailClientsDictionary.ContainsKey(mailHeader.EmailAddress))
                    await Task.Delay(1000);

                // Wait 3 seconds to ensure message is read
                await Task.Delay(3000);

                try
                {
                    // If the message exists
                    StorageFile storageFile = null;
                    if ((storageFile = await IOUtil.FileExists(mailHeader.MessagePath)) != null)
                    {
                        // Get the message
                        StructuredMessage message = await MailClientsDictionary[mailHeader.EmailAddress].LoadMessage(storageFile);

                        // Mark message as read on server
                        if (!mailHeader.IsSeen)
                        {
                            MailClientsDictionary[mailHeader.EmailAddress].MarkAsRead(message, mailHeader.Mailbox);
                            EmailTileNotification.RemoveTile(message.Uid);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(mailHeader.EmailAddress, mailHeader.Mailbox.FullName, ex.ToString());
                }
            });
        }

        /// <summary>
        /// Marks a message as unread.
        /// </summary>
        /// <param name="mailHeader">The mail header.</param>
        private async void MarkMessageAsUnread(MailHeader mailHeader)
        {
            try
            {
                // If the message exists
                StorageFile storageFile = null;
                if ((storageFile = await IOUtil.FileExists(mailHeader.MessagePath)) != null)
                {
                    // Get the message
                    StructuredMessage message = await MailClientsDictionary[mailHeader.EmailAddress].LoadMessage(storageFile);

                    // Mark message as unread on server
                    if (mailHeader.IsSeen)
                        MailClientsDictionary[mailHeader.EmailAddress].MarkAsUnread(message, mailHeader.Mailbox);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailHeader.EmailAddress, mailHeader.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Marks a message as deleted.
        /// </summary>
        /// <param name="mailHeader">The mail header.</param>
        private async void MarkAsDeleted(MailHeader mailHeader)
        {
            try
            {
                // If the message exists
                StorageFile storageFile = null;
                if ((storageFile = await IOUtil.FileExists(mailHeader.MessagePath)) != null)
                {
                    // Get the message
                    StructuredMessage message = await MailClientsDictionary[mailHeader.EmailAddress].LoadMessage(storageFile);

                    // Mark message as unread on server
                    if (!mailHeader.IsDeleted)
                        MailClientsDictionary[mailHeader.EmailAddress].MarkAsDeleted(message, mailHeader.Mailbox);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailHeader.EmailAddress, mailHeader.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Marks a message as undeleted.
        /// </summary>
        /// <param name="mailHeader">The mail header.</param>
        private async void MarkAsUndeleted(MailHeader mailHeader)
        {
            try
            {
                // If the message exists
                StorageFile storageFile = null;
                if ((storageFile = await IOUtil.FileExists(mailHeader.MessagePath)) != null)
                {
                    // Get the message
                    StructuredMessage message = await MailClientsDictionary[mailHeader.EmailAddress].LoadMessage(storageFile);

                    // Mark message as unread on server
                    if (mailHeader.IsDeleted)
                        MailClientsDictionary[mailHeader.EmailAddress].MarkAsUndeleted(message, mailHeader.Mailbox);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailHeader.EmailAddress, mailHeader.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Marks a message as flagged.
        /// </summary>
        /// <param name="mailHeader">The mail header.</param>
        private async void MarkMessageAsFlagged(MailHeader mailHeader)
        {
            try
            {
                // If the message exists
                StorageFile storageFile = null;
                if ((storageFile = await IOUtil.FileExists(mailHeader.MessagePath)) != null)
                {
                    // Get the message
                    StructuredMessage message = await MailClientsDictionary[mailHeader.EmailAddress].LoadMessage(storageFile);

                    // Mark message as flagged on server
                    if (!mailHeader.IsFlagged)
                        MailClientsDictionary[mailHeader.EmailAddress].MarkAsFlagged(message, mailHeader.Mailbox);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailHeader.EmailAddress, mailHeader.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Marks a message as unflagged.
        /// </summary>
        /// <param name="mailHeader">The mail header.</param>
        private async void MarkMessageAsUnflagged(MailHeader mailHeader)
        {
            try
            {
                // If the message exists
                StorageFile storageFile = null;
                if ((storageFile = await IOUtil.FileExists(mailHeader.MessagePath)) != null)
                {
                    // Get the message
                    StructuredMessage message = await MailClientsDictionary[mailHeader.EmailAddress].LoadMessage(storageFile);

                    // Mark message as flagged on server
                    if (mailHeader.IsFlagged)
                        MailClientsDictionary[mailHeader.EmailAddress].MarkAsUnflagged(message, mailHeader.Mailbox);
                }

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailHeader.EmailAddress, mailHeader.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a message.
        /// </summary>
        /// <param name="messagePaths">The message paths.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="raiseOnDeletingMessage">true, if DeletingMessage event should be raised; otherwise, false.</param>
        private void DeleteMessages(Dictionary<string, string> messagePaths, Mailbox mailbox, bool raiseOnDeletingMessage = true)
        {
            // If we find the associated mail client
            MailClient mailClient = SelectedMailClient;
            if (mailClient != null)
            {
                try
                {
                    // Mark message as unread on server
                    MailClientsDictionary[mailClient.AccountSettingsData.EmailAddress].DeleteMessage(messagePaths, mailbox, raiseOnDeletingMessage);
                    foreach (string uid in messagePaths.Keys)
                        EmailTileNotification.RemoveTile(uid);

                    // Update account unread email count
                    if (StorageSettings.MailHeaderDictionary.ContainsKey(SelectedAccount.Key))
                        UpdateAccountUnreadEmailCount(SelectedAccount.Key, StorageSettings.MailHeaderDictionary.GetAccountUnreadEmailCount(SelectedAccount.Key));

                    // Update account unread email count - mailbox list view item
                    MailboxTreeDataContext.Value.Value.Single(o => o.Mailbox.FullName.Equals(mailbox.FullName, StringComparison.OrdinalIgnoreCase)).MailHeaderDictionaryCollectionChanged(this, null);
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Moves the list of mail headers to the specified mailbox.
        /// </summary>
        /// <param name="mailHeaders">The list of mail headers to move.</param>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        private async Task MoveMailHeaders(IEnumerable<MailHeader> mailHeaders, AccountSettingsData accountSettingsData, Mailbox sourceMailbox, Mailbox destinationMailbox)
        {
            try
            {
                // Stores a value indicating if a reset of the selected mail header is required
                bool resetSelectedMailHeader = false;
                int mailHeaderIndex = -1;

                // Loop through each mail header
                foreach (MailHeader mailHeader in mailHeaders.ToList())
                {
                    // Determine if reset is required
                    resetSelectedMailHeader = (lvMailHeaders.SelectedItem == mailHeader);
                    if (resetSelectedMailHeader)
                        mailHeaderIndex = StorageSettings.MailHeaderDictionary[SelectedMailClient.AccountSettingsData.EmailAddress][sourceMailbox.FullName].IndexOf(mailHeader);

                    // Ensure the mail header dictionary entry exists
                    StorageSettings.MailHeaderDictionary.EnsureMailHeader(accountSettingsData, destinationMailbox);

                    // Add the header to the destination folder
                    StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][destinationMailbox.FullName].Add(mailHeader);

                    // Remove the header from the source folder
                    StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][sourceMailbox.FullName].Remove(mailHeader);

                    // Reset the mailbox
                    mailHeader.Mailbox = destinationMailbox;

                    // Update account unread email count for source mailbox
                    MailboxListViewItem mailboxListViewItem = MailboxTreeDataContext.Value.Value.FirstOrDefault(o => o.Mailbox.FullName.Equals(sourceMailbox.FullName, StringComparison.OrdinalIgnoreCase));
                    if (mailboxListViewItem != null)
                        mailboxListViewItem.MailHeaderDictionaryCollectionChanged(this, null);

                    // Update account unread email count for destination mailbox
                    mailboxListViewItem = MailboxTreeDataContext.Value.Value.FirstOrDefault(o => o.Mailbox.FullName.Equals(destinationMailbox.FullName, StringComparison.OrdinalIgnoreCase));
                    if (mailboxListViewItem != null)
                        mailboxListViewItem.MailHeaderDictionaryCollectionChanged(this, null);

                    // Wait for animations to render fully
                    await Task.Delay(10);
                }

                // If reset is required
                if (resetSelectedMailHeader)
                {
                    // Default to first item if available
                    if (StorageSettings.MailHeaderDictionary[accountSettingsData.EmailAddress][sourceMailbox.FullName].Count > 0)
                    {
                        EnsureItemIsSelected(lvMailHeaders, new object[] { GetNextMailHeader(SelectedMailClient, sourceMailbox, mailHeaderIndex) });
                    }
                    // Else reset to nothing
                    else
                        lvMailHeaders.SelectedItem = -1;
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, destinationMailbox.FullName, "MoveMailHeaders: " + ex.ToString());
            }
        }

        /// <summary>
        /// Adds the mailbox.
        /// </summary>
        /// <param name="mailboxListViewItem">The mailbox list view item.</param>
        private async Task AddNewMailbox(MailboxListViewItem mailboxListViewItem)
        {
            // If we find the associated mail client
            MailClient mailClient = SelectedMailClient;
            if (mailClient != null)
            {
                try
                {
                    // Set the data
                    mailboxListViewItem.TextBlockVisibility = Visibility.Visible;
                    MailboxTreeDataContext.Value.Value[MailboxTreeDataContext.Value.Value.IndexOf(mailboxListViewItem)] = mailboxListViewItem;
                    // Create the mailbox
                    await mailClient.AddMailbox(mailboxListViewItem.Mailbox);
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, mailboxListViewItem.Mailbox.FullName, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Renames the mailbox.
        /// </summary>
        /// <param name="oldMailboxListViewItem">The old mailbox.</param>
        /// <param name="mailboxListViewItem">The mailbox list view item.</param>
        private async Task RenameExistingMailbox(Mailbox oldMailbox, MailboxListViewItem mailboxListViewItem)
        {
            // If we find the associated mail client
            MailClient mailClient = SelectedMailClient;
            if (mailClient != null)
            {
                try
                {
                    // Set the data
                    mailboxListViewItem.TextBlockVisibility = Visibility.Visible;
                    await mailClient.RenameMailbox(oldMailbox, mailboxListViewItem.Mailbox);
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, mailboxListViewItem.Mailbox.FullName, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Removes the mailbox.
        /// </summary>
        /// <param name="mailboxListViewItem">The mailbox list view item.</param>
        private async Task RemoveExistingMailbox(MailboxListViewItem mailboxListViewItem)
        {
            try
            {
                // If we have a mailbox list view item
                if (mailboxListViewItem != null)
                {
                    // If we find the associated mail client
                    MailClient mailClient = SelectedMailClient;
                    if (mailClient != null)
                    {
                        // Update the mailbox tree
                        foreach (Mailbox mailbox in mailClient.MailboxTree
                                .Where(o => o.FullName.Equals(mailboxListViewItem.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                                         || o.FullName.StartsWith(mailboxListViewItem.Mailbox.FullName + Mailbox.DefaultHierarchyDelimiter, StringComparison.OrdinalIgnoreCase))
                                .ToList()
                                .OrderByDescending(o => o.FullName.Length)
                                .OrderBy(o => o.Name))
                        {
                            await mailClient.RemoveMailbox(mailbox);
                            lvMailboxTree.SelectedItem = MailboxTreeDataContext.Value.Value.First();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", mailboxListViewItem.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Gets the next mail header for the specified mail client, mailbox and in relation to the specified header.
        /// </summary>
        /// <param name="mailClient">The mail client.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="mailHeaderIndex">The mail header index.</param>
        /// <returns>The next mail header.</returns>
        private MailHeader GetNextMailHeader(MailClient mailClient, Mailbox mailbox, int mailHeaderIndex)
        {
            try
            {
                // Get the next mail header index
                int nextMailHeaderIndex = mailHeaderIndex;
                // If the index is out of bounds
                if (nextMailHeaderIndex > lvMailHeaders.Items.Count - 1)
                {
                    // Try to get the previous mail header
                    nextMailHeaderIndex = mailHeaderIndex - 1;
                    // If the previous mail header is out of bounds - return null
                    if (nextMailHeaderIndex < 0)
                        return null;
                    // Else return the previous mail header
                    else
                        return (MailHeader)lvMailHeaders.Items[nextMailHeaderIndex];
                }
                else
                    return (MailHeader)lvMailHeaders.Items[nextMailHeaderIndex];
            }
            catch
            {
                return (lvMailHeaders.Items.Count > 0 ? (MailHeader)lvMailHeaders.Items[0] : null);
            }
        }

        /// <summary>
        /// Shows or hides the filters.
        /// </summary>
        /// <param name="visibility">The filter visibility.</param>
        private void ShowHideFilters(Visibility visibility)
        {
            cbEmailFlags.Visibility = visibility;
            bSearch.Visibility = visibility;
        }

        /// <summary>
        /// Shows or hides the bottom app bar buttons.
        /// </summary>
        /// <param name="leftPanelVisibility">The left panel visibility.</param>
        /// <param name="mailboxPanelVisibility">The mailbox panel visibility.</param>
        /// <param name="mailboxPanelVisibilityAction">The mailbox panel visibility action.</param>
        /// <param name="messagePanelVisibility">The message panel visibility.</param>
        /// <param name="messagePanelVisibilityAction">The message panel visibility action.</param>
        private void ShowHideBottomAppBarButtons(Visibility leftPanelVisibility, Visibility mailboxPanelVisibility, Action mailboxPanelVisibilityAction, Visibility messagePanelVisibility, Action messagePanelVisibilityAction)
        {
            try
            {
                // Set the visibility properties of the panels
                LeftPanel.Visibility = leftPanelVisibility;
                MailboxPanel.Visibility = mailboxPanelVisibility;
                MessagePanel.Visibility = messagePanelVisibility;
                FolderOptions.Visibility = (MailboxPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);

                // If mailbox panel is visible
                if (mailboxPanelVisibility == Visibility.Visible)
                    mailboxPanelVisibilityAction();

                // If message panel is visible
                if (messagePanelVisibility == Visibility.Visible)
                    messagePanelVisibilityAction();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Shows or hides the email app bar buttons.
        /// </summary>
        /// <param name="emailPanelVisibility">The email panel visibility.</param>
        /// <param name="message">The message.</param>
        private void ShowHideMailAppButtons(Visibility emailPanelVisibility, StructuredMessage message)
        {
            try
            {
                // Set the visibility property
                spEmailPanel.Visibility = emailPanelVisibility;

                // If email panel is visible
                if (emailPanelVisibility == Visibility.Visible)
                {
                    // If no message supplied - hide everything
                    if (message == null)
                    {
                        bEdit.Visibility = Visibility.Collapsed;
                        bReply.Visibility = Visibility.Collapsed;
                        bReplyAll.Visibility = Visibility.Collapsed;
                        bForward.Visibility = Visibility.Collapsed;
                        bDelete.Visibility = Visibility.Collapsed;
                        tbTo.Visibility = Visibility.Collapsed;
                        tbCc.Visibility = Visibility.Collapsed;
                        tbNoRecipients.Visibility = Visibility.Collapsed;
                    }
                    // Else if message supplied
                    else
                    {
                        // If no recipients
                        if (message.To.Count + message.CarbonCopies.Count == 0)
                        {
                            tbTo.Visibility = Visibility.Collapsed;
                            tbCc.Visibility = Visibility.Collapsed;
                            tbNoRecipients.Visibility = Visibility.Visible;
                        }
                        // Else if there are recipients
                        else
                        {
                            tbTo.Visibility = (message.To.Count > 0 ? Visibility.Visible : Visibility.Collapsed);
                            tbCc.Visibility = (message.CarbonCopies.Count > 0 ? Visibility.Visible : Visibility.Collapsed);
                            tbNoRecipients.Visibility = Visibility.Collapsed;
                        }

                        // If drafts folder
                        if (SelectedMailbox != null
                         && SelectedMailbox.Folder == MailboxFolders.Drafts)
                        {
                            bEdit.Visibility = Visibility.Visible;
                            bReply.Visibility = Visibility.Collapsed;
                            bReplyAll.Visibility = Visibility.Collapsed;
                            bForward.Visibility = Visibility.Collapsed;
                            bDelete.Visibility = Visibility.Visible;
                        }
                        // If drafts folder
                        else if (SelectedMailbox != null
                         && SelectedMailbox.Folder == MailboxFolders.Outbox)
                        {
                            bEdit.Visibility = Visibility.Collapsed;
                            bReply.Visibility = Visibility.Collapsed;
                            bReplyAll.Visibility = Visibility.Collapsed;
                            bForward.Visibility = Visibility.Collapsed;
                            bDelete.Visibility = Visibility.Visible;
                        }
                        // Else if any other folder
                        else
                        {
                            bEdit.Visibility = Visibility.Collapsed;
                            bReply.Visibility = Visibility.Visible;
                            bReplyAll.Visibility = (message.To.Count > 1 || message.CarbonCopies.Count > 1 ? Visibility.Visible : Visibility.Collapsed);
                            bForward.Visibility = Visibility.Visible;
                            bDelete.Visibility = Visibility.Visible;
                        }
                    }
                }
                // Else if email panel is collapsed
                else
                {
                    tbTo.Visibility = Visibility.Collapsed;
                    tbCc.Visibility = Visibility.Collapsed;
                    tbNoRecipients.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Shows or hides the mailbox app bar buttons.
        /// </summary>
        /// <param name="mailboxListViewItem">The mailbox list view item.</param>
        private void ShowHideMailboxAppBarButtons(MailboxListViewItem mailboxListViewItem)
        {
            try
            {
                // If a mailbox list view item is selected
                if (mailboxListViewItem != null)
                {
                    DeleteMailbox.Visibility = (mailboxListViewItem.Mailbox.IsSystem || mailboxListViewItem.Mailbox.IsReserved ? Visibility.Collapsed : Visibility.Visible);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Enables or disables drag and drop functionality depending on selected mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        private void EnabledDisableDragAndDrop(Mailbox mailbox)
        {
            // If Drafts or Outbox - disable drag and drop
            if (mailbox.Folder == MailboxFolders.Drafts
             || mailbox.Folder == MailboxFolders.Outbox)
            {
                lvMailboxTree.CanDragItems = false;
                lvMailboxTree.CanReorderItems = false;
            }
            // Else if anything else
            else
            {
                lvMailboxTree.CanDragItems = true;
                lvMailboxTree.CanReorderItems = true;
            }
        }

        /// <summary>
        /// Resets the application.
        /// </summary>
        internal void ResetApplication()
        {
            // Reset the application
            BottomAppBar.IsOpen = false;
            gMailboxTree.DataContext = null;
            lvMailHeaders.ItemsSource = null;
            prRestoreProgress.IsActive = false;
            ResetHtmlDocumentState();
        }

        /// <summary>
        /// Resets the html document state.
        /// </summary>
        internal void ResetHtmlDocumentState()
        {
            try
            {
                emailBodyLayout.DataContext = null;
                emailBodyText.NavigateToString("");
                alvAttachments.ItemsSource = null;
                ShowHideMailAppButtons((MailClientsDictionary.Count > 0 ? Visibility.Visible : Visibility.Collapsed), null);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Resets the email headers progress ring state.
        /// </summary>
        /// <param name="mailClient">The mail client.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="isActive">true if is active; otherwise, false.</param>
        internal void ResetEmailHeaderProgressRingState(MailClient mailClient, Mailbox mailbox, bool isActive)
        {
            try
            {
                prRestoreProgress.IsActive = isActive;
                prRestoreProgress.Visibility = (isActive ? Visibility.Visible : Visibility.Collapsed);
                lvMailHeaders.Visibility = (isActive ? Visibility.Collapsed : Visibility.Visible);
                if (isActive) ResetHtmlDocumentState();

                // Determine progress state
                switch (isActive)
                {
                    // If active
                    case true:
                        tbNoMessages.Visibility = Visibility.Collapsed;
                        break;

                    // If not active
                    default:
                        // If completed and no messages
                        if (mailClient.MailboxDownloadStatus.ContainsKey(mailbox.FullName)
                         && mailClient.MailboxDownloadStatus[mailbox.FullName] == MailboxActionState.Completed
                         && StorageSettings.MailHeaderDictionary.ContainsKey(mailClient.AccountSettingsData.EmailAddress)
                         && StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress].ContainsKey(mailbox.FullName)
                         && StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][mailbox.FullName].Count == 0)
                        {
                            tbNoMessages.Visibility = Visibility.Visible;
                            // Determine which folder we are in
                            switch (mailbox.IsSystem)
                            {
                                // If system mailbox
                                case true:
                                    tbNoMessages.Text = "System mailboxes do not contain messages.";
                                    break;

                                // Else if not a system mailbox
                                default:
                                    // Determine what folder this is
                                    switch (mailbox.Folder)
                                    {
                                        // If Outbox
                                        case MailboxFolders.Outbox:
                                            tbNoMessages.Text = "No messages waiting to be sent.";
                                            break;

                                        // Anything else
                                        default:
                                            // Determine the DownloadEmailsFrom set
                                            switch (mailClient.AccountSettingsData.DownloadEmailsFrom)
                                            {
                                                // If Anytime
                                                case DownloadEmailsFromOptions.Anytime:
                                                    tbNoMessages.Text = "No messages.";
                                                    break;

                                                // Anything else
                                                default:
                                                    tbNoMessages.Text = string.Format("No messages for {0}.", mailClient.AccountSettingsData.DownloadEmailsFrom.ToString().ToWords().ToLower());
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                        // Else if busy or are there messages
                        else
                            tbNoMessages.Visibility = Visibility.Collapsed;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Resets the email header filter state.
        /// </summary>
        /// <param name="isFiltered">true, if is filtered state; otherwise false.</param>
        /// <param name="queryText">The query text.</param>
        internal void ResetEmailHeaderFilterState(bool isFiltered, string queryText)
        {
            try
            {
                gSearchResult.Visibility = (isFiltered ? Visibility.Visible : Visibility.Collapsed);
                gFilter.Visibility = (isFiltered ? Visibility.Collapsed : Visibility.Visible);
                gTitle.ColumnDefinitions[0].Width = new GridLength((isFiltered ? 0 : 110));
                tbQueryText.Text = queryText;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Resets the message progress state.
        /// </summary>
        /// <param name="mailClient">The mail client.</param>
        /// <param name="messageProgressEventArgs">The message progress event arguments.</param>
        private void ResetRestoreMessageProgressState(MailClient mailClient, MessageProgressEventArgs messageProgressEventArgs)
        {
            try
            {
                // Get the selected mail client
                MailClient selectedMailClient = SelectedMailClient;

                // Get the selected mailbox folder
                MailboxListViewItem mailboxListViewItem = SelectedMailboxListViewItem;

                // Show download progress for the currently selected mailbox
                if (selectedMailClient != null
                 && mailboxListViewItem != null
                 && selectedMailClient.AccountSettingsData.EmailAddress == mailClient.AccountSettingsData.EmailAddress
                 && mailboxListViewItem.Mailbox.FullName.Equals(messageProgressEventArgs.Mailbox.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    pbDownloadProgress.Maximum = messageProgressEventArgs.TotalMessageSize;
                    downloadProgressMessageTotal.Text = string.Format("{0} Messages", messageProgressEventArgs.TotalMessageCount);
                    downloadProgressMessageTotalSize.Text = messageProgressEventArgs.TotalMessageSizeFormat;
                    pbDownloadProgress.Value = messageProgressEventArgs.RemainingMessageSize;
                    downloadProgressMessageNumber.Text = string.Format("{0} Remaining", messageProgressEventArgs.RemainingMessageCount);
                    downloadProgressMessageNumberSize.Text = string.Format("{0} Remaining", messageProgressEventArgs.RemainingMessageSizeFormat);
                }
                // Else if reset
                else if (mailClient == null
                      && messageProgressEventArgs == null)
                {
                    pbDownloadProgress.Maximum = 0;
                    downloadProgressMessageTotal.Text = string.Format("{0} Messages", 0);
                    downloadProgressMessageTotalSize.Text = "0";
                    pbDownloadProgress.Value = 0;
                    downloadProgressMessageNumber.Text = string.Format("{0} Remaining", 0);
                    downloadProgressMessageNumberSize.Text = string.Format("{0} Remaining", 0);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Reset the restore message progress visibility.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        private void ResetRestoreMessageProgressVisibility(MailClient mailClient, Mailbox mailbox)
        {
            try
            {
                // If null or completed - hide
                if (mailClient.MailboxRestoreStatus[mailbox.FullName] == MailboxActionState.Completed)
                    gMailHeaders.RowDefinitions[1].Height = new GridLength(0);
                // Else show
                else
                    gMailHeaders.RowDefinitions[1].Height = new GridLength(50);

                // Reset email header progress ring state
                ResetEmailHeaderProgressRingState(mailClient, mailbox, mailClient.MailboxRestoreStatus[mailbox.FullName] == MailboxActionState.Busy || mailClient.MailboxDownloadStatus[mailbox.FullName] == MailboxActionState.Busy);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
            }
        }

        ///// <summary>
        ///// Provide print content for scenario 2 first page
        ///// </summary>
        //protected override void PreparePrintContent()
        //{
        //    try
        //    {
        //        if (firstPage == null)
        //        {
        //            firstPage = new PrintPage();
        //            StackPanel header = (StackPanel)firstPage.FindName("header");
        //            header.Visibility = Windows.UI.Xaml.Visibility.Visible;
        //        }

        //        // Add the (newley created) page to the printing root which is part of the visual tree and force it to go
        //        // through layout so that the linked containers correctly distribute the content inside them.
        //        PrintingRoot.Children.Add(firstPage);
        //        PrintingRoot.InvalidateMeasure();
        //        PrintingRoot.UpdateLayout();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile.Instance.LogError("", "", ex.ToString());
        //    }
        //}

        /// <summary>
        /// Removes an account.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        internal async Task RemoveAccount(AccountSettingsData accountSettingsData)
        {
            try
            {
                // Remove the account
                MailClient mailClient;
                MailClientsDictionary.TryRemove(accountSettingsData.EmailAddress, out mailClient);
                StorageSettings.AccountSettingsDataDictionary.Remove(accountSettingsData.EmailAddress);
                await StorageSettings.SaveAccountSettingsDataDictionary();
                ConcurrentDictionary<string, MailHeaderObservableCollectionView> mailHeaderDictionaryEntry;
                StorageSettings.MailHeaderDictionary.TryRemove(accountSettingsData.EmailAddress, out mailHeaderDictionaryEntry);

                // Update the tiles
                EmailTileNotification.RemoveTiles(mailHeaderDictionaryEntry.SelectMany(o => o.Value).Select(o => o.Uid));

                // Databind the mailboxes
                BindMailboxes(MailboxTreeDataContext.Value.Key);

                // If current mailbox tree context is for this account
                if (MailboxTreeDataContext.HasValue
                 && MailboxTreeDataContext.Value.Key.EmailAddress.Equals(accountSettingsData.EmailAddress, StringComparison.OrdinalIgnoreCase))
                {
                    // If there are no accounts
                    if (StorageSettings.AccountSettingsDataDictionary.Count == 0)
                    {
                        // Reset the application
                        ResetApplication();
                        //ResetRestoreMessageProgressState(null, null);
                    }
                    // If there's atleast 1 account
                    else
                    {
                        // Bind it
                        BindMailboxTree(StorageSettings.AccountSettingsDataDictionary.First().Value, MailClientsDictionary.First().Value.MailboxTree);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(accountSettingsData.EmailAddress, "", ex.ToString());
            }
        }

        /// <summary>
        /// Sets the delete button glyph.
        /// </summary>
        private void SetDeleteButtonGlyph()
        {
            if (SelectedMailbox != null && SelectedMailbox.Folder == MailboxFolders.DeletedItems)
            {
                Delete.Content = "";
                bDelete.Content = "";
            }
            else
            {
                Delete.Content = "";
                bDelete.Content = "";
            }
        }

        /// <summary>
        /// Gets a message size in terms of bytes from the specified long size.
        /// </summary>
        /// <param name="size">The long size.</param>
        /// <returns>The message size.</returns>
        private string GetMessageSize(double size)
        {
            try
            {
                if (size >= 0 && size < 1024)
                    return string.Format("{0} Bytes", size.ToString("N2"));
                else if (size >= 1024 && size < 1048576)
                    return string.Format("{0} KB", (size / 1024).ToString("N2"));
                else if (size >= 1048576 && size < 1073741824)
                    return string.Format("{0} MB", (size / 1048576).ToString("N2"));
                else
                    return string.Format("{0} MB", (size / 1073741824).ToString("N2"));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return string.Format("{0} Bytes", size.ToString("N2"));
            }
        }

        /// <summary>
        /// Navigates to the ComposeMailPage witht he specified context.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        internal void NavigateTo(NavigationContext navigationContext)
        {
            try
            {
                // Stores the message
                StructuredMessage message = null;

                // If Edit
                if (navigationContext == NavigationContext.Edit)
                    message = emailBodyLayout.DataContext as StructuredMessage;
                // Anything else
                else
                {
                    // Create a new message
                    message = (SelectedMessage is PopMessage ? (StructuredMessage)new PopMessage() : (StructuredMessage)new ImapMessage());
                    message.TextContentType = ETextContentType.Html;

                    // Make sure a message is selected
                    if (navigationContext != NavigationContext.New
                     && SelectedMessage != null)
                    {
                        // Determine the navigation context
                        switch (navigationContext)
                        {
                            // If Edit
                            case NavigationContext.Edit:
                                message.To.AddRange(SelectedMessage.To);
                                message.CarbonCopies.AddRange(SelectedMessage.CarbonCopies);
                                message.Subject = SelectedMessage.Subject;
                                message.Attachments.AddRange(SelectedMessage.Attachments);
                                break;

                            // If Reply
                            case NavigationContext.Reply:
                                message.To.Add(SelectedMessage.From);
                                message.Subject = "Re: " + SelectedMessage.Subject;
                                break;

                            // If ReplyToAll
                            case NavigationContext.ReplyToAll:
                                message.To.AddRange(SelectedMessage.To.Where(o => o.Address != SelectedMailClient.AccountSettingsData.EmailAddress));
                                message.CarbonCopies.AddRange(SelectedMessage.CarbonCopies.Where(o => o.Address != SelectedMailClient.AccountSettingsData.EmailAddress));
                                message.Subject = "Re: " + SelectedMessage.Subject;
                                break;

                            // If Forward
                            case NavigationContext.Forward:
                                message.To.AddRange(SelectedMessage.To.Where(o => o.Address != SelectedMailClient.AccountSettingsData.EmailAddress));
                                message.CarbonCopies.AddRange(SelectedMessage.CarbonCopies.Where(o => o.Address != SelectedMailClient.AccountSettingsData.EmailAddress));
                                message.Subject = "Fw: " + SelectedMessage.Subject;
                                message.Attachments.AddRange(SelectedMessage.Attachments);
                                break;
                        }

                        // Set common properties
                        if (navigationContext != NavigationContext.Forward)
                            message.Header.Importance = SelectedMessage.Header.Importance;
                        message.Date = SelectedMessage.Date;
                        message.From = SelectedMessage.From;
                        message.TextContentType = SelectedMessage.TextContentType;
                        message.Text = SelectedMessage.Text;
                        message.PlainText = SelectedMessage.PlainText;
                    }
                }

                // Navigate to compose mail page
                MessageNavigationContext = new MessageNavigationContext(message, navigationContext);
                Frame.Navigate(typeof(ComposeMailPage));
                Window.Current.Content = Frame;
                Window.Current.Activate();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                try
                {
                    MessageDialog messageDialog = new MessageDialog(string.Format("A problem occured while trying to {0} the selected message.", navigationContext.ToString().ToLower()), string.Format("{0} Message Failed", navigationContext.ToString()));
                    messageDialog.ShowAsync();
                }
                catch (Exception ex2)
                {
                    LogFile.Instance.LogError("", "", ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when a pointer enters the hit test area of this element.
        /// </summary>
        /// <param name="sender">The object that raised the event (Grid).</param>
        /// <param name="e">The event data (PointerRoutedEventArgs).</param>
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 0);
        }

        /// <summary>
        /// Occurs when a pointer leaves the hit test area of this element.
        /// </summary>
        /// <param name="sender">The object that raised the event (Grid).</param>
        /// <param name="e">The event data (PointerRoutedEventArgs).</param>
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        /// <summary>
        /// Occurs when an otherwise unhandled Tap interaction occurs over the hit test area of this element.
        /// </summary>
        /// <param name="sender">The object that raised the event (Grid).</param>
        /// <param name="e">The event data (TappedRoutedEventArgs).</param>
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Grid grid = (Grid)sender;
                lascListAccountsSettingsControl.ShowListAccount(((AccountSettingsData)grid.DataContext).EmailAddress);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }
    }
}
