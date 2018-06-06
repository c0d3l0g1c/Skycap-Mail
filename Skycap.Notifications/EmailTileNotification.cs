using System;
using System.Collections.Generic;
using System.Linq;
using Skycap.IO;
using Skycap.Net.Common;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Skycap.Notifications
{
    /// <summary>
    /// Represents the email tile notification.
    /// </summary>
    public static class EmailTileNotification
    {
        /// <summary>
        /// The tile updater.
        /// </summary>
        private static TileUpdater _tileUpdater;

        /// <summary>
        /// Initialises a new instance of the Skycap.Notifications.EmailTileNotification class.
        /// </summary>
        static EmailTileNotification()
        {
            // If tile updater is not initialised
            if (_tileUpdater == null)
            {
                _tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                _tileUpdater.EnableNotificationQueue(true);
            }
        }

        /// <summary>
        /// Updates the tile with the subject and body.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <param name="from">The from address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public static void UpdateTile(string uid, string from, string subject, string body)
        {
            try
            {
                // Make sure that the message has From, Subject and Body
                if (!string.IsNullOrEmpty(from)
                 && !string.IsNullOrEmpty(subject)
                 && !string.IsNullOrEmpty(body))
                {
                    // Get the tile template
                    XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideText01);

                    // Set From, Subject and Body
                    XmlNodeList textElements = tileXml.GetElementsByTagName("text");
                    textElements.Item(0).AppendChild(tileXml.CreateTextNode(from));
                    textElements.Item(1).AppendChild(tileXml.CreateTextNode(subject));
                    textElements.Item(2).AppendChild(tileXml.CreateTextNode(IOUtil.TrimBody(body)));

                    // Update the tile
                    TileNotification tile = new TileNotification(tileXml);
                    tile.Tag = uid;
                    _tileUpdater.Update(tile);
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Removes the tile from the schedule.
        /// </summary>
        /// <param name="uid">The uid.</param>
        public static void RemoveTile(string uid)
        {
            try
            {
                // Check if a tile has a tag with the specified uid
                ScheduledTileNotification scheduledTileNotification = _tileUpdater.GetScheduledTileNotifications().FirstOrDefault(o => o.Tag.Equals(uid, StringComparison.OrdinalIgnoreCase));
                // If a tile is found, remove it
                if (scheduledTileNotification != null)
                    _tileUpdater.RemoveFromSchedule(scheduledTileNotification);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Removes the tiles from the schedule.
        /// </summary>
        /// <param name="uids">The uids.</param>
        public static void RemoveTiles(IEnumerable<string> uids)
        {
            foreach (string uid in uids)
                RemoveTile(uid);
        }
    }
}
