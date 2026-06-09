namespace AlconMusicPlayer.Domain.Entities
{
    public class Playlist
    {
        public Guid Id { get; init; }
        public string Name { get; private set; }
        private readonly List<Track> _tracks = [];
        public IReadOnlyList<Track> Tracks => _tracks.AsReadOnly();

        private Playlist() { }

        public static Playlist Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Playlist name must not be empty", nameof(name));
            return new Playlist()
            {
                Id = Guid.NewGuid(),
                Name = name,
            };
        }

        public void AddTrack(Track track)
        {
            ArgumentNullException.ThrowIfNull(track);
            if (!_tracks.Contains(track))
                _tracks.Add(track);
        }

        public void RemoveTrack(Guid trackId) =>
            _tracks.RemoveAll(t => t.Id == trackId);

        public void RenamePlaylist(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName)) throw new ArgumentException("Playlist name must not be empty", nameof(playlistName));
            Name = playlistName;
        }
    }
}
