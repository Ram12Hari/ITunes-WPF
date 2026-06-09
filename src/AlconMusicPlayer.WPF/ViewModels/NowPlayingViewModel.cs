using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class NowPlayingViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;
    
    private Track? _currentTrack;
    public Track? CurrentTrack
    {
        get => _currentTrack;
        set
        {
            SetProperty(ref _currentTrack, value);
            OnPropertyChanged(nameof(NowPlayingDisplay));
        }
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    private int _currentTrackIndex;
    private List<Track> _queue = [];

    public string NowPlayingDisplay => CurrentTrack == null
        ? "No track playing"
        : $"{CurrentTrack.Title} • {CurrentTrack.Artist.Name}";

    public ICommand PlayCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }

    public NowPlayingViewModel(IMusicLibraryService musicLibraryService)
    {
        _musicLibraryService = musicLibraryService;
        
        PlayCommand = new RelayCommand(Play, () => CurrentTrack != null);
        PauseCommand = new RelayCommand(Pause, () => IsPlaying);
        NextCommand = new RelayCommand(Next, () => _queue.Count > 0 && _currentTrackIndex < _queue.Count - 1);
        PreviousCommand = new RelayCommand(Previous, () => _queue.Count > 0 && _currentTrackIndex > 0);
        
        // Auto-load first track on startup
        InitializeFirstTrack();
    }

    private void InitializeFirstTrack()
    {
        var allTracks = _musicLibraryService.GetAllTracks();
        if (allTracks.Any())
        {
            PlayTrack(allTracks.First(), allTracks);
        }
    }

    /// <summary>
    /// Start playing from a specific track. Queues all remaining tracks for next/prev navigation.
    /// </summary>
    public void PlayTrack(Track track, IEnumerable<Track> allAvailableTracks)
    {
        _queue = allAvailableTracks.ToList();
        _currentTrackIndex = _queue.IndexOf(track);
        
        if (_currentTrackIndex < 0)
        {
            _queue.Clear();
            _currentTrackIndex = 0;
        }

        CurrentTrack = track;
        IsPlaying = true;
        CommandManager.InvalidateRequerySuggested();
    }

    private void Play()
    {
        if (CurrentTrack != null)
            IsPlaying = true;
        CommandManager.InvalidateRequerySuggested();
    }

    private void Pause()
    {
        IsPlaying = false;
        CommandManager.InvalidateRequerySuggested();
    }

    private void Next()
    {
        if (_currentTrackIndex < _queue.Count - 1)
        {
            _currentTrackIndex++;
            CurrentTrack = _queue[_currentTrackIndex];
            IsPlaying = true;
        }
        CommandManager.InvalidateRequerySuggested();
    }

    private void Previous()
    {
        if (_currentTrackIndex > 0)
        {
            _currentTrackIndex--;
            CurrentTrack = _queue[_currentTrackIndex];
            IsPlaying = true;
        }
        CommandManager.InvalidateRequerySuggested();
    }
}
