using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TwitterCommander.Helper;

public class EmptyStringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value == null)
            return Visibility.Collapsed;
        if(value is string s && String.IsNullOrEmpty(s))
            return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}