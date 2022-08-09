using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TwitterCommander.Helper;

public class BooleanToBrushConverter : IValueConverter
{
    public bool IsReversed { get; set; }

    public SolidColorBrush TrueColor { get; set; } = new SolidColorBrush(Colors.Blue);
    public SolidColorBrush FalseColor { get; set; } = new SolidColorBrush(Colors.BlueViolet);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        if (this.IsReversed)
        {
            val = !val;
        }

        if (val)
        {
            return TrueColor;
        }

        return FalseColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}