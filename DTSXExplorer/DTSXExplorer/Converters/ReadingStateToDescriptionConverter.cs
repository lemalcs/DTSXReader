using System;
using System.Globalization;

namespace DTSXExplorer
{
    /// <summary>
    /// Returns a description based on the state of reading operation which is stored in a <see cref="bool"/> value.
    /// </summary>
    public class ReadingStateToDescriptionConverter : BaseValueConverter<ReadingStateToDescriptionConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value ? "Start reading" : "Stop reading";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
