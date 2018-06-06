using System;
using Skycap.Net.Common;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Settings
{
    /// <summary>
    /// The privacy settings policy.
    /// </summary>
    public sealed partial class PrivacySettingsControl : UserControl
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
        /// Initialises a new instance of the Skycap.Settings.PrivacySettingsControl class.
        /// </summary>
        public PrivacySettingsControl()
        {
            this.InitializeComponent();
            this.Loaded += PrivacySettingsControl_Loaded;
        }

        /// <summary>
        /// Occurs when the PrivacySettingsControl has been constructed and added to the object tree.
        /// </summary>
        /// <param name="sender">The object that raised the event (PrivacySettingsControl).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private async void PrivacySettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFolder dataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Data");
                StorageFile privacyPolicyFile = await dataFolder.GetFileAsync("Privacy Policy.rtf");
                string content = await FileIO.ReadTextAsync(privacyPolicyFile);
                rtbPrivacyPolicy.Document.SetText(TextSetOptions.FormatRtf, content);
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
            }
        }

        /// <summary>
        /// Occurs when the charm flyout visibility changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (CharmFlyout).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void flyout_IsOpenChanged(object sender, EventArgs e)
        {
            // Raise appropriate events for when the flyout is opened and closed
            if (cfPrivacyPolicy.IsOpen)
                if (Opened != null) Opened(this, EventArgs.Empty);
            else
                if (Closed != null) Closed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Shows the privacy policy settings.
        /// </summary>
        public void Show()
        {
            cfPrivacyPolicy.IsOpen = true;
        }
    }
}
