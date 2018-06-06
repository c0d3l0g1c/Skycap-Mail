using System;

using Skycap.Net.Common;

using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Skycap.Notifications
{
    /// <summary>
    /// Represents an email badge notification.
    /// </summary>
    public static class EmailBadgeNotification
    {
        /// <summary>
        /// The badge updater.
        /// </summary>
        private static BadgeUpdater _badgeUpdater;

        /// <summary>
        /// Initialises a new instance of the Skycap.Notifications.EmailBadgeNotification class.
        /// </summary>
        static EmailBadgeNotification()
        { 
            // Initialise local variables
            if (_badgeUpdater == null)
                _badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
        }

        /// <summary>
        /// Updates the badge with the number of unread emails.
        /// </summary>
        /// <param name="number">The number of unread emails.</param>
        public static void UpdateBadge(int number)
        {
            try
            {
                // create a string with the badge template xml
                string badgeXmlString = string.Format("<badge value='{0}'/>", number);
                XmlDocument badgeDOM = new XmlDocument();

                // Create a DOM
                badgeDOM.LoadXml(badgeXmlString);

                // Load the xml string into the DOM, catching any invalid xml characters 
                BadgeNotification badge = new BadgeNotification(badgeDOM);

                // Create a badge notification
                _badgeUpdater.Update(badge);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }
    }
}
