using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.ApplicationService.Interfaces
{
    public interface IPlaylistService
    {
        IEnumerable<Playlist> GetAllPlaylists();
        void AddPlaylist(string playlistName);
        void RemovePlaylist(Guid playlistID);
        void AddTrack(Guid playlistID, Guid trackID);
        void RemoveTrack(Guid playlistID, Guid trackID);
        void RenamePlaylist(Guid playlistID, string newName);

    }
}
