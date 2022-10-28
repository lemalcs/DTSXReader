using System;
using System.Globalization;
using System.Windows;

namespace DTSXExplorer
{
    /// <summary>
    /// Returns a <see cref="Visibility.Visible"/> value when source value is true otherwise <see cref="Visibility.Hidden"/>.
    /// </summary>
    public class BoolToVisibilityConverter : BaseValueConverter<BoolToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
