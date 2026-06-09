using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;

namespace AlconMusicPlayer.Infra.Repositories
{
    public class InMemoryTrackRepository : ITrackRepository
    {
        private readonly List<Track> _tracks;

        public InMemoryTrackRepository(List<Track> tracks)
        {
            _tracks = tracks;
        }

        public IEnumerable<Track> GetAllTracks()
        {
            return _tracks;
        }

        public Track? GetTrackById(Guid trackID)
        {
            return _tracks.FirstOrDefault(track => track.Id == trackID);
        }

        public IEnumerable<Track> GetTracksByAlbum(Guid albumID)
        {
            return _tracks.Where(track => track.Album?.Id == albumID);
        }

        public IEnumerable<Track> GetTracksByArtist(Guid artistID)
        {
            return _tracks.Where(track => track.Artist?.Id == artistID);
        }

        public IEnumerable<Track> SearchTrack(string trackTitle)
        {
            if (string.IsNullOrWhiteSpace(trackTitle)) return Enumerable.Empty<Track>();
            return _tracks.Where(track => track.Title.Contains(trackTitle, StringComparison.OrdinalIgnoreCase));
        }
    }
}
