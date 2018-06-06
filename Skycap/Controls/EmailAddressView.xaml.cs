using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skycap.Net.Common;
using Skycap.Net.Common.Collections;
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
    /// Represents an email address item view.
    /// </summary>
    public sealed partial class EmailAddressView : UserControl
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.EmailAddressView class.
        /// </summary>
        public EmailAddressView()
            : base()
        {
            InitializeComponent();
        }
    }
}
