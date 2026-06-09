using AlconMusicPlayer.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlconMusicPlayer.ApplicationService.UseCases;

/// <summary>
/// Removes a track from a playlist after validating the playlist exists.
/// ADR-018: Existence validation of the playlist ID before delegating is the business rule here.
/// Removing a track ID that isn't in the list is a no-op (safe by design in Playlist.RemoveTrack).
/// </summary>
public class RemoveTrackFromPlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ILogger<RemoveTrackFromPlaylistUseCase> _logger;

    public RemoveTrackFromPlaylistUseCase(IPlaylistRepository playlistRepository, ILogger<RemoveTrackFromPlaylistUseCase> logger)
    {
        _playlistRepository = playlistRepository;
        _logger = logger;
    }

    public void Execute(Guid playlistId, Guid trackId)
    {
        var playlist = _playlistRepository.GetPlaylistByID(playlistId)
            ?? throw new InvalidOperationException($"Playlist with ID {playlistId} not found.");

        playlist.RemoveTrack(trackId);
        _playlistRepository.UpdatePlaylist(playlist);
        _logger.LogInformation("Successfully removed track with ID {TrackId} from playlist with ID {PlaylistId}", trackId, playlistId);
    }
}
