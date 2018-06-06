using System;

using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Notifications.ToastContent;

using Windows.UI.Notifications;

namespace Skycap.Notifications
{
    /// <summary>
    /// Represents an email toast notification.
    /// </summary>
    public static class EmailToastNotification
    {
        /// <summary>
        /// The email toast notification.
        /// </summary>
        private static IToastText04 _notification;
        /// <summary>
        /// The toast notifier.
        /// </summary>
        private static ToastNotifier _toastNotifier;

        /// <summary>
        /// Initialises a new instance of the Skycap.Notifications.EmailToastNotification class.
        /// </summary>
        static EmailToastNotification()
        {
            // Initialise local variables
            if (_notification == null)
                _notification = ToastContentFactory.CreateToastText04();

            if (_toastNotifier == null)
                _toastNotifier = ToastNotificationManager.CreateToastNotifier();
        }

        /// <summary>
        /// Gets a value that tells you whether there is an app, user, or system block that prevents the display of a toast notification.
        /// </summary>
        public static bool Setting
        {
            get
            {
                return (_toastNotifier.Setting == NotificationSetting.Enabled ? true : false);
            }
        }

        /// <summary>
        /// Displays the specified email as a toast notification.
        /// </summary>
        /// <param name="from">The from address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="context">The context.</param>
        public static void Show(string from, string subject, string body, string context)
        {
            try
            {
                // Set the subject and content
                _notification.Launch = context;
                _notification.TextHeading.Text = from;
                _notification.TextBody1.Text = subject;
                _notification.TextBody2.Text = IOUtil.TrimBody(body);

                // Show the notification
                ToastNotification toast = _notification.CreateNotification();
                _toastNotifier.Show(toast);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }
    }
}
