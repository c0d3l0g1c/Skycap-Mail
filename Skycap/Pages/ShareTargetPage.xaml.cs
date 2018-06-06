using System;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

// The Share Target Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234241

namespace Skycap.Pages
{
    // TODO: Edit the manifest to enable use as a share target
    //
    // The package manifest could not be automatically updated.  Open the package manifest
    // file and ensure that support for activation as a share target is enabled.

    // TODO: Respond to activation as a share target
    //
    // The following code could not be automatically added to your application subclass,
    // either because the appropriate class could not be located or because a method with
    // the same name already exists.  Ensure that appropriate code deals with activation
    // by displaying a flyout appropriate for receiving a shared item.
    //
    // /// <summary>
    // /// Invoked when the application is activated as the target of a sharing operation.
    // /// </summary>
    // /// <param name="args">Details about the activation request.</param>
    // protected override void OnShareTargetActivated(Windows.ApplicationModel.Activation.ShareTargetActivatedEventArgs args)
    // {
    //     var shareTargetPage = new Skycap.Pages.ShareTargetPage();
    //     shareTargetPage.Activate(args);
    // }
    /// <summary>
    /// This page allows other applications to share content through this application.
    /// </summary>
    public sealed partial class ShareTargetPage : Skycap.Net.Common.LayoutAwarePage
    {
        /// <summary>
        /// Provides a channel to communicate with Windows about the sharing operation.
        /// </summary>
        private Windows.ApplicationModel.DataTransfer.ShareTarget.ShareOperation _shareOperation;

        public ShareTargetPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when another application wants to share content through this application.
        /// </summary>
        /// <param name="args">Activation data used to coordinate the process with Windows.</param>
        public async void Activate(ShareTargetActivatedEventArgs args)
        {
            this._shareOperation = args.ShareOperation;

            // Communicate metadata about the shared content through the view model
            var shareProperties = this._shareOperation.Data.Properties;
            var thumbnailImage = new BitmapImage();
            this.DefaultViewModel["Title"] = shareProperties.Title;
            this.DefaultViewModel["Description"] = shareProperties.Description;
            this.DefaultViewModel["Image"] = thumbnailImage;
            this.DefaultViewModel["Sharing"] = false;
            this.DefaultViewModel["ShowImage"] = false;
            this.DefaultViewModel["Comment"] = String.Empty;
            this.DefaultViewModel["SupportsComment"] = true;
            Window.Current.Content = this;
            Window.Current.Activate();

            // Update the shared content's thumbnail image in the background
            if (shareProperties.Thumbnail != null)
            {
                var stream = await shareProperties.Thumbnail.OpenReadAsync();
                thumbnailImage.SetSource(stream);
                this.DefaultViewModel["ShowImage"] = true;
            }
        }

        /// <summary>
        /// Invoked when the user clicks the Share button.
        /// </summary>
        /// <param name="sender">Instance of Button used to initiate sharing.</param>
        /// <param name="e">Event data describing how the button was clicked.</param>
        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Sharing"] = true;
            this._shareOperation.ReportStarted();

            // TODO: Perform work appropriate to your sharing scenario using
            //       this._shareOperation.Data, typically with additional information captured
            //       through custom user interface elements added to this page such as 
            //       this.DefaultViewModel["Comment"]

            this._shareOperation.ReportCompleted();
        }
    }
}
