using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Domain.Tests;

public class TrackTests
{
    // ── helpers ───────────────────────────────────────────────────────────────
    private static Artist MakeArtist() => Artist.Create("Test Artist");
    private static Album MakeAlbum() => Album.Create("Test Album");

    // ── Track.Create — happy path ─────────────────────────────────────────────
    [Fact]
    public void Create_Should_Return_Track_When_All_Inputs_Are_Valid()
    {
        // Arrange
        var artist = MakeArtist();
        var album = MakeAlbum();

        // Act
        var track = Track.Create("Song A", "path/song.mp3", artist, album, Genre.Pop, 180);

        // Assert
        Assert.NotNull(track);
        Assert.Equal("Song A", track.Title);
        Assert.Equal("path/song.mp3", track.FilePath);
        Assert.Equal(artist, track.Artist);
        Assert.Equal(album, track.Album);
        Assert.Equal(Genre.Pop, track.Genre);
        Assert.Equal(180, track.Duration);
        Assert.Equal(TimeSpan.FromSeconds(180), track.DurationDisplay);
    }

    [Fact]
    public void Create_Should_Assign_New_Non_Empty_Guid()
    {
        var track = Track.Create("T", "p.mp3", MakeArtist(), MakeAlbum(), Genre.Rock, 60);

        Assert.NotEqual(Guid.Empty, track.Id);
    }

    [Fact]
    public void Create_Should_Initialize_PlayCount_To_Zero()
    {
        var track = Track.Create("T", "p.mp3", MakeArtist(), MakeAlbum(), Genre.Rock, 60);

        Assert.Equal(0, track.PlayCount);
    }

    [Fact]
    public void Create_Should_Generate_Unique_Ids_For_Different_Tracks()
    {
        var t1 = Track.Create("T1", "p1.mp3", MakeArtist(), MakeAlbum(), Genre.Rock, 60);
        var t2 = Track.Create("T2", "p2.mp3", MakeArtist(), MakeAlbum(), Genre.Rock, 60);

        Assert.NotEqual(t1.Id, t2.Id);
    }

    // ── Track.Create — guard clauses ──────────────────────────────────────────
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_Title_IsNullOrWhitespace(string? title)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Track.Create(title!, "p.mp3", MakeArtist(), MakeAlbum(), Genre.Pop, 120));

        Assert.Equal("title", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_FilePath_IsNullOrWhitespace(string? filePath)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Track.Create("Song", filePath!, MakeArtist(), MakeAlbum(), Genre.Pop, 120));

        Assert.Equal("filePath", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Create_Should_Throw_When_Duration_IsNotPositive(int duration)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Track.Create("Song", "p.mp3", MakeArtist(), MakeAlbum(), Genre.Pop, duration));

        Assert.Equal("duration", ex.ParamName);
    }

    // ── IncrementPlayCount ────────────────────────────────────────────────────
    [Fact]
    public void IncrementPlayCount_Should_Increase_PlayCount_By_One()
    {
        var track = Track.Create("T", "p.mp3", MakeArtist(), MakeAlbum(), Genre.Ambient, 90);

        track.IncrementPlayCount();

        Assert.Equal(1, track.PlayCount);
    }

    [Fact]
    public void IncrementPlayCount_Should_Accumulate_Over_Multiple_Calls()
    {
        var track = Track.Create("T", "p.mp3", MakeArtist(), MakeAlbum(), Genre.Ambient, 90);

        track.IncrementPlayCount();
        track.IncrementPlayCount();
        track.IncrementPlayCount();

        Assert.Equal(3, track.PlayCount);
    }
}