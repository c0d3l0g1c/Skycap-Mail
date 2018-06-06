using System;
using System.Linq;
using System.Threading.Tasks;
using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Notifications;
using Skycap.Pages;
using Skycap.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Skycap
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// The storage settings.
        /// </summary>
        private static StorageSettings _storageSettings;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Resuming += OnResuming;
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the session state is corrupted.
        /// </summary>
        internal static bool IsSessionStateCorrupted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the email toast notification context.
        /// </summary>
        internal static EmailToastNotificationContext EmailToastNotificationContext
        {
            get;
            set;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Indicate ui is not supended
            StorageSettings.IsUISuspended = false;

            // Unregister background tasks if registered
            SyncMailBackgroundTask.Unregister();

            // Wait for background task to complete
            WaitForBackgroundTaskToComplete();

            try
            {
                EmailToastNotificationContext = (string.IsNullOrEmpty(args.Arguments) ? null : IOUtil.Deserialize<EmailToastNotificationContext>(args.Arguments));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                SuspensionManager.RegisterFrame(rootFrame, "appFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated
                 || args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser
                 || args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                {
                    try
                    {
                        await SuspensionManager.RestoreAsync();

                        if (StorageSettings.MailHeaderDictionary.AllEmailCount == 0)
                            throw new Exception("Mail header dictionary data missing.");

                        IsSessionStateCorrupted = false;
                    }
                    catch
                    {
                        IsSessionStateCorrupted = true;
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            else
            {
                try
                {
                    // If toast notification
                    if (App.EmailToastNotificationContext != null)
                    {
                        // Set the selected items
                        MainPage.Current.SelectedAccount = StorageSettings.AccountSettingsDataDictionary.Single(o => o.Key.Equals(App.EmailToastNotificationContext.EmailAddress, StringComparison.OrdinalIgnoreCase));
                        MainPage.Current.SelectedMailboxListViewItem = MainPage.Current.MailboxTreeDataContext.Value.Value.Single(o => o.Mailbox.FullName.Equals(App.EmailToastNotificationContext.MailboxFullName));
                        MainPage.Current.SelectedMailHeader = StorageSettings.MailHeaderDictionary[App.EmailToastNotificationContext.EmailAddress][App.EmailToastNotificationContext.MailboxFullName].Single(o => o.Uid.Equals(App.EmailToastNotificationContext.Uid, StringComparison.OrdinalIgnoreCase));
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError("", "", ex.ToString());
                }
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            LogFile.Instance.LogInformation("", "", string.Format("Launched: IsUISuspended - {0}, IsTaskRunning - {1}.", StorageSettings.IsUISuspended, StorageSettings.IsTaskRunning));

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being resumed.
        /// </summary>
        /// <param name="sender">The source of the resume request.</param>
        /// <param name="e">Details about the resume request.</param>
        private void OnResuming(object sender, object e)
        {
            try
            {
                // Indicate ui is not supended
                StorageSettings.IsUISuspended = false;

                // Unregister background tasks if registered
                SyncMailBackgroundTask.Unregister();

                // Wait for background task to complete
                WaitForBackgroundTaskToComplete();

                // If MailHeadersItemsSource
                MainPage.Current.BindMailHeader(StorageSettings.AccountSettingsDataDictionary[MainPage.Current.MailboxTreeDataContext.Value.Key.EmailAddress], ((MailboxListViewItem)SuspensionManager.SessionState["MailboxTreeSelectedItem"]).Mailbox, MainPage.Current.QueryText);
                LogFile.Instance.LogInformation("", "", string.Format("Resumed: IsUISuspended - {0}, IsTaskRunning - {1}.", StorageSettings.IsUISuspended, StorageSettings.IsTaskRunning));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogInformation("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = null;
            try
            {
                // Wait for below code to complete
                deferral = e.SuspendingOperation.GetDeferral();

                // Indicate ui is supended
                StorageSettings.IsUISuspended = true;

                // Register background tasks if not registered
                SyncMailBackgroundTask.Register();

                // Unload and disconnect mailboxes
                MainPage.Current.UnloadAndDisconnectMailboxes();

                // Save state
                await SuspensionManager.SaveAsync();
                LogFile.Instance.LogInformation("", "", string.Format("Suspended: IsUISuspended - {0}, IsTaskRunning - {1}.", StorageSettings.IsUISuspended, StorageSettings.IsTaskRunning));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogInformation("", "", ex.ToString());
            }
            finally
            {
                // Indicate app can be suspended
                deferral.Complete();
            }
        }

        /// <summary>
        /// Occurs when an exception is raised by application code, forwarded from the native level. Applications can mark the occurrence as handled in event data.
        /// </summary>
        /// <param name="sender">The object that raised the event (Application).</param>
        /// <param name="e">The event data (UnhandledExceptionEventArgs).</param>
        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            LogFile.Instance.LogError("", "", e.Exception.ToString());
        }

        /// <summary>
        /// Occurs when a faulted System.Threading.Tasks.Task's unobserved exception is about to trigger exception escalation policy, which, by default, would terminate the process.
        /// </summary>
        /// <param name="sender">The object that raised the event (TaskScheduler).</param>
        /// <param name="e">The event data (UnobservedTaskExceptionEventArgs).</param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            LogFile.Instance.LogError("", "", e.Exception.ToString());
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            try
            {
                // TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
                // event in OnWindowCreated to speed up searches once the application is already running

                // If the Window isn't already using Frame navigation, insert our own Frame
                var previousContent = Window.Current.Content;
                var frame = previousContent as Frame;

                // If the app does not contain a top-level frame, it is possible that this 
                // is the initial launch of the app. Typically this method and OnLaunched 
                // in App.xaml.cs can call a common method.
                if (frame == null)
                {
                    // Create a Frame to act as the navigation context and associate it with
                    // a SuspensionManager key
                    frame = new Frame();
                    SuspensionManager.RegisterFrame(frame, "appFrame");

                    if (args.PreviousExecutionState == ApplicationExecutionState.Terminated
                     || args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser
                     || args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                    {
                        // Restore the saved session state only when appropriate
                        try
                        {
                            await SuspensionManager.RestoreAsync();

                            if (StorageSettings.MailHeaderDictionary.AllEmailCount == 0)
                                throw new Exception("Mail header dictionary data missing.");
                        }
                        catch (SuspensionManagerException ex)
                        {
                            //Something went wrong restoring state.
                            //Assume there is no state and continue
                            LogFile.Instance.LogError("", "", ex.ToString());
                            IsSessionStateCorrupted = true;
                        }
                    }
                }

                // Show progress ring
                MainPage.Current.ResetEmailHeaderProgressRingState(MainPage.Current.SelectedMailClient, MainPage.Current.SelectedMailbox, true);

                // Reset email header state
                MainPage.Current.ResetEmailHeaderFilterState(true, args.QueryText);

                // Bind mail header
                await MainPage.Current.BindMailHeaderAsync(MainPage.Current.SelectedMailClient.AccountSettingsData, MainPage.Current.SelectedMailbox, args.QueryText);

                // Hide progress ring
                MainPage.Current.ResetEmailHeaderProgressRingState(MainPage.Current.SelectedMailClient, MainPage.Current.SelectedMailbox, false);

                // Ensure the current window is active
                Window.Current.Activate();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Invoked when the application is activated as the target of a sharing operation.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected override void OnShareTargetActivated(Windows.ApplicationModel.Activation.ShareTargetActivatedEventArgs args)
        {
            try
            {
                var shareTargetPage = new Skycap.Pages.ShareTargetPage();
                shareTargetPage.Activate(args);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Notifies the user of an important message.
        /// </summary>
        /// <param name="textBlock">The text block used to display the message.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="notifyType">The notify type.</param>
        internal static void NotifyUser(TextBlock textBlock, string message, NotifyType notifyType)
        {
            try
            {
                // Determine the notify type
                switch (notifyType)
                {
                    // If StatusMessage
                    case NotifyType.StatusMessage:
                        textBlock.Foreground = AppResources.QuadColourBrush;
                        break;

                    // If ErrorMessage
                    case NotifyType.ErrorMessage:
                        textBlock.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                }
                // Set the message
                textBlock.Text = message;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Fills the web view brush so controls can correctly overlay the web view control.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="bottomAppBar">The bottom app bar.</param>
        /// <param name="rectanglePlaceholder">The rectangle web view placeholder.</param>
        /// <param name="webView">The web view.</param>
        internal static void FillWebViewBrush(Page page, AppBar bottomAppBar, Rectangle rectanglePlaceholder, WebView webView)
        {
            try
            {
                WebViewBrush webViewBrush = new WebViewBrush();
                webViewBrush.SourceName = webView.Name;
                webViewBrush.Redraw();
                rectanglePlaceholder.Fill = webViewBrush;
                webView.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Unfills the web view brush so controls can correctly overlay the web view control.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="bottomAppBar">The bottom app bar.</param>
        /// <param name="rectanglePlaceholder">The rectangle web view placeholder.</param>
        /// <param name="webView">The web view.</param>
        internal static void UnfillWebViewBrush(Page page, AppBar bottomAppBar, Rectangle rectanglePlaceholder, WebView webView)
        {
            try
            {
                rectanglePlaceholder.Fill = new SolidColorBrush(Colors.Transparent);
                webView.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Gets an elements rect structure.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element rect.</returns>
        internal static Rect GetElementRect(FrameworkElement element)
        {
            try
            {
                GeneralTransform buttonTransform = element.TransformToVisual(null);
                Point point = buttonTransform.TransformPoint(new Point());
                point.X -= 20;
                return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return new Rect();
            }
        }

        /// <summary>
        /// Runs the specified action on the UI thread.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="action">The action to run.</param>
        /// <param name="coreDispatcherPriority">The core dispatcher priority.</param>
        internal static async void RunOnUIThread(CoreDispatcher dispatcher, DispatchedHandler action, CoreDispatcherPriority coreDispatcherPriority = CoreDispatcherPriority.Low)
        {
            try
            {
                await dispatcher.RunAsync(coreDispatcherPriority, action);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Waits for the background task to complete.
        /// </summary>
        internal static async void WaitForBackgroundTaskToComplete()
        {
            // Get the current time
            DateTime time = DateTime.Now;

            // Make sure the task is not running
            while (StorageSettings.IsTaskRunning
                && DateTime.Now.Subtract(time).Seconds <= 10)
                await Task.Delay(50);

            // Mark background task as completed
            StorageSettings.IsTaskRunning = false;
        }
    }
}