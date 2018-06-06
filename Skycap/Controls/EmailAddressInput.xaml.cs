using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Callisto.Controls;

using Skycap.Net.Common;
using Skycap.Pages;

using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Skycap.Controls
{
    /// <summary>
    /// Represents an email address rich edit box control.
    /// </summary>
    public sealed partial class EmailAddressInput : UserControl
    {
        /// <summary>
        /// The email addresses.
        /// </summary>
        private ObservableCollection<FormattedEmailAddress> _emailAddresses;
        /// <summary>
        /// The scroll viewer.
        /// </summary>
        private ScrollViewer _scrollViewer;
        /// <summary>
        /// The email address text box.
        /// </summary>
        private WatermarkTextBox _textBox;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.EmailAddressInput class.
        /// </summary>
        public EmailAddressInput()
        {
            this.InitializeComponent();

            // Initialise local variables
            _emailAddresses = new ObservableCollection<FormattedEmailAddress>();
            _emailAddresses.Add(new FormattedEmailAddress("mail@skycap.com") { ReadOnlyMode = Visibility.Collapsed });
            icEmailAddress.DataContext = _emailAddresses;
        }

        /// <summary>
        /// Gets the email addresses.
        /// </summary>
        public ObservableCollection<FormattedEmailAddress> EmailAddresses
        {
            get
            {
                return _emailAddresses;
            }
        }

        /// <summary>
        /// Occurs when this control gets focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.FocusState == FocusState.Programmatic)
            {
                if (_textBox != null)
                    _textBox.Focus(FocusState.Programmatic);
            }
        }

        /// <summary>
        /// Occurs when the ScrollViewer has been constructed and added to the object tree.
        /// </summary>
        /// <param name="sender">The object that raised the event (ScrollViewer).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = (ScrollViewer)sender;
        }

        /// <summary>
        /// Occurs when a keyboard key is pressed while the Email Address text box has focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (KeyRoutedEventArgs).</param>
        private void tbEmailAddress_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Get the email address textbox
            _textBox = (WatermarkTextBox)sender;

            // Scroll to end as text is input
            _scrollViewer.UpdateLayout();
            _scrollViewer.ScrollToVerticalOffset(_scrollViewer.ActualHeight * 2);

            // If email is valid - handle event
            if (IsValidEmail(e.Key))
                e.Handled = true;
        }

        /// <summary>
        /// Occurs when the Remove button is clicked.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            Button button = (Button)sender;
            // Get email address from button tag
            FormattedEmailAddress emailAddress = (FormattedEmailAddress)button.Tag;
            // Remove email address
            _emailAddresses.Remove(emailAddress);
        }

        /// <summary>
        /// Occurs when the email address textbox looses focus.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void WatermarkTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Get the email address textbox
            _textBox = (WatermarkTextBox)sender;
            // Check if email is valid
            IsValidEmail(null);
            // Close the popup
            CloseSuggestions();
        }

        /// <summary>
        /// Occurs when content changes in the email address textbox.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void WatermarkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the email address textbox
            _textBox = (WatermarkTextBox)sender;

            // If text has been entered
            if (!string.IsNullOrWhiteSpace(_textBox.Text))
            {
                // Get matching email addresses
                IEnumerable<EmailAddress> foundEmailAddresses = ComposeMailPage.Current.StoredEmailAddresses.Where(o => ((!string.IsNullOrEmpty(o.DisplayName)
                                                                                                                      && o.DisplayName.ToLower().Contains(_textBox.Text.ToLower())))
                                                                                                                      || o.Address.ToLower().Contains(_textBox.Text.ToLower()));

                // If any matches are found
                if (foundEmailAddresses.Count() > 0)
                {
                    // Position the popup and display it
                    pSuggestions.VerticalOffset = icEmailAddress.ActualHeight + 1;
                    pSuggestions.IsOpen = true;
                    lvEmailAddress.ItemsSource = foundEmailAddresses;
                    lvEmailAddress.Visibility = Visibility.Visible;
                    _textBox.Focus(FocusState.Programmatic);
                }
                // Else if there are not matches
                else
                    // Close the popup
                    CloseSuggestions();
            }
        }

        /// <summary>
        /// Occurs when the currently selected email address changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (TextBox).</param>
        /// <param name="e">The event data (RoutedEventArgs).</param>
        private void lvEmailAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If a selection was made
            if (lvEmailAddress.SelectedItem != null)
            {
                // Add the selected email address
                _emailAddresses.Insert(_emailAddresses.Count - 1, new FormattedEmailAddress((EmailAddress)lvEmailAddress.SelectedItem));
                // Clear the email address textbox
                _textBox.Text = string.Empty;
                // Close the popup
                CloseSuggestions();
            }
        }

        /// <summary>
        /// Closes the suggestions.
        /// </summary>
        private void CloseSuggestions()
        {
            // Close the popup
            pSuggestions.IsOpen = false;
            lvEmailAddress.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Determines if the text entered is a valid email address.
        /// </summary>
        /// <param name="key">The key that was pressed that initiated this.</param>
        /// <returns>true, if is valid email; otherwise, false.</returns>
        private bool IsValidEmail(VirtualKey? key)
        {
            // If a semi colon was pressed
            if (!key.HasValue
             || (int)key == 186
             || key == VirtualKey.Enter)
            {
                if (EmailAddress.IsValid(_textBox.Text.Trim()))
                {
                    // Add to the email address list
                    _emailAddresses.Insert(_emailAddresses.Count - 1, new FormattedEmailAddress(_textBox.Text.Trim()));
                    _textBox.Text = string.Empty;
                    return true;
                }
                else if (EmailAddresses.Count == 1)
                {
                    // Set border and brush to red
                    _textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    _textBox.BorderThickness = new Thickness(1);
                }
                return false;
            }
            else if (_textBox.BorderBrush != null)
            {
                // Set border and brush to red
                _textBox.BorderBrush = null;
                _textBox.BorderThickness = new Thickness(0);
            }
            return false;
        }
    }

    /// <summary>
    /// Represents the ObservableCollection<EmailAddress> extension methods.
    /// </summary>
    public static class ObservableCollectionEmailAddressExtensions
    {
        /// <summary>
        /// Adds an email address to the end of the ObservableCollection<EmailAddress>.
        /// </summary>
        /// <param name="instance">The ObservableCollection<EmailAddress> instance.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange(this ObservableCollection<FormattedEmailAddress> instance, IEnumerable<EmailAddress> items)
        {
            foreach (var item in items)
                instance.Insert(instance.Count - 1, new FormattedEmailAddress(item.Address, item.DisplayName));
        }
    }
}
