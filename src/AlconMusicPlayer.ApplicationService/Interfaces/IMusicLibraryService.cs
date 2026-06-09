using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.ApplicationService.Interfaces
{
    public interface IMusicLibraryService
    {
        IEnumerable<Track> GetAllTracks();
        IEnumerable<Album> GetAllAlbums();
        IEnumerable<Artist> GetAllArtists();
        IEnumerable<Track> GetAllTracksByAlbum(Guid albumId);
        IEnumerable<Track> GetAllTracksByArtist(Guid artistId);
        IEnumerable<Track> SearchTrack(string trackTitle);
    }
}
