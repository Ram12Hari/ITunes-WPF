
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.Dialogs;
using AlconMusicPlayer.WPF.ViewModels.Base;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicService;
    private readonly IPlaylistService _playlistService;
    private readonly CreatePlaylistUseCase _createPlaylistUseCase;
    private readonly AddTrackToPlaylistUseCase _addTrackUseCase;
    private readonly NowPlayingViewModel _nowPlayingViewModel;
    private readonly ILogger<LibraryViewModel> _logger;

    private List<Track> _allTracks = [];
    private Artist? _activeArtistFilter;
    private Album? _activeAlbumFilter;

    // --- Tracks ---
    private ObservableCollection<Track> _tracks = [];
    public ObservableCollection<Track> Tracks
    {
        get => _tracks;
        set => SetProperty(ref _tracks, value);
    }

    private Track? _selectedTrack;
    public Track? SelectedTrack
    {
        get => _selectedTrack;
        set
        {
            SetProperty(ref _selectedTrack, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    // --- Search ---
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); ApplyFilter(); }
    }

    // --- Filter label (shown when an artist/album filter is active) ---
    private string _activeFilterLabel = "";
    public string ActiveFilterLabel
    {
        get => _activeFilterLabel;
        set => SetProperty(ref _activeFilterLabel, value);
    }

    // --- Playlists (drives context menu submenu) ---
    public ObservableCollection<Playlist> AvailablePlaylists { get; } = [];

    // --- Error feedback ---
    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    // --- Commands ---
    public ICommand ClearSearchCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand AddToPlaylistCommand { get; }   // RelayCommand<Playlist>
    public ICommand AddToNewPlaylistCommand { get; } // opens dialog
    public ICommand PlayTrackCommand { get; } // RelayCommand<Track>

    public LibraryViewModel(
        IMusicLibraryService musicService,
        IPlaylistService playlistService,
        CreatePlaylistUseCase createPlaylistUseCase,
        AddTrackToPlaylistUseCase addTrackUseCase,
        NowPlayingViewModel nowPlayingViewModel,
        ILogger<LibraryViewModel> logger)
    {
        _musicService = musicService;
        _playlistService = playlistService;
        _createPlaylistUseCase = createPlaylistUseCase;
        _addTrackUseCase = addTrackUseCase;
        _nowPlayingViewModel = nowPlayingViewModel;
        _logger = logger;

        ClearSearchCommand = new RelayCommand(() => SearchText = "");
        ClearFilterCommand = new RelayCommand(ClearFilter, () => !string.IsNullOrEmpty(ActiveFilterLabel));
        AddToPlaylistCommand = new RelayCommand<Playlist>(AddSelectedTrackToPlaylist,
                                      p => SelectedTrack != null && p != null && AvailablePlaylists.Count > 0);
        AddToNewPlaylistCommand = new RelayCommand(AddSelectedTrackToNewPlaylist,
                                      () => SelectedTrack != null);
        PlayTrackCommand = new RelayCommand<Track>(PlayTrack,
                                       t => t != null);

        LoadTracks();
        RefreshPlaylists();
    }

    // Called by MainViewModel when user clicks an Artist in the sidebar
    public void FilterByArtist(Artist artist)
    {
        _activeArtistFilter = artist;
        _activeAlbumFilter = null;
        ActiveFilterLabel = $"Artist: {artist.Name}";
        SearchText = "";
        ApplyFilter();
    }

    // Called by MainViewModel when user clicks an Album in the sidebar
    public void FilterByAlbum(Album album)
    {
        _activeAlbumFilter = album;
        _activeArtistFilter = null;
        ActiveFilterLabel = $"Album: {album.Name}";
        SearchText = "";
        ApplyFilter();
    }

    public void RefreshPlaylists()
    {
        AvailablePlaylists.Clear();
        foreach (var p in _playlistService.GetAllPlaylists())
            AvailablePlaylists.Add(p);
    }

    private void LoadTracks()
    {
        _allTracks = _musicService.GetAllTracks().ToList();
        ApplyFilter();
    }

    private void ClearFilter()
    {
        _activeArtistFilter = null;
        _activeAlbumFilter = null;
        ActiveFilterLabel = "";
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<Track> result = _allTracks;

        // Artist/Album filter first
        if (_activeArtistFilter != null)
            result = result.Where(t => t.Artist.Id == _activeArtistFilter.Id);
        else if (_activeAlbumFilter != null)
            result = result.Where(t => t.Album.Id == _activeAlbumFilter.Id);

        // Then search text on top
        if (!string.IsNullOrWhiteSpace(SearchText))
            result = result.Where(t =>
                t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Artist.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Album.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Tracks = new ObservableCollection<Track>(result);
    }

    private void AddSelectedTrackToPlaylist(Playlist? playlist)
    {
        if (SelectedTrack == null || playlist == null) return;
        try
        {
            _addTrackUseCase.Execute(playlist.Id, SelectedTrack.Id);
            ErrorMessage = "";
            RefreshPlaylists();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding track '{TrackId}' to playlist '{PlaylistId}'",
                SelectedTrack?.Id, playlist?.Id);
            ErrorMessage = ex.Message;
        }
    }

    private void AddSelectedTrackToNewPlaylist()
    {
        if (SelectedTrack == null) return;

        // Show dialog — code-behind is acceptable for a modal dialog window
        var dialog = new NewPlaylistDialog { Owner = System.Windows.Application.Current.MainWindow };
        if (dialog.ShowDialog() != true || dialog.PlaylistName == null) return;

        try
        {
            _createPlaylistUseCase.Execute(dialog.PlaylistName);
            // Fetch the newly created playlist by name to get its ID
            var newPlaylist = _playlistService.GetAllPlaylists()
                .FirstOrDefault(p => p.Name == dialog.PlaylistName)
                ?? throw new InvalidOperationException("Playlist was not created.");

            _addTrackUseCase.Execute(newPlaylist.Id, SelectedTrack.Id);
            ErrorMessage = "";
            RefreshPlaylists();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding track '{TrackId}' to new playlist '{PlaylistName}'",
                SelectedTrack?.Id, dialog.PlaylistName);
            ErrorMessage = ex.Message;
        }
    }

    private void PlayTrack(Track? track)
    {
        if (track == null) return;
        _nowPlayingViewModel.PlayTrack(track, Tracks);
    }
}