namespace AlconMusicPlayer.Domain.Entities
{
    public class Artist
    {
        public Guid Id { get; init; }
        public string Name { get; init; }

        private Artist() { }

        public static Artist Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Artist Name cannot be empty", nameof(name));
            return new Artist()
            {
                Id = Guid.NewGuid(),
                Name = name
            };
        }
    }
}
