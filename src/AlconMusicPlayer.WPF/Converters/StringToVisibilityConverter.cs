
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlconMusicPlayer.WPF.Converters;

/// <summary>
/// Returns Visible when the string is non-empty, Collapsed when null or empty.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}