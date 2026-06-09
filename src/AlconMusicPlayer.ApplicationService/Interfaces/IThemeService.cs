namespace AlconMusicPlayer.ApplicationService.Interfaces
{
    /// <summary>
    /// Manages application themes and theme switching at runtime.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>Gets the current theme name (e.g., "Light", "Dark")</summary>
        string CurrentTheme { get; }

        /// <summary>Gets all available themes</summary>
        IReadOnlyList<string> AvailableThemes { get; }

        /// <summary>Changes the application theme at runtime</summary>
        void SetTheme(string themeName);

        /// <summary>Raised when theme is changed</summary>
        event EventHandler? ThemeChanged;
    }
}
