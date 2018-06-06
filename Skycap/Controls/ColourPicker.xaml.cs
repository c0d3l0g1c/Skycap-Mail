using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Skycap.Net.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    /// Represents a control to pick a colour.
    /// </summary>
    public sealed partial class ColourPicker : UserControl
    {
        /// <summary>
        /// Occurs when the currently selected colour changes. 
        /// </summary>
        public event EventHandler ColourSelectionChanged;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.ColourPicker class.
        /// </summary>
        public ColourPicker()
        {
            this.InitializeComponent();

            // Fill the list of colours
            var colors = typeof(Colors).GetTypeInfo().DeclaredProperties;
            this.DataContext = colors.Select(o => o.Name);
        }

        /// <summary>
        /// Gets the selected colour.
        /// </summary>
        public Color SelectedColour
        {
            get
            {
                // If a colour was selected
                if (lvColour.SelectedIndex != -1)
                {
                    try
                    {
                        // Try to get the selected colour
                        string colourName = (string)lvColour.SelectedItem;
                        PropertyInfo colourProperty = typeof(Colors).GetTypeInfo().GetDeclaredProperty(colourName);
                        return (Color)colourProperty.GetValue(null);
                    }
                    catch(Exception ex)
                    {
                        // Return Black if no colour is selected
                        LogFile.Instance.LogError("", "", ex.ToString());
                        return Colors.Black;
                    }
                }
                // Return Black if no colour is selected
                return Colors.Black;
            }
            set
            {
                try
                {
                    // Try to get the selected colour
                    lvColour.SelectedItem = lvColour.Items.Cast<PropertyInfo>().First(o => (Color)o.GetValue(null) == value);
                }
                catch(Exception ex)
                {
                    LogFile.Instance.LogError("", "", ex.ToString());
                }
            }
        }

        /// <summary>
        /// Show popup on screen.
        /// </summary>
        /// <param name="horizontalOffset">The distance between the left side of the application window and the left side of the popup.</param>
        /// <param name="verticalOffset">The distance between the top of the application window and the top of the popup.</param>
        public void Show(double horizontalOffset, double verticalOffset)
        {
            pColourPicker.HorizontalOffset = horizontalOffset;
            pColourPicker.VerticalOffset = verticalOffset;
            pColourPicker.IsOpen = true;
        }

        /// <summary>
        /// Hide popup on screen.
        /// </summary>
        public void Hide()
        {
            pColourPicker.IsOpen = false;
        }

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListView).</param>
        /// <param name="e">The event data (SelectionChangedEventArgs).</param>
        private void lvColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Raise the ColourSelectionChanged event
            if (ColourSelectionChanged != null)
                ColourSelectionChanged(this, EventArgs.Empty);

            // Hide the colour picker
            Hide();
        }
    }
}
