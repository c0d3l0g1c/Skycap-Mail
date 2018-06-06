using System;
using Skycap.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Skycap.Settings
{
    /// <summary>
    /// Represents the add accounts settings email service provider.
    /// </summary>
    public class AddAccountsSettingsEmailServiceProvider
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Settings.AddAccountsSettingsEmailServiceProvider class.
        /// </summary>
        /// <param name="emailServiceProvider">The email service provider.</param>
        /// <param name="imageSource">The image source.</param>
        public AddAccountsSettingsEmailServiceProvider(EmailServiceProvider emailServiceProvider, string imageSource)
            : base()
        { 
            // Initialise local variables
            EmailServiceProvider = emailServiceProvider;
            ImageSource = imageSource;
        }

        /// <summary>
        /// Gets or sets the email service provider.
        /// </summary>
        public EmailServiceProvider EmailServiceProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        public string ImageSource
        {
            get;
            set;
        }
    }
}
