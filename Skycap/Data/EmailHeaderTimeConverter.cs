using System;
using System.Globalization;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Skycap.Data
{
    /// <summary>
    /// Converts a date to a friendly date.
    /// </summary>
    public class EmailHeaderTimeConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="language">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime date = (DateTime)value;
            return date.ToString("H:mm tt");
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="language">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = value as string;
            strValue = DateTime.Now.ToString("yyyy-MM-dd ") + strValue; 
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
                return resultDateTime;
            return DependencyProperty.UnsetValue;
        }
    }
}
