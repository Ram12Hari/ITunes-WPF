using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlconMusicPlayer.WPF.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = (bool?)value ?? false;
        // If invert parameter is passed, invert the logic
        bool invert = parameter?.ToString() == "invert";
        bool shouldShow = invert ? !boolValue : boolValue;
        return shouldShow ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
