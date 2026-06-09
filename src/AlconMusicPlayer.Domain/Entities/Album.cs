namespace AlconMusicPlayer.Domain.Entities
{
    public class Album
    {
        public Guid Id { get; init; }
        public string Name { get; init; }

        private Album() { }

        public static Album Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Album Name cannot be empty", nameof(name));
            return new Album()
            {
                Id = Guid.NewGuid(),
                Name = name
            };
        }
    }
}
