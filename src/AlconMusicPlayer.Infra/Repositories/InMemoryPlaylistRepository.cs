using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;

namespace AlconMusicPlayer.Infra.Repositories
{
    public class InMemoryPlaylistRepository : IPlaylistRepository
    {
        private readonly List<Playlist> _playlists = [];
        public void AddPlaylist(Playlist playlist)
        {
            ArgumentNullException.ThrowIfNull(playlist);
            if (_playlists.Any(p => p.Id == playlist.Id)) return;
            _playlists.Add(playlist);
        }

        public IEnumerable<Playlist> GetAllPlaylists()
        {
            return _playlists;
        }

        public Playlist? GetPlaylistByID(Guid playlistID)
        {
            return _playlists.FirstOrDefault(playlist => playlist.Id == playlistID);
        }

        public void RemovePlaylist(Guid playlistID)
        {
            _playlists.RemoveAll(playlist => playlist.Id == playlistID);
        }

        public void UpdatePlaylist(Playlist playlist)
        {
            ArgumentNullException.ThrowIfNull(playlist, nameof(playlist));
            var index = _playlists.FindIndex(p => p.Id == playlist.Id);
            if (index == -1) throw new InvalidOperationException($"Playlist with ID {playlist.Id} and Name {playlist.Name} not found");
            _playlists[index] = playlist;
        }
    }
}
