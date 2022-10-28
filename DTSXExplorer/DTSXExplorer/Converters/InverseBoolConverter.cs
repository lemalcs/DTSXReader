using System;
using System.Globalization;

namespace DTSXExplorer
{
    /// <summary>
    /// Returns the inverse of a <see cref="bool"/> value.
    /// </summary>
    public class InverseBoolConverter : BaseValueConverter<InverseBoolConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
