using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    private readonly AddTrackToPlaylistUseCase _addTrackUseCase;
    private readonly RemoveTrackFromPlaylistUseCase _removeTrackUseCase;
    private readonly NowPlayingViewModel _nowPlayingViewModel;
    private readonly ILogger<PlaylistViewModel> _logger;

    private Playlist? _playlist;

    public string PlaylistName => _playlist?.Name ?? "";
    public ObservableCollection<Track> Tracks { get; } = [];

    public event Action? BackRequested;

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

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RemoveTrackCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand PlayTrackCommand { get; }

    public PlaylistViewModel(
        AddTrackToPlaylistUseCase addTrackUseCase,
        RemoveTrackFromPlaylistUseCase removeTrackUseCase,
        NowPlayingViewModel nowPlayingViewModel,
        ILogger<PlaylistViewModel> logger)
    {
        _addTrackUseCase = addTrackUseCase;
        _removeTrackUseCase = removeTrackUseCase;
        _nowPlayingViewModel = nowPlayingViewModel;
        _logger = logger;

        RemoveTrackCommand = new RelayCommand(RemoveSelectedTrack,
            () => _playlist != null && SelectedTrack != null);
        BackCommand = new RelayCommand(() => BackRequested?.Invoke());
        PlayTrackCommand = new RelayCommand<Track>(PlayTrack,
            t => t != null);
    }

    public void Load(Playlist playlist)
    {
        _playlist = playlist;
        Tracks.Clear();
        foreach (var t in playlist.Tracks)
            Tracks.Add(t);
        OnPropertyChanged(nameof(PlaylistName));
    }

    /// <summary>
    /// Called when a track is dragged/dropped or selected from the library into this playlist.
    /// </summary>
    public void AddTrack(Guid trackId)
    {
        if (_playlist == null) return;
        try
        {
            // ADR-018: AddTrackToPlaylistUseCase validates existence + duplicate
            _addTrackUseCase.Execute(_playlist.Id, trackId);
            ErrorMessage = "";
            ReloadTracks();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding track with ID {TrackId} to playlist with ID {PlaylistId}", trackId, _playlist.Id);
            ErrorMessage = ex.Message;
        }
    }


    private void PlayTrack(Track? track)
    {
        if (track == null || _playlist == null) return;
        _nowPlayingViewModel.PlayTrack(track, Tracks);
    }

    private void RemoveSelectedTrack()
    {
        if (_playlist == null || SelectedTrack == null) return;
        try
        {
            // ADR-018: RemoveTrackFromPlaylistUseCase validates playlist exists before removing
            _removeTrackUseCase.Execute(_playlist.Id, SelectedTrack.Id);
            ErrorMessage = "";
            ReloadTracks();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing track with ID {TrackId} from playlist with ID {PlaylistId}", SelectedTrack?.Id, _playlist?.Id);
            ErrorMessage = ex.Message;
        }
    }

    private void ReloadTracks()
    {
        if (_playlist == null) return;
        Tracks.Clear();
        foreach (var t in _playlist.Tracks)
            Tracks.Add(t);
    }
}