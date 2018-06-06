using System;
using System.Linq;

using Windows.UI.Xaml.Controls;

using Skycap.Data;
using System.Collections.Generic;
using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// Represents the email service radio button list.
    /// </summary>
    public sealed partial class EmailServiceRadioButtonList : UserControl
    {
        /// <summary>
        /// The email service provider.
        /// </summary>
        private EmailServiceProvider _emailServiceProvider;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.EmailServiceRadioButtonList class.
        /// </summary>
        public EmailServiceRadioButtonList()
        {
            this.InitializeComponent();

            // Builds the control
            BuildControl();
        }

        /// <summary>
        /// Gets or sets the email service provider.
        /// </summary>
        public EmailServiceProvider EmailServiceProvider
        {
            get
            {
                return _emailServiceProvider;
            }
            set
            {
                _emailServiceProvider = value;

                // Refresh the control
                RefreshControl();
            }
        }

        /// <summary>
        /// Gets or sets the dimensions by which the email services are displayed.
        /// </summary>
        public Orientation Orientation
        {
            get
            {
                return spLayout.Orientation;
            }
            set
            {
                spLayout.Orientation = value;
            }
        }

        /// <summary>
        /// Gets the selected item.
        /// </summary>
        public EmailService SelectedItem
        {
            get
            {
                EmailService emailService = EmailService.Pop;
                foreach (RadioButton radioButton in spLayout.Children)
                {
                    if (radioButton.IsChecked.Value)
                    {
                        emailService = (EmailService)Enum.Parse(typeof(EmailService), radioButton.Content.ToString());
                        break;
                    }
                }
                return emailService;
            }
            set
            {
                foreach (RadioButton radioButton in spLayout.Children)
                {
                    if (radioButton.Content.ToString() == value.ToString())
                    {
                        radioButton.IsChecked = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the control.
        /// </summary>
        private void BuildControl()
        {
            // Default layout
            spLayout.Orientation = Orientation.Horizontal;

            // Make sure it's empty
            spLayout.Children.Clear();

            // Gets the email services
            string[] emailServices = Enum.GetNames(typeof(EmailService)).ToArray();

            // Loop through list
            for (int i = 0; i < emailServices.Count(); i++)
            {
                // Create radio button
                RadioButton radioButton = new RadioButton();
                radioButton.Content = emailServices[i];
                radioButton.GroupName = "EmailService";

                // Reset radio button
                ResetRadioButton(radioButton, emailServices[i]);

                // Add radio button
                spLayout.Children.Add(radioButton);
            }
        }

        /// <summary>
        /// Refreshes the control.
        /// </summary>
        private void RefreshControl()
        {
            // Gets the email services
            string[] emailServices = Enum.GetNames(typeof(EmailService)).ToArray();

            // Loop through list
            for (int i = 0; i < emailServices.Count(); i++)
            {
                // Reset radio button
                ResetRadioButton((RadioButton)spLayout.Children[i], emailServices[i]);
            }
        }

        /// <summary>
        /// Resets a radio button to an appropriate state.
        /// </summary>
        /// <param name="radioButton">The radio button.</param>
        /// <param name="emailService">The email service.</param>
        private void ResetRadioButton(RadioButton radioButton, string emailService)
        {
            // Make sure radio button is enabled
            radioButton.IsEnabled = true;

            // Determine which email provider is set and remove email services accordingly
            switch (EmailServiceProvider)
            {
                // If Hotmail
                case EmailServiceProvider.Hotmail:
                    if (emailService == EmailService.Imap.ToString())
                        radioButton.IsEnabled = false;
                    else if (emailService == EmailService.Pop.ToString())
                        radioButton.IsChecked = true;
                    break;

                // If Gmail, Yahoo or Aol
                case EmailServiceProvider.Gmail:
                case EmailServiceProvider.Yahoo:
                case EmailServiceProvider.Aol:
                    if (emailService == EmailService.Imap.ToString())
                        radioButton.IsChecked = true;
                    break;

                // Anything else
                case Data.EmailServiceProvider.Other:
                    if (emailService == EmailService.Pop.ToString())
                        radioButton.IsChecked = true;
                    break;
            }
        }
    }
}
