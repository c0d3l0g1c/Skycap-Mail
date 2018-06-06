using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skycap.FontEnumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// Represents a control to pick a font and size.
    /// </summary>
    public sealed partial class FontPicker : UserControl
    {
        /// <summary>
        /// Occurs when the currently selected font family changes. 
        /// </summary>
        public event EventHandler FontFamilySelectionChanged;
        /// <summary>
        /// Occurs when the currently selected font size changes. 
        /// </summary>
        public event EventHandler FontSizeSelectionChanged;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.FontPicker class.
        /// </summary>
        public FontPicker()
        {
            this.InitializeComponent();

            // Create the fonts and sizes
            FontEnumerator fontEnumerator = new FontEnumerator();
            KeyValuePair<IEnumerable<FontFamily>, float[]> dataContext = new KeyValuePair<IEnumerable<FontFamily>, float[]>
            (
                fontEnumerator.ListSystemFonts().OrderBy(o => o).Select(o => new FontFamily(o)),
                new float[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 }
            );

            // Bind the data
            this.DataContext = dataContext;
        }

        /// <summary>
        /// Gets the selected font family.
        /// </summary>
        public FontFamily SelectedFontFamily
        {
            get
            {
                return (FontFamily)lvFontFamily.SelectedItem;
            }
            set
            {
                lvFontFamily.SelectedItem = value;
            }
        }

        /// <summary>
        /// Gets the selected font size.
        /// </summary>
        public float SelectedFontSize
        {
            get
            {
                return (float)lvFontSize.SelectedItem;
            }
            set
            {
                lvFontSize.SelectedItem = value;
            }
        }

        /// <summary>
        /// Show popup on screen.
        /// </summary>
        /// <param name="horizontalOffset">The distance between the left side of the application window and the left side of the popup.</param>
        /// <param name="verticalOffset">The distance between the top of the application window and the top of the popup.</param>
        public void Show(double horizontalOffset, double verticalOffset)
        {
            pFontPicker.HorizontalOffset = horizontalOffset;
            pFontPicker.VerticalOffset = verticalOffset;
            pFontPicker.IsOpen = true;
        }

        /// <summary>
        /// Hide popup on screen.
        /// </summary>
        public void Hide()
        {
            pFontPicker.IsOpen = false;
        }

        /// <summary>
        /// Occurs when the currently selected font family changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListView).</param>
        /// <param name="e">The event data (SelectionChangedEventArgs).</param>
        private void lvFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Raise the FontFamilySelectionChanged event
            if (FontFamilySelectionChanged != null)
                FontFamilySelectionChanged(this, EventArgs.Empty);

            // Hide the font picker
            Hide();
        }

        /// <summary>
        /// Occurs when the currently selected font size changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (ListView).</param>
        /// <param name="e">The event data (SelectionChangedEventArgs).</param>
        private void lvFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Raise the FontSizeSelectionChanged event
            if (FontSizeSelectionChanged != null)
                FontSizeSelectionChanged(this, EventArgs.Empty);

            // Hide the font picker
            Hide();
        }
    }
}
