using System;
using System.Globalization;

namespace DTSXExplorer
{
    /// <summary>
    /// Converts an <see cref="ExportDestination"/> value to a <see cref="bool"/> value if the source value
    /// matches the parameter sent by current control.
    /// </summary>
    public class BoolToExportDestinationConverter : BaseValueConverter<BoolToExportDestinationConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(ExportDestination), parameter.ToString()).Equals((ExportDestination)value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(ExportDestination), parameter.ToString());
        }
    }
}
