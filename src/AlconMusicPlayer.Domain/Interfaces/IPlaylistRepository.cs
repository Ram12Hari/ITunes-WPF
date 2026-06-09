using AlconMusicPlayer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlconMusicPlayer.Domain.Interfaces
{
    public interface IPlaylistRepository
    {
        IEnumerable<Playlist> GetAllPlaylists();
        Playlist? GetPlaylistByID(Guid playlistID);
        void AddPlaylist(Playlist playlist);
        void RemovePlaylist(Guid playlistID);
        void UpdatePlaylist(Playlist playlist);
    }
}
