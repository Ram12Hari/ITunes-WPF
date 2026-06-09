using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;

namespace AlconMusicPlayer.Infra.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ITrackRepository _trackRepository;

        public PlaylistService(IPlaylistRepository playlistRepository, ITrackRepository trackRepository)
        {
            _playlistRepository = playlistRepository;
            _trackRepository = trackRepository;
        }
        public void AddPlaylist(string playlistName)
        {
            var playlist = Playlist.Create(playlistName);
            _playlistRepository.AddPlaylist(playlist);
        }

        public void AddTrack(Guid playlistID, Guid trackID)
        {
            var playlist = _playlistRepository.GetPlaylistByID(playlistID)
                ?? throw new InvalidOperationException($"Playlist with ID {playlistID} not found.");

            var track = _trackRepository.GetTrackById(trackID)
                ?? throw new InvalidOperationException($"Track with ID {trackID} not found.");

            playlist.AddTrack(track);
            _playlistRepository.UpdatePlaylist(playlist);
            
        }

        public IEnumerable<Playlist> GetAllPlaylists() => _playlistRepository.GetAllPlaylists();

        public void RemovePlaylist(Guid playlistID)
        {
            _playlistRepository.RemovePlaylist(playlistID);
            
        }

        public void RemoveTrack(Guid playlistID, Guid trackID)
        {
            var playlist = _playlistRepository.GetPlaylistByID(playlistID)
                ?? throw new InvalidOperationException($"Playlist with ID {playlistID} not found.");

            playlist.RemoveTrack(trackID);
            _playlistRepository.UpdatePlaylist(playlist);

        }

        public void RenamePlaylist(Guid playlistID, string newName)
        {
            var playlist = _playlistRepository.GetPlaylistByID(playlistID)
                ?? throw new InvalidOperationException($"Playlist with ID {playlistID} not found.");

            playlist.RenamePlaylist(newName);
            _playlistRepository.UpdatePlaylist(playlist);

        }
    }
}
