using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Skycap.Data;
using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Notifications;

using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Skycap.Tasks
{
    /// <summary>
    /// Represents the mail background task.
    /// </summary>
    public sealed class SyncMailBackgroundTask : IBackgroundTask
    {
        /// <summary>
        /// The Skycap Mail Sync task name.
        /// </summary>
        private const string SkycapMailSyncTaskName = "SkycapMailSync";

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;
        /// <summary>
        /// The mail clients.
        /// </summary>
        private List<MailClient> _mailClients;

        /// <summary>
        /// Initialises a new instance of the Skycap.Tasks.SyncMailBackgroundTask class.
        /// </summary>
        public SyncMailBackgroundTask()
        {
            ApplicationData.Current.DataChanged += Current_DataChanged;
        }

        /// <summary>
        /// Determines if this task is registered.
        /// </summary>
        /// <param name="backgroundTaskRegistration">The background task registration.</param>
        /// <returns>true if registered; otherwise, false.</returns>
        private static bool IsRegistered(out IBackgroundTaskRegistration backgroundTaskRegistration)
        {
            backgroundTaskRegistration = null;
            try
            {
                // Get the background task registration
                backgroundTaskRegistration = BackgroundTaskRegistration.AllTasks
                                            .Where(o => o.Value.Name == SkycapMailSyncTaskName.ToString())
                                            .Select(o => o.Value)
                                            .SingleOrDefault();

                // Return background task registration
                return (backgroundTaskRegistration != null);
            }
            catch(Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// Registers the background task.
        /// </summary>
        public async static void Register()
        {
            try
            {
                // The background task builder
                IBackgroundTaskRegistration backgroundTaskRegistration = null;

                try
                {
                    // Check if we have background access
                    BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
                    LogFile.Instance.LogInformation("", "", string.Format("Lock Screen Access: {0}.", backgroundAccessStatus));
                }
                catch
                {
                    LogFile.Instance.LogInformation("", "", "Lock Screen Access.");
                }

                // If SkycapMailSyncNowPeriodically is not registered
                if (!SyncMailBackgroundTask.IsRegistered(out backgroundTaskRegistration))
                {
                    // Register SkycapMailSyncNowPeriodically
                    BackgroundTaskBuilder skycapMailSyncPeriodically = new BackgroundTaskBuilder();
                    skycapMailSyncPeriodically.Name = SkycapMailSyncTaskName;
                    skycapMailSyncPeriodically.TaskEntryPoint = typeof(SyncMailBackgroundTask).FullName;
                    skycapMailSyncPeriodically.SetTrigger(new TimeTrigger(15, false));
                    skycapMailSyncPeriodically.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                    skycapMailSyncPeriodically.Register();
                    LogFile.Instance.LogInformation("", "", "Registered Background Task.");
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Unregisters the background task.
        /// </summary>
        public static void Unregister()
        {
            try
            {
                // The background task builder
                IBackgroundTaskRegistration backgroundTaskRegistration = null;

                // Deregister SkycapMailSyncNowPeriodically
                if (SyncMailBackgroundTask.IsRegistered(out backgroundTaskRegistration))
                {
                    backgroundTaskRegistration.Unregister(true);
                    LogFile.Instance.LogInformation("", "", "Unregistered Background Task.");
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Performs the work of a background task. The system calls this method when the associated background task has been triggered.
        /// </summary>
        /// <param name="taskInstance">An interface to an instance of the background task. The system creates this instance when the task has been triggered to run.</param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += taskInstance_Canceled;
            BackgroundTaskDeferral deferral = null;
            try
            {
                // Get the background task deferral
                deferral = taskInstance.GetDeferral();
                LogFile.Instance.LogInformation("", "", string.Format("Run: IsUISuspended - {0}, IsTaskRunning - {1}.", StorageSettings.IsUISuspended, StorageSettings.IsTaskRunning));

                // If the ui is suspended
                if (StorageSettings.IsUISuspended)
                {
                    // Mark background task as completed
                    StorageSettings.IsTaskRunning = true;
                    LogFile.Instance.LogInformation(this.GetType().Name, "", "Background Task Started.");

                    // Create the cancellation token source
                    _cancellationTokenSource = new CancellationTokenSource();

                    // Create the mail clients list
                    _mailClients = new List<MailClient>();

                    // Loop through each setting
                    await StorageSettings.InitialiseAsync();
                    Parallel.ForEach(StorageSettings.AccountSettingsDataDictionary.Values, async(accountSettingsData) =>
                    {
                        // Create the mail client
                        MailClient mailClient = await MailClient.GetMailClient(accountSettingsData);
                        mailClient.UpdatedMessage += mailClient_UpdatedMessage;
                        mailClient.DeletedMessage += mailClient_DeletedMessage;
                        mailClient.DownloadedMessage += mailClient_DownloadedMessage;

                        // Login
                        if (mailClient.Login().IsSuccessfull)
                        {
                            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, this.GetType().Name, "Logged in.");

                            // Download unread messages
                            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, this.GetType().Name, "Download started...");
                            _mailClients.Add(mailClient);
                            await mailClient.DownloadUnreadMessagesTask(_cancellationTokenSource.Token);
                            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, this.GetType().Name, "Download completed.");

                            // Logout
                            await mailClient.Logout();
                            LogFile.Instance.LogInformation(mailClient.AccountSettingsData.EmailAddress, this.GetType().Name, "Logged out.");

                            // Unsubscribe from events
                            mailClient.DownloadedMessage -= mailClient_DownloadedMessage;
                            mailClient.UpdatedMessage -= mailClient_UpdatedMessage;
                            mailClient.DeletedMessage -= mailClient_DeletedMessage;

                            // Remove mail client
                            _mailClients.Remove(mailClient);

                            // Save
                            await StorageSettings.SaveMailHeaderDictionary();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(this.GetType().Name, "", ex.ToString());
            }
            finally
            {
                // Remove the cancellation token
                _cancellationTokenSource = null;

                // Mark background task as completed
                StorageSettings.IsTaskRunning = false;
                LogFile.Instance.LogInformation(this.GetType().Name, "", "Background Task Completed.");
                deferral.Complete();
            }
        }

        /// <summary>
        /// Attaches a cancellation event handler to the background task instance.
        /// </summary>
        /// <param name="sender">The object that raised the event (IBackgroundTaskInstance).</param>
        /// <param name="reason">The event data (BackgroundTaskCancellationReason).</param>
        private async void taskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                if (_mailClients != null)
                {
                    // Loop through each mail client
                    foreach (MailClient mailClient in _mailClients)
                    {
                        // Disconnect the mail client
                        if (mailClient.State.HasFlag(MailClientState.Authenticated))
                            await mailClient.Logout();
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(this.GetType().Name, "", ex.ToString());
            }
            finally
            {
                // Mark background task as completed
                StorageSettings.IsTaskRunning = false;
                LogFile.Instance.LogInformation(this.GetType().Name, "", "Background Task Cancelled.");
            }
        }

        /// <summary>
        /// Occurs when roaming application data is synchronised.
        /// </summary>
        /// <param name="sender">The object that raised the event (ApplicationData).</param>
        /// <param name="args">The event data (object).</param>
        private void Current_DataChanged(ApplicationData sender, object args)
        {
            // If the ui is not suspended and the task is running
            if (_cancellationTokenSource != null
             && !StorageSettings.IsUISuspended
              && StorageSettings.IsTaskRunning)
            {
                try
                {
                    // Cancel the task
                    _cancellationTokenSource.Cancel();
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(this.GetType().Name, "", ex.ToString());
                }
                finally
                {
                    LogFile.Instance.LogInformation(this.GetType().Name, "", "Background Task Cancel Requested.");
                }
            }
        }

        /// <summary>
        /// Occurs when a mail client message update attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private async void mailClient_UpdatedMessage(object sender, MessageEventArgs e)
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
                    // Remove message from mail header
                    int mailHeaderIndex = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].IndexOf(mailHeader);
                    StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName][mailHeaderIndex].UpdateFlags(e.Message);
                }

                // Save
                await StorageSettings.SaveMailHeaderDictionary();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when a mail client message delete attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        private async void mailClient_DeletedMessage(object sender, DeleteMessageEventArgs e)
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
                    // Remove message from mail header
                    int mailHeaderIndex = StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].IndexOf(mailHeader);
                    StorageSettings.MailHeaderDictionary[mailClient.AccountSettingsData.EmailAddress][e.Mailbox.FullName].Remove(mailHeader);
                }

                // Save
                await StorageSettings.SaveMailHeaderDictionary();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }

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
                // Create the mail header
                MailHeader mailHeader = new MailHeader(mailClient.AccountSettingsData.EmailAddress, e.Message.Uid, e.Message, e.Mailbox);

                // Create the mail header
                StorageSettings.MailHeaderDictionary.AddOrUpdate(mailClient.AccountSettingsData, e.Mailbox, mailHeader);

                // Save
                await StorageSettings.SaveMailHeaderDictionary();

                // Get from, subject, body and context
                string from = (e.Message.From == null ? "(Unknown)" : e.Message.From.DisplayNameAlternate);
                string subject = e.Message.Subject;
                string body = e.Message.PlainText;

                // Only notify todays messages
                if (e.Message.Date >= DateTime.Today)
                {
                    string context = null;
                    try
                    {
                        // Create the toast context
                        context = IOUtil.Serialise(new EmailToastNotificationContext(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, e.Message.Uid));

                        // Show email notification
                        await Task.Delay(2000);
                        EmailToastNotification.Show(from, subject, body, context);
                    }
                    catch (Exception ex)
                    {
                        LogFile.Instance.LogError(this.GetType().Name, "", ex.ToString());
                    }
                }

                // If this is an unread email
                if (!mailHeader.IsSeen)
                {
                    // Update tile if today
                    if (mailHeader.Date >= DateTime.Today)
                    {
                        // Update tile
                        EmailTileNotification.UpdateTile(mailHeader.Uid, from, subject, body);
                    }

                    // Update unread email count
                    int unreadEmailsCount = StorageSettings.MailHeaderDictionary.AllUnreadEmailCount;

                    // Update badge if we haven't reached the limit
                    if (unreadEmailsCount <= 100)
                        EmailBadgeNotification.UpdateBadge(unreadEmailsCount);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(mailClient.AccountSettingsData.EmailAddress, e.Mailbox.FullName, ex.ToString());
            }
        }
    }
}
