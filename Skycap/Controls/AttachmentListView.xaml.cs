using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skycap.Net.Common.Collections;
using Skycap.Net.Common.MessageParts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// The attachment list view mode.
    /// </summary>
    public enum AttachmentListViewMode
    { 
        /// <summary>
        /// Indicates read only mode.
        /// </summary>
        ReadOnly,
        /// <summary>
        /// Indicates edit mode.
        /// </summary>
        Edit
    }

    /// <summary>
    /// Represents the attachment list view control.
    /// </summary>
    public sealed partial class AttachmentListView : UserControl
    {
        /// <summary>
        /// The attachment that was right tapped.
        /// </summary>
        private ListViewItem attachmentRightTapped;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.AttachmentListView class.
        /// </summary>
        public AttachmentListView()
        {
            this.InitializeComponent();
            Mode = AttachmentListViewMode.ReadOnly;
        }

        /// <summary>
        /// Gets or sets the attachment list view mode.
        /// </summary>
        public AttachmentListViewMode Mode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an object source used to generate the content of the ItemsControl.
        /// </summary>
        public object ItemsSource
        {
            get
            {
                return lvAttachments.ItemsSource;
            }
            set
            {
                lvAttachments.ItemsSource = value;
            }
        }

        /// <summary>
        /// Occurs when a right-tap input stimulus happens while the pointer is over the attachments list.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListView).</param>
        /// <param name="e">The event data (RightTappedRoutedEventArgs).</param>
        private async void MessageAttachments_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Don't show the appbar
            e.Handled = true;
            if (MainPage.Current.BottomAppBar.IsOpen)
                MainPage.Current.BottomAppBar.IsOpen = false;

            // Get the attachment
            Attachment attachment = (Attachment)((FrameworkElement)e.OriginalSource).DataContext;
            StorageFile newStorageFile = await StorageFile.GetFileFromPathAsync(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, attachment.TransferFilename));

            // Create a menu and add commands specifying a callback delegate for each.
            // Since command delegates are unique, no need to specify command Ids.
            PopupMenu menu = new PopupMenu();
            menu.Commands.Add(new UICommand("Open", async (command) =>
            {
                await Launcher.LaunchFileAsync(newStorageFile, new LauncherOptions() { TreatAsUntrusted = false });
            }));
            menu.Commands.Add(new UICommand("Open With", async (command) =>
            {
                await Launcher.LaunchFileAsync(newStorageFile, new LauncherOptions() { DisplayApplicationPicker = true });
            }));
            if (Mode == AttachmentListViewMode.ReadOnly)
            {
                menu.Commands.Add(new UICommand("Save", async (command) =>
                {
                    FileSavePicker fileSavePicker = new FileSavePicker();
                    fileSavePicker.FileTypeChoices.Add("File", new List<string>() { attachment.TransferFilenameExtension });
                    fileSavePicker.SuggestedFileName = newStorageFile.Name;
                    fileSavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    StorageFile saveStorageFile = await fileSavePicker.PickSaveFileAsync();
                    if (saveStorageFile != null)
                        await newStorageFile.CopyAndReplaceAsync(saveStorageFile);
                }));
            }
            else
            {
                menu.Commands.Add(new UICommand("Remove", (command) =>
                {
                    AttachmentCollection attachmentCollection = (AttachmentCollection)ItemsSource;
                    attachmentCollection.Remove(attachment);
                    ItemsSource = null;
                    ItemsSource = attachmentCollection;
                }));
            }

            // Show the menu
            await menu.ShowForSelectionAsync(App.GetElementRect(attachmentRightTapped), Placement.Below);
        }

        /// <summary>
        /// Occurs when the pointer device initiates a Press action within the Attachments list view.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListView).</param>
        /// <param name="e">The event data (PointerRoutedEventArgs).</param>
        private void MessageAttachments_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Get controls at current point
            IEnumerable<UIElement> elements = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(null).Position, lvAttachments);
            // Find the list view item
            foreach (UIElement element in elements)
            {
                if (element is ListViewItem)
                {
                    attachmentRightTapped = (ListViewItem)element;
                    break;
                }
            }
        }
    }
}
