using System;

using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Skycap.Common
{
    /// <summary>
    /// Converts a boolean value to a specified colour.
    /// </summary>
    public sealed class BooleanToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((string)parameter == "Attachment")
                return (value is bool && (bool)value) ? AppResources.TertiaryColourBrush : AppResources.QuadColourBrush;
            else if ((string)parameter == "Draft")
                return (value is bool && (bool)value) ? AppResources.TertiaryColourBrush : new SolidColorBrush(Colors.Orange);
            else if ((string)parameter == "Flagged")
                return (value is bool && (bool)value) ? AppResources.TertiaryColourBrush : AppResources.HepColourBrush;
            else
                return (value is bool && (bool)value) ? AppResources.TertiaryColourBrush : AppResources.PrimaryColourBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is SolidColorBrush && ((SolidColorBrush)value).Color == AppResources.TertiaryColourBrush.Color;
        }
    }
}
