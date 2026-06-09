using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Domain.Tests;

public class PlaylistTests
{
    // ── helpers ───────────────────────────────────────────────────────────────
    private static Track MakeTrack(string title = "Track") =>
        Track.Create(title, $"{title}.mp3", Artist.Create("A"), Album.Create("B"), Genre.Pop, 120);

    // ── Playlist.Create — happy path ──────────────────────────────────────────
    [Fact]
    public void Create_Should_Return_Playlist_With_Given_Name()
    {
        var playlist = Playlist.Create("Favorites");

        Assert.NotNull(playlist);
        Assert.Equal("Favorites", playlist.Name);
    }

    [Fact]
    public void Create_Should_Assign_New_Non_Empty_Guid()
    {
        var playlist = Playlist.Create("P");

        Assert.NotEqual(Guid.Empty, playlist.Id);
    }

    [Fact]
    public void Create_Should_Initialize_With_Empty_Track_List()
    {
        var playlist = Playlist.Create("P");

        Assert.Empty(playlist.Tracks);
    }

    [Fact]
    public void Create_Should_Generate_Unique_Ids_For_Different_Playlists()
    {
        var p1 = Playlist.Create("P1");
        var p2 = Playlist.Create("P2");

        Assert.NotEqual(p1.Id, p2.Id);
    }

    // ── Playlist.Create — guard clauses ───────────────────────────────────────
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_Name_IsNullOrWhitespace(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() => Playlist.Create(name!));

        Assert.Equal("name", ex.ParamName);
    }

    // ── AddTrack ──────────────────────────────────────────────────────────────
    [Fact]
    public void AddTrack_Should_Add_Track_To_Playlist()
    {
        var playlist = Playlist.Create("P");
        var track = MakeTrack();

        playlist.AddTrack(track);

        Assert.Single(playlist.Tracks);
        Assert.Contains(track, playlist.Tracks);
    }

    [Fact]
    public void AddTrack_Should_Not_Add_Duplicate_Track()
    {
        var playlist = Playlist.Create("P");
        var track = MakeTrack();

        playlist.AddTrack(track);
        playlist.AddTrack(track); // second call — same object

        Assert.Single(playlist.Tracks);
    }

    [Fact]
    public void AddTrack_Should_Throw_When_Track_IsNull()
    {
        var playlist = Playlist.Create("P");

        Assert.Throws<ArgumentNullException>(() => playlist.AddTrack(null!));
    }

    [Fact]
    public void AddTrack_Should_Allow_Multiple_Different_Tracks()
    {
        var playlist = Playlist.Create("P");
        var t1 = MakeTrack("T1");
        var t2 = MakeTrack("T2");
        var t3 = MakeTrack("T3");

        playlist.AddTrack(t1);
        playlist.AddTrack(t2);
        playlist.AddTrack(t3);

        Assert.Equal(3, playlist.Tracks.Count);
    }

    // ── RemoveTrack ───────────────────────────────────────────────────────────
    [Fact]
    public void RemoveTrack_Should_Remove_Track_By_Id()
    {
        var playlist = Playlist.Create("P");
        var track = MakeTrack();
        playlist.AddTrack(track);

        playlist.RemoveTrack(track.Id);

        Assert.Empty(playlist.Tracks);
    }

    [Fact]
    public void RemoveTrack_Should_Be_NoOp_When_Track_Not_In_Playlist()
    {
        var playlist = Playlist.Create("P");
        var track = MakeTrack();
        playlist.AddTrack(track);

        playlist.RemoveTrack(Guid.NewGuid()); // non-existent id

        Assert.Single(playlist.Tracks); // original track still present
    }

    [Fact]
    public void RemoveTrack_Should_Only_Remove_Matching_Track()
    {
        var playlist = Playlist.Create("P");
        var t1 = MakeTrack("T1");
        var t2 = MakeTrack("T2");
        playlist.AddTrack(t1);
        playlist.AddTrack(t2);

        playlist.RemoveTrack(t1.Id);

        Assert.Single(playlist.Tracks);
        Assert.Contains(t2, playlist.Tracks);
    }

    // ── RenamePlaylist ────────────────────────────────────────────────────────
    [Fact]
    public void RenamePlaylist_Should_Update_Name()
    {
        var playlist = Playlist.Create("Old Name");

        playlist.RenamePlaylist("New Name");

        Assert.Equal("New Name", playlist.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RenamePlaylist_Should_Throw_When_Name_IsNullOrWhitespace(string? name)
    {
        var playlist = Playlist.Create("P");

        var ex = Assert.Throws<ArgumentException>(() => playlist.RenamePlaylist(name!));

        Assert.Equal("playlistName", ex.ParamName);
    }
}