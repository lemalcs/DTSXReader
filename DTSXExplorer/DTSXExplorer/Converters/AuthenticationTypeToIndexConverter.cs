using System;
using System.Globalization;

namespace DTSXExplorer
{
    /// <summary>
    /// Returns an <see cref="int"/> value (used as index) based on an <see cref="AuthenticationType"/> value and vice versa.
    /// </summary>
    public class AuthenticationTypeToIndexConverter : BaseValueConverter<AuthenticationTypeToIndexConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(AuthenticationType)value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AuthenticationType)(int)value;
        }
    }
}
