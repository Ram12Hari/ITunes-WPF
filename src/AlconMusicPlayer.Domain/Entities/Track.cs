namespace AlconMusicPlayer.Domain.Entities
{
    public class Track
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public string FilePath { get; init; }
        public Artist Artist { get; init; }
        public Album Album { get; init; }
        public Genre Genre { get; init; }
        public int Duration { get; init; }
        public TimeSpan DurationDisplay { get; init; }
        public int PlayCount { get; private set; }

        private Track() { }

        public static Track Create(string title, string filePath, Artist artist, Album album, Genre genre, int duration)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title must not be empty", nameof(title));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path should not be empty", nameof(filePath));
            if (duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration), "Duration should be more than zero");

            Track track = new Track()
            {
                Id = Guid.NewGuid(),
                Title = title,
                FilePath = filePath,
                Artist = artist,
                Album = album,
                Genre = genre,
                Duration = duration,
                DurationDisplay = TimeSpan.FromSeconds(duration),
            };
            return track;
        }
        public void IncrementPlayCount() => PlayCount++;

    }
}
