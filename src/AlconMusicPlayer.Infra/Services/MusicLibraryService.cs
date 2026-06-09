using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;

namespace AlconMusicPlayer.Infra.Services
{
    public class MusicLibraryService : IMusicLibraryService
    {
        private readonly ITrackRepository _trackRepository;

        public MusicLibraryService(ITrackRepository trackRepository)
        {
            _trackRepository = trackRepository;
        }

        public IEnumerable<Track> GetAllTracks() =>
            _trackRepository.GetAllTracks();

        public IEnumerable<Album> GetAllAlbums() =>
            _trackRepository.GetAllTracks()
                .Where(track => track.Album != null)
                .DistinctBy(track => track.Album!.Id)
                .Select(track => track.Album!);

        public IEnumerable<Artist> GetAllArtists() =>
            _trackRepository.GetAllTracks()
                .Where(track => track.Artist != null)
                .DistinctBy(track => track.Artist!.Id)
                .Select(track => track.Artist!);

        public IEnumerable<Track> GetAllTracksByAlbum(Guid albumId) =>
            _trackRepository.GetTracksByAlbum(albumId);

        public IEnumerable<Track> GetAllTracksByArtist(Guid artistId) =>
            _trackRepository.GetTracksByArtist(artistId);

        public IEnumerable<Track> SearchTrack(string trackTitle) =>
            _trackRepository.SearchTrack(trackTitle);
    }
}
