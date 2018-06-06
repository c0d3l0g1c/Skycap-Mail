using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Skycap.Controls;
using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Net.Common.Collections;
using Skycap.Net.Common.MessageParts;
using Skycap.Net.Imap;
using Skycap.Text.Converters;

using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Skycap.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ComposeMailPage : LayoutAwarePage
    {
        /// <summary>
        /// The contact picker.
        /// </summary>
        private ContactPicker contactPicker;
        /// <summary>
        /// The font picker.
        /// </summary>
        private FontPicker fpFontPicker;
        /// <summary>
        /// The colour picker.
        /// </summary>
        private ColourPicker cpColourPicker;
        /// <summary>
        /// The insert link dialog.
        /// </summary>
        private InsertLinkDialog insertLinkDialog;
        /// <summary>
        /// A value indicating whether the control key is pressed.
        /// </summary>
        private bool isCtrlKeyDown;

        /// <summary>
        /// Initialises a new instance of the Skycap.ComposeMailPage class.
        /// </summary>
        public ComposeMailPage()
        {
            this.InitializeComponent();

            // Initialise local variables
            Current = this;
            contactPicker = new ContactPicker() { CommitButtonText = "Add Contact" };
            contactPicker.SelectionMode = ContactSelectionMode.Fields;
            contactPicker.DesiredFields.Add(KnownContactField.Email);
            fpFontPicker = new FontPicker();
            fpFontPicker.FontFamilySelectionChanged += fpFontPicker_FontFamilySelectionChanged;
            fpFontPicker.FontSizeSelectionChanged += fpFontPicker_FontSizeSelectionChanged;
            cpColourPicker = new ColourPicker();
            cpColourPicker.ColourSelectionChanged += cpColourPicker_ColourSelectionChanged;
            insertLinkDialog = new InsertLinkDialog();
            insertLinkDialog.LinkInserted += insertLinkDialog_LinkInserted;

            // Subscribe to KeyDown and KeyUp
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        /// <summary>
        /// Gets or sets the compose mail page.
        /// </summary>
        internal static ComposeMailPage Current
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of stored email addresses.
        /// </summary>
        internal List<EmailAddress> StoredEmailAddresses
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the selected mail client.
        /// </summary>
        public MailClient SelectedMailClient
        {
            get
            {
                return (MailClient)cbMailAccount.SelectedItem;
            }
            set
            {
                cbMailAccount.SelectedItem = value;
            }
        }

        /// <summary>
        /// Gets the message navigation context.
        /// </summary>
        internal static MessageNavigationContext MessageNavigationContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Occurs when the trial timer interval elapses.
        /// </summary>
        /// <param name="sender">The object that raised the event (CoreWindow).</param>
        /// <param name="e">The event data (KeyEventArgs).</param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            // If Control key
            if (e.VirtualKey == VirtualKey.Control)
                isCtrlKeyDown = true;
            // Else if Control key was already pressed
            else if (isCtrlKeyDown)
            {
                // Determine what combination Ctrl+Key was pressed
                switch (e.VirtualKey)
                {
                    // If Send
                    case VirtualKey.LeftWindows:
                        if (bSend.Visibility == Visibility.Visible)
                            bSend_Click(this, new RoutedEventArgs());
                        break;

                    // If Add Attachments
                    case VirtualKey.T:
                        if (bAttach.Visibility == Visibility.Visible)
                            bAttach_Click(this, new RoutedEventArgs());
                        break;

                    // If Save Draft
                    case VirtualKey.S:
                        if (bSaveDraft.Visibility == Visibility.Visible)
                            bSaveDraft_Click(this, new RoutedEventArgs());
                        break;

                    // If Discard
                    case VirtualKey.D:
                        if (bDelete.Visibility == Visibility.Visible)
                            bDelete_Click(this, new RoutedEventArgs());
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
                isCtrlKeyDown = false;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected async override void SaveState(Dictionary<string, object> pageState)
        {
            try
            {
                if (FillMessage(false, true))
                {
                    // Save to drafts
                    await SelectedMailClient.SaveToDrafts(MessageNavigationContext.Message);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Invoked when this page is about to be navigated away from.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached. The Parameter property provides the group to be displayed.</param>
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                // If Back
                if (e.NavigationMode == NavigationMode.Back)
                {
                    // Reset cache
                    int cacheSize = MainPage.Current.Frame.CacheSize;
                    MainPage.Current.Frame.CacheSize = 0;
                    MainPage.Current.Frame.CacheSize = cacheSize;

                    // Show On Keyboard Input
                    SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;
                }
                else
                {
                    // Do base OnNavigatedFrom
                    base.OnNavigatedFrom(e);
                }

                // Save the email addresses
                await IOUtil.SaveStoredEmailAddresses(SelectedMailClient.AccountSettingsData.EmailAddress, StoredEmailAddresses);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached. The Parameter property provides the group to be displayed.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                // Do base OnNavigatedTo
                base.OnNavigatedTo(e);

                // Set control defaults
                MessageNavigationContext = MainPage.Current.MessageNavigationContext;
                cbMailAccount.ItemsSource = MainPage.Current.MailClientsDictionary.Values;
                cbMailAccount.SelectedItem = MainPage.Current.SelectedMailClient;
                cbMailAccount.IsEnabled = (MainPage.Current.MailClientsDictionary.Values.Count > 1);

                // Show On Keyboard Input
                SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;

                // Bind controls to the message
                if (MessageNavigationContext.NavigationContext != NavigationContext.Forward)
                {
                    txtTo.EmailAddresses.AddRange(MessageNavigationContext.Message.To);
                    txtCc.EmailAddresses.AddRange(MessageNavigationContext.Message.CarbonCopies);
                }
                txtBcc.EmailAddresses.AddRange(MessageNavigationContext.Message.BlindedCarbonCopies);
                cbPriority.SelectedItem = MessageNavigationContext.Message.Header.Importance.ToString();
                txtSubject.Text = (MessageNavigationContext.Message.Subject == "(No subject)" ? string.Empty : MessageNavigationContext.Message.Subject);

                // Determine the navigation context
                switch (MessageNavigationContext.NavigationContext)
                {
                    // If Edit
                    case NavigationContext.Edit:
                        switch (MessageNavigationContext.Message.TextContentType)
                        {
                            case ETextContentType.Html:
                                rbHtml.IsChecked = true;
                                redBody.Document.SetText(TextSetOptions.FormatRtf, MessageNavigationContext.Message.TempText);
                                emailBodyText.NavigateToString(await IOUtil.FormattedHtml(MessageNavigationContext.Message));
                                break;

                            case ETextContentType.Plain:
                                rbPlain.IsChecked = true;
                                redBody.Document.SetText(TextSetOptions.None, (MessageNavigationContext.Message.TempPlainText ?? MessageNavigationContext.Message.TempText));
                                emailBodyText.NavigateToString((MessageNavigationContext.Message.PlainText ?? MessageNavigationContext.Message.Text).Replace(Environment.NewLine, "<br/>"));
                                break;
                        }
                        break;

                    // Anything else
                    default:
                        switch (MessageNavigationContext.Message.TextContentType)
                        {
                            case ETextContentType.Html:
                                rbHtml.IsChecked = true;
                                emailBodyText.NavigateToString(await IOUtil.FormattedHtml(MessageNavigationContext.Message));
                                break;

                            case ETextContentType.Plain:
                                rbPlain.IsChecked = true;
                                emailBodyText.NavigateToString((MessageNavigationContext.Message.PlainText ?? MessageNavigationContext.Message.Text).Replace(Environment.NewLine, "<br/>"));
                                break;
                        }
                        break;
                }

                alvAttachments.ItemsSource = MessageNavigationContext.Message.Attachments;

                // Create the rtf text
                CreateRtfText(MessageNavigationContext);

                // Update layout
                UpdateLayout();

                // Ensure that the grid size is limited so content can scroll
                gBody.Height = gBody.ActualHeight;
                redBody.MaxHeight = gBody.Height;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the AppBar changes from hidden to visible.
        /// </summary>
        /// <param name="sender">The object that raised the event (BottomAppBar).</param>
        /// <param name="e">The event data (object).</param>
        private void BottomAppBar_Opened(object sender, object e)
        {
            // Make sure bottom app bar is closed if a dialog is open
            if (MainPage.Current.IsDialogOpen)
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
            if (MainPage.Current.IsDialogOpen)
                return;

            App.UnfillWebViewBrush(this, BottomAppBar, emailBodyTextBrush, emailBodyText);
        }

        /// <summary>
        /// .
        /// </summary>
        /// <param name="sender">The object that raised the event (ComboBox).</param>
        /// <param name="e">The event data (SelectionChangedEventArgs).</param>
        private async void cbMailAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Get the stored email addresses
                StoredEmailAddresses = await IOUtil.GetStoredEmailAddresses(SelectedMailClient.AccountSettingsData.EmailAddress);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Show More button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (HyperlinkButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void hbShowMore_Click(object sender, RoutedEventArgs e)
        {
            // Show more
            hbShowMore.Visibility = Visibility.Collapsed;
            tbBcc.Visibility = Visibility.Visible;
            txtBcc.Visibility = Visibility.Visible;
            bBccAddContact.Visibility = Visibility.Visible;
            tbPriority.Visibility = Visibility.Visible;
            cbPriority.Visibility = Visibility.Visible;
            tbContentType.Visibility = Visibility.Visible;
            spContentType.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Fires when a Content Type radio button is fired.
        /// </summary>
        /// <param name="sender">The object that raised the event (HyperlinkButton).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void ContentType_Checked(object sender, RoutedEventArgs e)
        {
            if (rbHtml != null && rbPlain != null)
            {
                if (rbHtml.IsChecked.Value)
                    RightPanel.Visibility = Visibility.Visible;
                else if (rbPlain.IsChecked.Value)
                    RightPanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Occurs when the system processes an interaction that displays a context menu.
        /// </summary>
        /// <param name="sender">The object that raised the event (ContextMenuEventArgs).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void redBody_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;

            // Open bottom app bar
            if (!BottomAppBar.IsOpen)
                BottomAppBar.IsOpen = true;
        }

        /// <summary>
        /// Occurs when redBody receives focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (RichEditBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void redBody_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string body;
                redBody.Document.GetText(TextGetOptions.None, out body);
                if (body.Trim(Environment.NewLine.ToCharArray()).Equals(StructuredMessage.DefaultBody, StringComparison.OrdinalIgnoreCase))
                    redBody.Document.SetText(TextSetOptions.None, string.Empty);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when redBody loses focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (redBody).</param>
        /// <param name="e">The event data (KeyRoutedEventArgs).</param>
        private void redBody_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                char[] spaceOrEOL = { ' ', '\r' };
                // If spacebar or enter is pressed, see if they just typed an url
                if ((e.Key == Windows.System.VirtualKey.Space) || (e.Key == Windows.System.VirtualKey.Enter))
                {
                    int startpos = redBody.Document.Selection.StartPosition;
                    int endpos = redBody.Document.Selection.EndPosition;

                    //doublecheck there's no selection
                    if (endpos == startpos)
                    {
                        string curText = string.Empty;
                        redBody.Document.GetText(TextGetOptions.None, out curText);
                        //remove final space
                        curText = curText.TrimEnd(spaceOrEOL);
                        endpos = curText.Length - 1;

                        //walk backwards until start of line or space is found
                        startpos = curText.LastIndexOfAny(spaceOrEOL);
                        startpos++;

                        string checkForUrl = curText.Substring(startpos, endpos - startpos + 1);
                        if (checkForUrl.StartsWith("http:") || checkForUrl.StartsWith("www") || checkForUrl.StartsWith("https:"))
                        {
                            if (checkForUrl.StartsWith("www")) checkForUrl = "http://" + checkForUrl;
                            //make a hyperlink
                            ITextRange linkRange = redBody.Document.GetRange(startpos, endpos + 1);
                            linkRange.Link = "\"" + checkForUrl + "\"";
                            linkRange.CharacterFormat.ForegroundColor = AppResources.HepColourBrush.Color;
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
        /// Occurs when redBody loses focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (redBody).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void redBody_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                string body;
                redBody.Document.GetText(TextGetOptions.None, out body);
                if (string.IsNullOrEmpty(body.Trim(Environment.NewLine.ToCharArray())))
                    redBody.Document.SetText(TextSetOptions.None, StructuredMessage.DefaultBody);

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the EmailBodyText gets focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListBox).</param>
        /// <param name="e">The event data (DragItemsStartingEventArgs).</param>
        private void emailBodyText_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSubject.Focus(FocusState.Pointer);
        }

        /// <summary>
        /// Occurs when an Add Contact button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bAddContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the contacts
                IReadOnlyList<ContactInformation> contacts = await contactPicker.PickMultipleContactsAsync();

                // Loop through each contact
                foreach (ContactInformation contact in contacts.Where(o => o.Emails.Count > 0))
                {
                    // If To
                    if (sender == bToAddContact)
                        txtTo.EmailAddresses.Insert(txtTo.EmailAddresses.Count - 1, new FormattedEmailAddress(contact.Emails[0].Value, contact.Name));
                    // Else if Cc
                    else if (sender == bCcAddContact)
                        txtCc.EmailAddresses.Insert(txtCc.EmailAddresses.Count - 1, new FormattedEmailAddress(contact.Emails[0].Value, contact.Name));
                    // Else if Bcc
                    else if (sender == bBccAddContact)
                        txtBcc.EmailAddresses.Insert(txtBcc.EmailAddresses.Count - 1, new FormattedEmailAddress(contact.Emails[0].Value, contact.Name));
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                try
                {
                    MessageDialog messageDialog = new MessageDialog("A problem occured while trying to add a contact.", "Add Contact Failed");
                    messageDialog.ShowAsync();
                }
                catch (Exception ex2)
                {
                    LogFile.Instance.LogError("", "", ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when the currently selected font family changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (FontPicker).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void fpFontPicker_FontFamilySelectionChanged(object sender, EventArgs e)
        {
            try
            {
                redBody.Document.Selection.CharacterFormat.Name = fpFontPicker.SelectedFontFamily.Source;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the currently selected font size changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (FontPicker).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void fpFontPicker_FontSizeSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                redBody.Document.Selection.CharacterFormat.Size = fpFontPicker.SelectedFontSize;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the currently selected colour changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (ColourPicker).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void cpColourPicker_ColourSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                redBody.Document.Selection.CharacterFormat.ForegroundColor = cpColourPicker.SelectedColour;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a link is inserted.
        /// </summary>
        /// <param name="sender">The object that raised the event (InsertLinkDialog).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void insertLinkDialog_LinkInserted(object sender, EventArgs e)
        {
            try
            {
                int startPosition = redBody.Document.Selection.EndPosition;
                ITextRange range = redBody.Document.GetRange(startPosition, startPosition);
                range.Text = insertLinkDialog.TextToBeDisplayed;
                range.Link = "\"" + insertLinkDialog.Address + "\"";
                range.CharacterFormat.ForegroundColor = Colors.Blue;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Send button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fills the message
                if (FillMessage(true, (MessageNavigationContext.NavigationContext == NavigationContext.Edit)))
                {
                    // Send the message
                    SelectedMailClient.SendMessage(MessageNavigationContext.Message, (MessageNavigationContext.NavigationContext == NavigationContext.Edit));

                    // Add to stored email addresses
                    IEnumerable<EmailAddress> emailAddresses = MessageNavigationContext.Message.To.Union(MessageNavigationContext.Message.CarbonCopies).Union(MessageNavigationContext.Message.BlindedCarbonCopies).Distinct();
                    StoredEmailAddresses.AddRange(emailAddresses.Where(o => !StoredEmailAddresses.Contains(o)));

                    // Go Back to main page
                    GoBack(this, e);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                try
                {
                    MessageDialog messageDialog = new MessageDialog("A problem occured while trying to send the message.", "Send Message Failed");
                    messageDialog.ShowAsync();
                }
                catch (Exception ex2)
                {
                    LogFile.Instance.LogError("", "", ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when the Attach button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bAttach_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the atachments
                FileOpenPicker fileOpenPicker = new FileOpenPicker();
                fileOpenPicker.FileTypeFilter.Add("*");
                fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                IReadOnlyList<StorageFile> storageFiles = await fileOpenPicker.PickMultipleFilesAsync();

                // Attach each file to the message
                foreach (StorageFile storageFile in storageFiles)
                {
                    string contentId = Guid.NewGuid().ToString();
                    ContentType contentType = null;
                    if (string.IsNullOrEmpty(storageFile.ContentType))
                    {
                        contentType = new ContentType();
                    }
                    else
                    {
                        string[] contentTypeParts = storageFile.ContentType.Split('/');
                        contentType = new ContentType(contentTypeParts[0], contentTypeParts[1]);
                    }
                    StorageFile tempStorageFile = await storageFile.CopyAsync(ApplicationData.Current.TemporaryFolder, Path.GetRandomFileName());
                    BasicProperties basicProperties = await storageFile.GetBasicPropertiesAsync();
                    Attachment attachment = new Attachment(tempStorageFile.Name, contentId, contentType, Path.GetDirectoryName(tempStorageFile.Path), basicProperties.Size);
                    attachment.TransferFilename = storageFile.Name;
                    attachment.IsUIAttachment = true;
                    MessageNavigationContext.Message.Attachments.Add(attachment);
                }

                // Bind the attachments
                alvAttachments.ItemsSource = null;
                alvAttachments.ItemsSource = MessageNavigationContext.Message.Attachments;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                try
                {
                    MessageDialog messageDialog = new MessageDialog("A problem occured while trying to attach one or more files.", "Add Attachments Failed");
                    messageDialog.ShowAsync();
                }
                catch (Exception ex2)
                {
                    LogFile.Instance.LogError("", "", ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when the Delete button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Delete from drafts
                if (MessageNavigationContext.Message != null
                 && !string.IsNullOrEmpty(MessageNavigationContext.Message.MessagePath))
                {
                    // Make sure the client is authenticated
                    if (!SelectedMailClient.State.HasFlag(MailClientState.Authenticated))
                        SelectedMailClient.Login();

                    // Delete message
                    Dictionary<string, string> messagePaths = new Dictionary<string, string>();
                    messagePaths.Add(MessageNavigationContext.Message.Uid, MessageNavigationContext.Message.MessagePath);
                    SelectedMailClient.DeleteMessage(messagePaths, SelectedMailClient.Drafts);
                }

                // Go back to main page
                GoBack(this, e);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                try
                {
                    MessageDialog messageDialog = new MessageDialog("A problem occured while trying to delete the draft message.", "Delete Draft Failed");
                    messageDialog.ShowAsync();
                }
                catch (Exception ex2)
                {
                    LogFile.Instance.LogError("", "", ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when the Save Draft button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bSaveDraft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fills the message
                tbStatus.Text = string.Empty;
                if (FillMessage(false, true))
                {
                    // Make sure the client is authenticated
                    if (SelectedMailClient is ImapMailClient
                    && !SelectedMailClient.State.HasFlag(MailClientState.Authenticated))
                        SelectedMailClient.Login();

                    // Save to drafts
                    App.NotifyUser(tbStatus, "Saving message to drafts folder...", NotifyType.StatusMessage);
                    await SelectedMailClient.SaveToDrafts(MessageNavigationContext.Message);
                    App.NotifyUser(tbStatus, "Message saved to drafts folder.", NotifyType.StatusMessage);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                try
                {
                    MessageDialog messageDialog = new MessageDialog("A problem occured while trying to save the draft message.", "Save Draft Failed");
                    messageDialog.ShowAsync();
                }
                catch (Exception ex2)
                {
                    LogFile.Instance.LogError("", "", ex2.ToString());
                }
            }
        }

        /// <summary>
        /// Occurs when the Paste button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bPaste_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }

        /// <summary>
        /// Occurs when the Copy button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bCopy_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        /// <summary>
        /// Occurs when the Redo button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bRedo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        /// <summary>
        /// Occurs when the Undo button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bUndo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        /// <summary>
        /// Occurs when the Font button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bFont_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Point fontButtonPosition = bFont.TransformToVisual(Window.Current.Content).TransformPoint(new Point());
                fpFontPicker.SelectedFontFamily = new FontFamily(redBody.Document.Selection.CharacterFormat.Name);
                fpFontPicker.SelectedFontSize = redBody.Document.Selection.CharacterFormat.Size;
                fpFontPicker.Show(fontButtonPosition.X, Window.Current.Bounds.Height - fpFontPicker.Height - BottomAppBar.ActualHeight - 40);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Bold button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bBold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                redBody.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Italic button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bItalic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                redBody.Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Underline button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bUnderline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (redBody.Document.Selection.CharacterFormat.Underline == UnderlineType.None)
                    redBody.Document.Selection.CharacterFormat.Underline = UnderlineType.Single;
                else
                    redBody.Document.Selection.CharacterFormat.Underline = UnderlineType.None;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Font Color button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void bFontColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Point fontColorButtonPosition = bFontColor.TransformToVisual(Window.Current.Content).TransformPoint(new Point());
                cpColourPicker.SelectedColour = redBody.Document.Selection.CharacterFormat.ForegroundColor;
                bFontColor.UpdateLayout();
                double horizontalOffset = (Window.Current.Bounds.Width - cpColourPicker.Width - 40);
                cpColourPicker.Show(horizontalOffset, Window.Current.Bounds.Height - cpColourPicker.Height - BottomAppBar.ActualHeight - 40);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the Alignment button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bAlignment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a menu and add commands specifying a callback delegate for each.
                // Since command delegates are unique, no need to specify command Ids.
                PopupMenu menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Left", (command) =>
                {
                    SetAlignment(ParagraphAlignment.Left);
                }));
                menu.Commands.Add(new UICommand("Center", (command) =>
                {
                    SetAlignment(ParagraphAlignment.Center);
                }));
                menu.Commands.Add(new UICommand("Right", (command) =>
                {
                    SetAlignment(ParagraphAlignment.Right);
                }));
                menu.Commands.Add(new UICommand("Justify", (command) =>
                {
                    SetAlignment(ParagraphAlignment.Justify);
                }));

                // Show the menu
                await menu.ShowForSelectionAsync(App.GetElementRect(bMore), Placement.Above);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the More button control is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (Button).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void bMore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a menu and add commands specifying a callback delegate for each.
                // Since command delegates are unique, no need to specify command Ids.
                PopupMenu menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Insert Image", async (command) =>
                {
                    try
                    {
                        // Select the image
                        FileOpenPicker fileOpenPicker = new FileOpenPicker();
                        fileOpenPicker.FileTypeFilter.Add(".jpg");
                        fileOpenPicker.FileTypeFilter.Add(".jpeg");
                        fileOpenPicker.FileTypeFilter.Add(".gif");
                        fileOpenPicker.FileTypeFilter.Add(".png");
                        fileOpenPicker.FileTypeFilter.Add(".tif");
                        fileOpenPicker.FileTypeFilter.Add(".tiff");
                        fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                        StorageFile file = await fileOpenPicker.PickSingleFileAsync();
                        // Insert the image
                        if (file != null)
                        {
                            IRandomAccessStream randomAccessStream = await file.OpenAsync(FileAccessMode.Read);
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(randomAccessStream);
                            redBody.Document.Selection.InsertImage(bitmapImage.PixelHeight, bitmapImage.PixelWidth, 0, VerticalCharacterAlignment.Baseline, file.Name, randomAccessStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError("", "", ex.ToString());
                        MessageDialog messageDialog = new MessageDialog("Unable to insert image.", "Insert Image Failed");
                        messageDialog.ShowAsync();
                    }
                }));
                menu.Commands.Add(new UICommand("Insert Link", (command) =>
                {
                    insertLinkDialog.Show();
                }));
                menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Bulleted List", (command) =>
                {
                    if (redBody.Document.Selection.ParagraphFormat.ListType == MarkerType.None)
                        redBody.Document.Selection.ParagraphFormat.ListType = MarkerType.Bullet;
                    else
                        redBody.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
                }));
                menu.Commands.Add(new UICommand("Numbered List", (command) =>
                {
                    redBody.Document.Selection.ParagraphFormat.ListStyle = MarkerStyle.Period;
                    if (redBody.Document.Selection.ParagraphFormat.ListType == MarkerType.None)
                        redBody.Document.Selection.ParagraphFormat.ListType = MarkerType.UnicodeSequence;
                    else
                        redBody.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
                }));
                menu.Commands.Add(new UICommand("Clear formatting", (command) =>
                {
                    redBody.Document.Selection.SetText(TextSetOptions.None, redBody.Document.Selection.Text);
                }));
                // Show the menu
                await menu.ShowForSelectionAsync(App.GetElementRect(bMore), Placement.Above);

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Copies the text of the text range to the Clipboard.
        /// </summary>
        private void Copy()
        {
            try
            {
                if (redBody.Document.CanCopy())
                    redBody.Document.Selection.Copy();

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Pastes text from the Clipboard into the text range.
        /// </summary>
        private void Paste()
        {
            try
            {
                if (redBody.Document.CanPaste())
                    redBody.Document.Selection.Paste(0);

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Reverses the most recent undo operation.
        /// </summary>
        private void Redo()
        {
            try
            {
                if (redBody.Document.CanRedo())
                    redBody.Document.Redo();

            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Undoes the most recent undo group.
        /// </summary>
        private void Undo()
        {
            try
            {
                if (redBody.Document.CanUndo())
                    redBody.Document.Undo();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Sets the paragraph alignment.
        /// </summary>
        /// <param name="alignment">The alignment to set.</param>
        private void SetAlignment(ParagraphAlignment alignment)
        {
            try
            {
                if (redBody.Document.Selection.ParagraphFormat.Alignment == alignment)
                    redBody.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Undefined;
                else
                    redBody.Document.Selection.ParagraphFormat.Alignment = alignment;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Fills the specified message.
        /// </summary>
        /// <param name="validate">true if validate; otherwise, false.</param>
        /// <param name="isDraft">true if is draft; otherwise, false.</param>
        private bool FillMessage(bool validate, bool isDraft)
        {
            try
            {
                // Store the text
                string text = MessageNavigationContext.Message.Text;
                string plainText = MessageNavigationContext.Message.PlainText;

                // Create new message and restore the text
                string uid = MessageNavigationContext.Message.Uid;
                string messagePath = MessageNavigationContext.Message.MessagePath;
                MessageNavigationContext.Message = (SelectedMailClient.AccountSettingsData.EmailService == Data.EmailService.Pop ? (StructuredMessage)new PopMessage() : (StructuredMessage)new ImapMessage());
                MessageNavigationContext.Message.Text = text;
                MessageNavigationContext.Message.PlainText = plainText;

                // If is draft
                if (isDraft)
                {
                    MessageNavigationContext.NavigationContext = NavigationContext.Edit;
                    MessageNavigationContext.Message.Uid = uid;
                    MessageNavigationContext.Message.MessagePath = messagePath;
                }

                // Make sure all the fields are populated
                string body = null;
                if (validate && (string.IsNullOrEmpty(txtSubject.Text) || txtSubject.Text.Equals(StructuredMessage.DefaultSubject, StringComparison.OrdinalIgnoreCase)))
                {
                    txtSubject.Focus(FocusState.Programmatic);
                    App.NotifyUser(tbStatus, "Please supply a subject for this email.", NotifyType.ErrorMessage);
                    return false;
                }
                else if (validate && string.IsNullOrEmpty(body))
                {
                    redBody.Document.GetText(TextGetOptions.None, out body);
                    body = body.Trim(Environment.NewLine.ToCharArray());
                    if (string.IsNullOrEmpty(body) || body.Equals(StructuredMessage.DefaultBody, StringComparison.OrdinalIgnoreCase))
                    {
                        redBody.Focus(FocusState.Programmatic);
                        App.NotifyUser(tbStatus, "Please supply a body for this email.", NotifyType.ErrorMessage);
                        return false;
                    }
                }

                // Set the mesage properties
                MessageNavigationContext.Message.To = GetEmailAddresses(txtTo);
                MessageNavigationContext.Message.CarbonCopies = GetEmailAddresses(txtCc);
                MessageNavigationContext.Message.BlindedCarbonCopies = GetEmailAddresses(txtBcc);

                // Make sure there are recipients
                if (validate
                 && MessageNavigationContext.Message.To.Count == 0
                 && MessageNavigationContext.Message.CarbonCopies.Count == 0
                 && MessageNavigationContext.Message.BlindedCarbonCopies.Count == 0)
                {
                    txtTo.Focus(FocusState.Programmatic);
                    App.NotifyUser(tbStatus, "Please supply atleast one recipient in the To, Cc or Bcc fields.", NotifyType.ErrorMessage);
                    return false;
                }

                MessageNavigationContext.Message.Subject = txtSubject.Text;
                ComboBoxItem importanceComboBoxItem = (ComboBoxItem)cbPriority.SelectedItem;
                MessageNavigationContext.Message.Header.Importance = (MailImportance)Enum.Parse(typeof(MailImportance), importanceComboBoxItem.Content.ToString());

                // Determine if plain or html mail
                if (rbPlain.IsChecked.Value)
                    MessageNavigationContext.Message.TextContentType = ETextContentType.Plain;
                else if (rbHtml.IsChecked.Value)
                {
                    MessageNavigationContext.Message.TextContentType = ETextContentType.Html;
                    redBody.Document.GetText(TextGetOptions.FormatRtf, out text);
                    if (MessageNavigationContext.NavigationContext == NavigationContext.Edit)
                    {
                        MessageNavigationContext.Message.TempText = text;
                        MessageNavigationContext.Message.Text = MessageNavigationContext.Message.Text;
                    }
                    else
                    {
                        RTF2HTML rtf2Html = new RTF2HTML(text);
                        string html = rtf2Html.ParseRTF();
                        html += string.Format("<hr style=\"color: {0};\"><br/><br/>", AppResources.OctColourBrush);
                        int bodyIndex = MessageNavigationContext.Message.Text.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
                        if (bodyIndex == -1)
                            MessageNavigationContext.Message.Text = html + MessageNavigationContext.Message.Text;
                        else
                            MessageNavigationContext.Message.Text = MessageNavigationContext.Message.Text.Insert(bodyIndex, html);
                    }
                }

                // Get the plain text
                redBody.Document.Selection.StartPosition = 0;
                redBody.Document.Selection.EndPosition = redBody.Document.Selection.StoryLength;
                MessageNavigationContext.Message.PlainText = redBody.Document.Selection.Text + MessageNavigationContext.Message.PlainText;

                // Get the attachments
                if (alvAttachments.ItemsSource != null)
                {
                    AttachmentCollection attachments = (AttachmentCollection)alvAttachments.ItemsSource;
                    foreach (Attachment attachment in attachments.ToList())
                    {
                        if (!MessageNavigationContext.Message.Attachments.Contains(attachment))
                            MessageNavigationContext.Message.Attachments.Add(attachment);
                    }
                }

                // Clear the status
                tbStatus.Text = string.Empty;

                return true;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Gets the email addresses from the specified Rich Edit Box.
        /// </summary>
        /// <param name="redEmailAddress">The rich edit box.</param>
        /// <returns>The email addresses.</returns>
        private EmailAddressCollection GetEmailAddresses(EmailAddressInput redEmailAddress)
        {
            EmailAddressCollection emailAddresses = new EmailAddressCollection();
            try
            {
                // Get the email addresses
                emailAddresses.AddRange(redEmailAddress.EmailAddresses.Take(redEmailAddress.EmailAddresses.Count - 1).Select(o => new EmailAddress(o.Address, o.DisplayName)));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
            return emailAddresses;
        }

        /// <summary>
        /// Writes a new line.
        /// </summary>
        /// <param name="rtf">The rtf builder.</param>
        private void WriteNewLine(StringBuilder rtf)
        {
            rtf.Append(@"\par");
        }

        /// <summary>
        /// Writes the signature.
        /// </summary>
        /// <param name="rtf">The rtf builder.</param>
        /// <param name="isSignatureRtf">true if signature is rtf; otherwise, false.</param>
        /// <param name="value">The signature value.</param>
        private void WriteSignature(StringBuilder rtf, bool isSignatureRtf, string value)
        {
            rtf.Append(string.Format(@"{0}{1}\par", (isSignatureRtf ? "" : @"\pard "), value));
        }

        /// <summary>
        /// Writes a horizontal line.
        /// </summary>
        /// <param name="rtf">The rtf builder.</param>
        /// <param name="length">The length of the horizontal line.</param>
        private void WriteHorizontalLine(StringBuilder rtf, int length)
        {
            rtf.Append(string.Format(@"\pard {0}\par", string.Empty.PadLeft(length, '_')));
        }

        /// <summary>
        /// Writes the reply text.
        /// </summary>
        /// <param name="rtf">The rtf builder.</param>
        /// <param name="name">The reply name.</param>
        /// <param name="value">The reply value.</param>
        private void WriteReplyText(StringBuilder rtf, string name, string value)
        {
            if (name == "From")
                rtf.Append(@"\pard");

            rtf.Append(string.Format(@"\b {0}:\b0  {1}\par", name, value));
        }

        /// <summary>
        /// Creates the rtf text.
        /// </summary>
        /// <param name="MessageNavigationContext">The message navigation context.</param>
        private void CreateRtfText(MessageNavigationContext MessageNavigationContext)
        {
            try
            {
                // If MainPage is available
                if (MainPage.Current != null
                 && SelectedMailClient != null)
                {
                    // Determine the message navigation context
                    switch (MessageNavigationContext.NavigationContext)
                    { 
                        // If New
                        case NavigationContext.New:
                            redBody.Foreground = AppResources.PentColourBrush;
                            if (SelectedMailClient.AccountSettingsData.UseAnEmailSignature)
                                redBody.Document.SetText(TextSetOptions.FormatRtf, SelectedMailClient.AccountSettingsData.EmailSignature);
                            break;

                        // If Edit
                        case NavigationContext.Edit:
                            break;

                        // Anything else
                        default:
                            // Stores the rtf
                            StringBuilder rtf = new StringBuilder();

                            // Stores a value indicating if the signature contains rtf
                            bool isSignatureRtf = SelectedMailClient.AccountSettingsData.EmailSignature.StartsWith(@"{\rtf1");

                            // Write rtf start tag
                            if (!isSignatureRtf)
                            {
                                // Open rtf tag
                                rtf.Append(@"{\rtf1");

                                // Write new line
                                WriteNewLine(rtf);

                                // Write new line
                                WriteNewLine(rtf);
                            }

                            // Write signature
                            if (SelectedMailClient.AccountSettingsData.UseAnEmailSignature)
                                WriteSignature(rtf, isSignatureRtf, SelectedMailClient.AccountSettingsData.EmailSignature);

                            // If not new
                            if (MessageNavigationContext.NavigationContext != NavigationContext.New)
                            {
                                // Write horizontal line
                                WriteHorizontalLine(rtf, (int)100);

                                // Write new line
                                WriteNewLine(rtf);

                                // Write reply text
                                WriteReplyText(rtf, "From", MessageNavigationContext.Message.From.ToString());
                                WriteReplyText(rtf, "Sent", MessageNavigationContext.Message.Date.ToString("ddd, MMMM dd, yyyy HH:mm tt"));
                                WriteReplyText(rtf, "To", string.Join("; ", MessageNavigationContext.Message.To.Select(o => o.DisplayNameAlternate)));
                                if (MessageNavigationContext.Message.CarbonCopies.Count > 0)
                                    WriteReplyText(rtf, "Cc", string.Join("; ", MessageNavigationContext.Message.CarbonCopies.Select(o => o.DisplayNameAlternate)));
                                WriteReplyText(rtf, "Subject", MessageNavigationContext.Message.Subject);
                                if (MessageNavigationContext.Message.Header.Importance != MailImportance.Normal)
                                    WriteReplyText(rtf, "Importance", MessageNavigationContext.Message.Header.Importance.ToString());
                            }

                            // Write rtf end tag
                            if (!isSignatureRtf)
                            {
                                // Close rtf tag
                                rtf.Append(@"}");
                            }

                            // Set the rtf text
                            ITextRange textRange = redBody.Document.Selection;
                            textRange.StartPosition = 0;
                            textRange.EndPosition = 0;
                            textRange.SetText(TextSetOptions.FormatRtf, rtf.ToString());
                            break;
                    }
                    txtSubject.Focus(FocusState.Programmatic);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }
    }
}
