using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class PlaylistsViewModel : ViewModelBase
{
    private readonly IPlaylistService _playlistService;
    private readonly CreatePlaylistUseCase _createPlaylistUseCase;
    private readonly ILogger<PlaylistsViewModel> _logger;

    public ObservableCollection<Playlist> Playlists { get; } = [];

    private Playlist? _selectedPlaylist;
    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set => SetProperty(ref _selectedPlaylist, value);
    }

    private string _newPlaylistName = "";
    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set => SetProperty(ref _newPlaylistName, value);
    }

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand CreatePlaylistCommand { get; }

    public PlaylistsViewModel(IPlaylistService playlistService, CreatePlaylistUseCase createPlaylistUseCase, ILogger<PlaylistsViewModel> logger)
    {
        _playlistService = playlistService;
        _createPlaylistUseCase = createPlaylistUseCase;
        _logger = logger;
        CreatePlaylistCommand = new RelayCommand(CreatePlaylist,
            () => !string.IsNullOrWhiteSpace(NewPlaylistName));
        Reload();
    }

    private void CreatePlaylist()
    {
        try
        {
            // ADR-018: CreatePlaylistUseCase handles empty name + duplicate validation
            _createPlaylistUseCase.Execute(NewPlaylistName);
            NewPlaylistName = "";
            ErrorMessage = "";
            Reload();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating playlist with name: {PlaylistName}", NewPlaylistName);
            ErrorMessage = ex.Message;
        }
    }

    public void Reload()
    {
        Playlists.Clear();
        foreach (var p in _playlistService.GetAllPlaylists())
            Playlists.Add(p);
    }
}