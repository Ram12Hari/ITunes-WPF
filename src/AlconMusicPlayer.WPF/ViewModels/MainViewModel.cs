using AlconMusicPlayer.WPF.ViewModels.Base;
using System.ComponentModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly LibraryViewModel _libraryViewModel;
    private readonly ArtistsViewModel _artistsViewModel;
    private readonly AlbumsViewModel _albumsViewModel;
    private readonly PlaylistsViewModel _playlistsViewModel;
    private readonly PlaylistViewModel _playlistViewModel;
    private readonly NowPlayingViewModel _nowPlayingViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public NowPlayingViewModel NowPlayingViewModel => _nowPlayingViewModel;

    public string SearchText
    {
        get => _libraryViewModel.SearchText;
        set
        {
            _libraryViewModel.SearchText = value;
            OnPropertyChanged();
        }
    }

    public ICommand ClearSearchCommand => _libraryViewModel.ClearSearchCommand;

    private ViewModelBase _currentView;
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ICommand ShowLibraryCommand { get; }
    public ICommand ShowArtistsCommand { get; }
    public ICommand ShowAlbumsCommand { get; }
    public ICommand ShowPlaylistsCommand { get; }
    public ICommand ShowSettingsCommand { get; }

    public MainViewModel(
        LibraryViewModel libraryViewModel,
        ArtistsViewModel artistsViewModel,
        AlbumsViewModel albumsViewModel,
        PlaylistsViewModel playlistsViewModel,
        PlaylistViewModel playlistViewModel,
        NowPlayingViewModel nowPlayingViewModel,
        SettingsViewModel settingsViewModel)
    {
        _libraryViewModel = libraryViewModel;
        _artistsViewModel = artistsViewModel;
        _albumsViewModel = albumsViewModel;
        _playlistsViewModel = playlistsViewModel;
        _playlistViewModel = playlistViewModel;
        _nowPlayingViewModel = nowPlayingViewModel;
        _settingsViewModel = settingsViewModel;

        _currentView = _libraryViewModel;

        ShowLibraryCommand = new RelayCommand(() => CurrentView = _libraryViewModel);
        ShowArtistsCommand = new RelayCommand(() => CurrentView = _artistsViewModel);
        ShowAlbumsCommand = new RelayCommand(() => CurrentView = _albumsViewModel);
        ShowPlaylistsCommand = new RelayCommand(() =>
        {
            _playlistsViewModel.Reload();
            _playlistsViewModel.SelectedPlaylist = null;
            CurrentView = _playlistsViewModel;
        });
        ShowSettingsCommand = new RelayCommand(() => CurrentView = _settingsViewModel);

        _artistsViewModel.PropertyChanged += OnArtistSelectionChanged;
        _libraryViewModel.PropertyChanged += OnLibraryPropertyChanged;
        _albumsViewModel.PropertyChanged += OnAlbumSelectionChanged;
        _playlistsViewModel.PropertyChanged += OnPlaylistSelectionChanged;
        _playlistViewModel.BackRequested += OnPlaylistBackRequested;
    }

    private void OnArtistSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ArtistsViewModel.SelectedArtist)) return;
        if (_artistsViewModel.SelectedArtist == null) return;

        _libraryViewModel.FilterByArtist(_artistsViewModel.SelectedArtist);
        CurrentView = _libraryViewModel;
    }

    private void OnAlbumSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AlbumsViewModel.SelectedAlbum)) return;
        if (_albumsViewModel.SelectedAlbum == null) return;

        _libraryViewModel.FilterByAlbum(_albumsViewModel.SelectedAlbum);
        CurrentView = _libraryViewModel;
    }

    private void OnPlaylistSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PlaylistsViewModel.SelectedPlaylist)) return;
        if (_playlistsViewModel.SelectedPlaylist == null) return;

        _playlistViewModel.Load(_playlistsViewModel.SelectedPlaylist);
        CurrentView = _playlistViewModel;
    }

    private void OnPlaylistBackRequested()
    {
        _playlistsViewModel.Reload();
        _playlistsViewModel.SelectedPlaylist = null;
        CurrentView = _playlistsViewModel;
    }

    private void OnLibraryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LibraryViewModel.SearchText))
            OnPropertyChanged(nameof(SearchText));
    }
}
