using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IThemeService _themeService;
        private string _selectedTheme;

        public ObservableCollection<string> AvailableThemes { get; }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (string.Equals(_selectedTheme, value, StringComparison.Ordinal))
                    return;

                SetProperty(ref _selectedTheme, value);
                _themeService.SetTheme(value);
            }
        }

        public ICommand ResetCommand { get; }

        public SettingsViewModel(IThemeService themeService)
        {
            _themeService = themeService;
            _selectedTheme = _themeService.CurrentTheme;
            AvailableThemes = new(_themeService.AvailableThemes);

            ResetCommand = new RelayCommand(() => SelectedTheme = "Light");
        }
    }
}
