using System;
using CharmFlyoutLibrary;

namespace Skycap.Settings
{
    /// <summary>
    /// The properties that a settings page must contain.
    /// </summary>
    public interface IFlyoutSettings
    {
        /// <summary>
        /// Gets the charm flyout settings.
        /// </summary>
        CharmFlyout FlyoutSettings
        {
            get;
        }
    }
}
