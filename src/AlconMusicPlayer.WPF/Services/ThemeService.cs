using AlconMusicPlayer.ApplicationService.Interfaces;
using System.Windows;

namespace AlconMusicPlayer.WPF.Services
{
    public class ThemeService : IThemeService
    {
        private string _currentTheme = "Light";

        public string CurrentTheme => _currentTheme;

        public IReadOnlyList<string> AvailableThemes => new[] { "Light" };

        public event EventHandler? ThemeChanged;

        public ThemeService()
        {
        }

        public void SetTheme(string themeName)
        {
            if (!AvailableThemes.Contains(themeName))
                throw new ArgumentException($"Theme '{themeName}' not found. Available: {string.Join(", ", AvailableThemes)}");

            if (_currentTheme == themeName)
                return;

            _currentTheme = themeName;
            ApplyTheme(themeName);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyTheme(string themeName)
        {
            var app = Application.Current;
            if (app == null) return;

            var themePath = $"pack://application:,,,/Resources/Themes/{themeName}Theme.xaml";
            var themeDictionary = new ResourceDictionary { Source = new Uri(themePath) };

            for (var i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var dictionary = app.Resources.MergedDictionaries[i];
                var source = dictionary.Source?.OriginalString;
                if (source != null && source.Contains("/Resources/Themes/", StringComparison.OrdinalIgnoreCase))
                {
                    app.Resources.MergedDictionaries.RemoveAt(i);
                }
            }

            app.Resources.MergedDictionaries.Insert(0, themeDictionary);
        }
    }
}