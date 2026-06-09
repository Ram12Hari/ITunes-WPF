using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlconMusicPlayer.ApplicationService.UseCases;

/// <summary>
/// Creates a new playlist after validating the name is not empty or a duplicate.
/// ADR-018: Use cases only where real business logic exists — duplicate check justifies this class.
/// </summary>
public class CreatePlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ILogger<CreatePlaylistUseCase> _logger;

    public CreatePlaylistUseCase(IPlaylistRepository playlistRepository, ILogger<CreatePlaylistUseCase> logger)
    {
        _playlistRepository = playlistRepository;
        _logger = logger;
    }

    public void Execute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Attempt to create playlist with empty name.");
            throw new ArgumentException("Playlist name must not be empty.", nameof(name));
        }

        // ADR-018: duplicate check — business rule not enforced anywhere else
        bool duplicate = _playlistRepository
            .GetAllPlaylists()
            .Any(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            _logger.LogWarning("Attempt to create duplicate playlist with name: {PlaylistName}", name);
            throw new InvalidOperationException($"A playlist named \"{name}\" already exists.");
        }
            

        var playlist = Playlist.Create(name.Trim());
        _playlistRepository.AddPlaylist(playlist);
        _logger.LogInformation("Created new playlist with name: {PlaylistName} and ID: {PlaylistId}", playlist.Name, playlist.Id);
    }
}
