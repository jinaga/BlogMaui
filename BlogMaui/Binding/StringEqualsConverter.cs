using System.Globalization;

namespace BlogMaui.Binding
{
    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean.");
            }

            if (parameter is string parameterString)
            {
                return value?.ToString() == parameterString;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("The target must be a string.");
            }

            if (value is bool valueBool)
            {
                if (parameter is string parameterString)
                {
                    return valueBool ? parameterString : string.Empty;
                }
            }
            return string.Empty;
        }
    }
}
