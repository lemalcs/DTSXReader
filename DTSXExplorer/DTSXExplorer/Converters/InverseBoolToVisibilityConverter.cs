using System;
using System.Globalization;
using System.Windows;

namespace DTSXExplorer
{
    /// <summary>
    /// Returns a <see cref="Visibility.Visible"/> value when source (<see cref="bool"/>) is true otherwise returns <see cref="Visibility.Hidden"/>.
    /// </summary>
    public class InverseBoolToVisibilityConverter : BaseValueConverter<InverseBoolToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
