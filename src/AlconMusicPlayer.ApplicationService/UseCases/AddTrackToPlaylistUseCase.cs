using AlconMusicPlayer.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlconMusicPlayer.ApplicationService.UseCases;

/// <summary>
/// Adds a track to a playlist after validating both exist and the track is not already present.
/// ADR-018: Playlist.AddTrack() handles the in-collection duplicate, but we still need
/// to validate existence of both IDs — that logic lives here, not in the domain entity.
/// </summary>
public class AddTrackToPlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ITrackRepository _trackRepository;
    private readonly ILogger<AddTrackToPlaylistUseCase> _logger;

    public AddTrackToPlaylistUseCase(
        IPlaylistRepository playlistRepository,
        ITrackRepository trackRepository,
        ILogger<AddTrackToPlaylistUseCase> logger)
    {
        _playlistRepository = playlistRepository;
        _trackRepository = trackRepository;
        _logger = logger;
    }

    public void Execute(Guid playlistId, Guid trackId)
    {
        var playlist = _playlistRepository.GetPlaylistByID(playlistId)
            ?? throw new InvalidOperationException($"Playlist with ID {playlistId} not found.");

        var track = _trackRepository.GetTrackById(trackId)
            ?? throw new InvalidOperationException($"Track with ID {trackId} not found.");

        // Playlist.AddTrack() already guards against duplicates internally
        playlist.AddTrack(track);
        _playlistRepository.UpdatePlaylist(playlist);
        _logger.LogInformation("Successfully added track with ID {TrackId} to playlist with ID {PlaylistId}", trackId, playlistId);
    }
}
