using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.WPF.Tests.TestData;

internal static class TestEntityFactory
{
    public static Artist CreateArtist(string name = "Test Artist") =>
        Artist.Create(name);

    public static Album CreateAlbum(string name = "Test Album") =>
        Album.Create(name);

    public static Track CreateTrack(
        string title = "Test Track",
        Artist? artist = null,
        Album? album = null,
        Genre genre = Genre.Rock,
        int duration = 180) =>
        Track.Create(title, $"C:/Music/{title.Replace(' ', '_')}.mp3", artist ?? CreateArtist(), album ?? CreateAlbum(), genre, duration);

    public static Playlist CreatePlaylist(string name = "Test Playlist", params Track[] tracks)
    {
        var playlist = Playlist.Create(name);

        foreach (var track in tracks)
            playlist.AddTrack(track);

        return playlist;
    }
}
