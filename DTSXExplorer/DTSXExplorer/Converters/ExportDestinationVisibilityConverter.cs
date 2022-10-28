using System;
using System.Globalization;
using System.Windows;

namespace DTSXExplorer
{
    /// <summary>
    /// Returns <see cref="Visibility.Visible"/> if source value (<see cref="ExportDestination"/>) 
    /// is the same as the parameter sent by current control, otherwise returns <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public class ExportDestinationVisibilityConverter : BaseValueConverter<ExportDestinationVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(ExportDestination), parameter.ToString()).Equals((ExportDestination)value) ?
                    Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
