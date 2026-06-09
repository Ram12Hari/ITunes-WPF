using AlconMusicPlayer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlconMusicPlayer.Domain.Interfaces
{
    public interface ITrackRepository
    {
        IEnumerable<Track> GetAllTracks();
        IEnumerable<Track> GetTracksByArtist(Guid artistID);
        IEnumerable<Track> GetTracksByAlbum(Guid albumID);
        IEnumerable<Track> SearchTrack(string trackTitle);
        Track? GetTrackById(Guid trackID);
    }
}
