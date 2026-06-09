# Unit Testing Guide for AlconMusicPlayer with xUnit

This guide explains how to add unit tests to this solution step by step, while keeping the codebase organized and easy to maintain.

## 1. Goal

We want to add automated unit tests for the main projects in this solution using **xUnit**.

Recommended focus order:

1. `AlconMusicPlayer.Domain`
2. `AlconMusicPlayer.ApplicationService`
3. `AlconMusicPlayer.Infra`
4. `AlconMusicPlayer.WPF` only where practical

## 2. Recommended Folder Structure

Keep test projects in a top-level `tests/` folder parallel to `src/`.

```text
AlconMusicPlayer2.0/
├─ src/
│  ├─ AlconMusicPlayer.Domain/
│  ├─ AlconMusicPlayer.ApplicationService/
│  ├─ AlconMusicPlayer.Infra/
│  └─ AlconMusicPlayer.WPF/
├─ tests/
│  ├─ AlconMusicPlayer.Domain.Tests/
│  ├─ AlconMusicPlayer.ApplicationService.Tests/
│  ├─ AlconMusicPlayer.Infra.Tests/
│  └─ AlconMusicPlayer.WPF.Tests/
└─ AlconMusicPlayer.sln
```

Why this is better:

- separates production code from tests
- matches common .NET solution structure
- makes CI and solution management easier
- keeps `src/` clean

## 3. Test Framework Choice

Use:

- **xUnit** for the test framework
- **Microsoft.NET.Test.Sdk** for test discovery/execution
- **xunit.runner.visualstudio** for IDE support
- **Moq** for mocking dependencies (only third-party package)

## 3.1 Target Framework Version

Your production projects target `.NET 8`, so test projects should also target `net8.0`.

Why:

- keeps runtime behavior aligned with production
- avoids cross-version surprises
- simplifies local and CI execution

## 4. Create the Test Projects

Run these commands from the solution root:

```powershell
dotnet new xunit -f net8.0 -o tests/AlconMusicPlayer.Domain.Tests
dotnet new xunit -f net8.0 -o tests/AlconMusicPlayer.ApplicationService.Tests
dotnet new xunit -f net8.0 -o tests/AlconMusicPlayer.Infra.Tests
```

For WPF, only add tests if you have logic worth testing outside the UI itself:

```powershell
dotnet new xunit -f net8.0 -o tests/AlconMusicPlayer.WPF.Tests
```

## 5. Add the Test Projects to the Solution

```powershell
dotnet sln AlconMusicPlayer.sln add tests/AlconMusicPlayer.Domain.Tests/AlconMusicPlayer.Domain.Tests.csproj
dotnet sln AlconMusicPlayer.sln add tests/AlconMusicPlayer.ApplicationService.Tests/AlconMusicPlayer.ApplicationService.Tests.csproj
dotnet sln AlconMusicPlayer.sln add tests/AlconMusicPlayer.Infra.Tests/AlconMusicPlayer.Infra.Tests.csproj
```

If you also create WPF tests:

```powershell
dotnet sln AlconMusicPlayer.sln add tests/AlconMusicPlayer.WPF.Tests/AlconMusicPlayer.WPF.Tests.csproj
```

## 6. Add Project References

Each test project should reference the project it tests.

### Domain tests

```powershell
dotnet add tests/AlconMusicPlayer.Domain.Tests/AlconMusicPlayer.Domain.Tests.csproj reference src/AlconMusicPlayer.Domain/AlconMusicPlayer.Domain.csproj
```

### ApplicationService tests

```powershell
dotnet add tests/AlconMusicPlayer.ApplicationService.Tests/AlconMusicPlayer.ApplicationService.Tests.csproj reference src/AlconMusicPlayer.ApplicationService/AlconMusicPlayer.ApplicationService.csproj
dotnet add tests/AlconMusicPlayer.ApplicationService.Tests/AlconMusicPlayer.ApplicationService.Tests.csproj reference src/AlconMusicPlayer.Domain/AlconMusicPlayer.Domain.csproj
```

### Infra tests

```powershell
dotnet add tests/AlconMusicPlayer.Infra.Tests/AlconMusicPlayer.Infra.Tests.csproj reference src/AlconMusicPlayer.Infra/AlconMusicPlayer.Infra.csproj
dotnet add tests/AlconMusicPlayer.Infra.Tests/AlconMusicPlayer.Infra.Tests.csproj reference src/AlconMusicPlayer.Domain/AlconMusicPlayer.Domain.csproj
dotnet add tests/AlconMusicPlayer.Infra.Tests/AlconMusicPlayer.Infra.Tests.csproj reference src/AlconMusicPlayer.ApplicationService/AlconMusicPlayer.ApplicationService.csproj
```

### WPF tests

```powershell
dotnet add tests/AlconMusicPlayer.WPF.Tests/AlconMusicPlayer.WPF.Tests.csproj reference src/AlconMusicPlayer.WPF/AlconMusicPlayer.WPF.csproj
```

## 7. Add Minimal NuGet Packages

To keep third-party libraries minimal, add only `Moq` where mocking is needed:

```powershell
dotnet add tests/AlconMusicPlayer.ApplicationService.Tests package Moq
dotnet add tests/AlconMusicPlayer.Infra.Tests package Moq
dotnet add tests/AlconMusicPlayer.WPF.Tests package Moq
```

## 8. What to Test in Each Project

## 8.1 Domain

Test pure business rules and entity behavior — no mocks, no infrastructure.

Good candidates: `Track`, `Playlist`, `Artist`, `Album` factories and guard clauses.

## 8.2 ApplicationService

Test use cases and orchestration logic using mocked repositories.

Good candidates: `CreatePlaylistUseCase`, `AddTrackToPlaylistUseCase`, `RemoveTrackFromPlaylistUseCase`.

## 8.3 Infra

Test in-memory repositories and services with real objects (no mocks needed).

Good candidates: `InMemoryPlaylistRepository`, `InMemoryTrackRepository`, `MusicLibraryService`, `PlaylistService`.

## 8.4 WPF

Prefer testing ViewModel logic only — no XAML, no rendering.
Note: `NowPlayingViewModel` calls `CommandManager.InvalidateRequerySuggested()` which requires a WPF dispatcher; test it on the STA thread or via `[STAThread]` test runner configuration.

## 9. Complete Test Code

> Copy each class into the matching test project under `tests/`.

---

### 9.1 Domain — `TrackTests.cs`

File: `tests/AlconMusicPlayer.Domain.Tests/TrackTests.cs`

```csharp
using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Domain.Tests;

public class TrackTests
{
    // ── helpers ───────────────────────────────────────────────────────────────
    private static Artist MakeArtist() => Artist.Create("Test Artist");
    private static Album  MakeAlbum()  => Album.Create("Test Album");

    // ── Track.Create — happy path ─────────────────────────────────────────────
    [Fact]
    public void Create_Should_Return_Track_When_All_Inputs_Are_Valid()
    {
        // Arrange
        var artist = MakeArtist();
        var album  = MakeAlbum();

        // Act
        var track = Track.Create("Song A", "path/song.mp3", artist, album, Genre.Pop, 180);

        // Assert
        Assert.NotNull(track);
        Assert.Equal("Song A",              track.Title);
        Assert.Equal("path/song.mp3",       track.FilePath);
        Assert.Equal(artist,                track.Artist);
        Assert.Equal(album,                 track.Album);
        Assert.Equal(Genre.Pop,             track.Genre);
        Assert.Equal(180,                   track.Duration);
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
```

---

### 9.2 Domain — `PlaylistTests.cs`

File: `tests/AlconMusicPlayer.Domain.Tests/PlaylistTests.cs`

```csharp
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
        var track    = MakeTrack();

        playlist.AddTrack(track);

        Assert.Single(playlist.Tracks);
        Assert.Contains(track, playlist.Tracks);
    }

    [Fact]
    public void AddTrack_Should_Not_Add_Duplicate_Track()
    {
        var playlist = Playlist.Create("P");
        var track    = MakeTrack();

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
        var track    = MakeTrack();
        playlist.AddTrack(track);

        playlist.RemoveTrack(track.Id);

        Assert.Empty(playlist.Tracks);
    }

    [Fact]
    public void RemoveTrack_Should_Be_NoOp_When_Track_Not_In_Playlist()
    {
        var playlist = Playlist.Create("P");
        var track    = MakeTrack();
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
```

---

### 9.3 Domain — `ArtistTests.cs`

File: `tests/AlconMusicPlayer.Domain.Tests/ArtistTests.cs`

```csharp
using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Domain.Tests;

public class ArtistTests
{
    [Fact]
    public void Create_Should_Return_Artist_With_Given_Name()
    {
        var artist = Artist.Create("Pink Floyd");

        Assert.NotNull(artist);
        Assert.Equal("Pink Floyd", artist.Name);
    }

    [Fact]
    public void Create_Should_Assign_New_Non_Empty_Guid()
    {
        var artist = Artist.Create("Coldplay");

        Assert.NotEqual(Guid.Empty, artist.Id);
    }

    [Fact]
    public void Create_Should_Generate_Unique_Ids()
    {
        var a1 = Artist.Create("A1");
        var a2 = Artist.Create("A2");

        Assert.NotEqual(a1.Id, a2.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_Name_IsNullOrWhitespace(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() => Artist.Create(name!));

        Assert.Equal("name", ex.ParamName);
    }
}
```

---

### 9.4 Domain — `AlbumTests.cs`

File: `tests/AlconMusicPlayer.Domain.Tests/AlbumTests.cs`

```csharp
using AlconMusicPlayer.Domain.Entities;

namespace AlconMusicPlayer.Domain.Tests;

public class AlbumTests
{
    [Fact]
    public void Create_Should_Return_Album_With_Given_Name()
    {
        var album = Album.Create("OK Computer");

        Assert.NotNull(album);
        Assert.Equal("OK Computer", album.Name);
    }

    [Fact]
    public void Create_Should_Assign_New_Non_Empty_Guid()
    {
        var album = Album.Create("Roja");

        Assert.NotEqual(Guid.Empty, album.Id);
    }

    [Fact]
    public void Create_Should_Generate_Unique_Ids()
    {
        var a1 = Album.Create("A1");
        var a2 = Album.Create("A2");

        Assert.NotEqual(a1.Id, a2.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_Throw_When_Name_IsNullOrWhitespace(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(() => Album.Create(name!));

        Assert.Equal("name", ex.ParamName);
    }
}
```

---

### 9.5 ApplicationService — `CreatePlaylistUseCaseTests.cs`

File: `tests/AlconMusicPlayer.ApplicationService.Tests/CreatePlaylistUseCaseTests.cs`

Requires `Moq` package in the `.csproj`.

```csharp
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using Moq;

namespace AlconMusicPlayer.ApplicationService.Tests;

public class CreatePlaylistUseCaseTests
{
    private readonly Mock<IPlaylistRepository> _repoMock = new();

    private CreatePlaylistUseCase CreateSut() =>
        new(_repoMock.Object);

    // ── happy path ────────────────────────────────────────────────────────────
    [Fact]
    public void Execute_Should_Call_AddPlaylist_Once_When_Name_Is_Valid()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllPlaylists()).Returns([]);
        var sut = CreateSut();

        // Act
        sut.Execute("Favorites");

        // Assert
        _repoMock.Verify(r => r.AddPlaylist(It.Is<Playlist>(p => p.Name == "Favorites")), Times.Once);
    }

    [Fact]
    public void Execute_Should_Trim_Whitespace_From_Name_Before_Saving()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllPlaylists()).Returns([]);
        var sut = CreateSut();

        // Act
        sut.Execute("  Chill Mix  ");

        // Assert — stored name must be trimmed
        _repoMock.Verify(r => r.AddPlaylist(It.Is<Playlist>(p => p.Name == "Chill Mix")), Times.Once);
    }

    // ── duplicate guard ───────────────────────────────────────────────────────
    [Fact]
    public void Execute_Should_Throw_InvalidOperationException_When_Name_Already_Exists()
    {
        // Arrange
        var existing = Playlist.Create("Favorites");
        _repoMock.Setup(r => r.GetAllPlaylists()).Returns([existing]);
        var sut = CreateSut();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sut.Execute("Favorites"));
    }

    [Fact]
    public void Execute_Should_Throw_When_Duplicate_Name_Has_Different_Casing()
    {
        // Arrange
        var existing = Playlist.Create("favorites");
        _repoMock.Setup(r => r.GetAllPlaylists()).Returns([existing]);
        var sut = CreateSut();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sut.Execute("FAVORITES"));
    }

    [Fact]
    public void Execute_Should_Not_Call_AddPlaylist_When_Duplicate_Name()
    {
        // Arrange
        var existing = Playlist.Create("Favorites");
        _repoMock.Setup(r => r.GetAllPlaylists()).Returns([existing]);
        var sut = CreateSut();

        // Act
        try { sut.Execute("Favorites"); } catch { /* expected */ }

        // Assert
        _repoMock.Verify(r => r.AddPlaylist(It.IsAny<Playlist>()), Times.Never);
    }

    // ── empty name guard ──────────────────────────────────────────────────────
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Execute_Should_Throw_ArgumentException_When_Name_IsNullOrWhitespace(string name)
    {
        var sut = CreateSut();

        Assert.Throws<ArgumentException>(() => sut.Execute(name));
    }
}
```

---

### 9.6 ApplicationService — `AddTrackToPlaylistUseCaseTests.cs`

File: `tests/AlconMusicPlayer.ApplicationService.Tests/AddTrackToPlaylistUseCaseTests.cs`

```csharp
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using Moq;

namespace AlconMusicPlayer.ApplicationService.Tests;

public class AddTrackToPlaylistUseCaseTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepoMock = new();
    private readonly Mock<ITrackRepository>    _trackRepoMock    = new();

    private AddTrackToPlaylistUseCase CreateSut() =>
        new(_playlistRepoMock.Object, _trackRepoMock.Object);

    // ── helpers ───────────────────────────────────────────────────────────────
    private static Track MakeTrack() =>
        Track.Create("Song", "song.mp3", Artist.Create("A"), Album.Create("B"), Genre.Pop, 120);

    // ── happy path ────────────────────────────────────────────────────────────
    [Fact]
    public void Execute_Should_Add_Track_To_Playlist_And_Call_Update()
    {
        // Arrange
        var playlist = Playlist.Create("My Playlist");
        var track    = MakeTrack();

        _playlistRepoMock.Setup(r => r.GetPlaylistByID(playlist.Id)).Returns(playlist);
        _trackRepoMock.Setup(r => r.GetTrackById(track.Id)).Returns(track);

        var sut = CreateSut();

        // Act
        sut.Execute(playlist.Id, track.Id);

        // Assert
        Assert.Contains(track, playlist.Tracks);
        _playlistRepoMock.Verify(r => r.UpdatePlaylist(playlist), Times.Once);
    }

    // ── not-found guards ──────────────────────────────────────────────────────
    [Fact]
    public void Execute_Should_Throw_When_Playlist_Not_Found()
    {
        // Arrange
        var track = MakeTrack();
        _playlistRepoMock.Setup(r => r.GetPlaylistByID(It.IsAny<Guid>())).Returns((Playlist?)null);
        _trackRepoMock.Setup(r => r.GetTrackById(track.Id)).Returns(track);

        var sut = CreateSut();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            sut.Execute(Guid.NewGuid(), track.Id));
    }

    [Fact]
    public void Execute_Should_Throw_When_Track_Not_Found()
    {
        // Arrange
        var playlist = Playlist.Create("P");
        _playlistRepoMock.Setup(r => r.GetPlaylistByID(playlist.Id)).Returns(playlist);
        _trackRepoMock.Setup(r => r.GetTrackById(It.IsAny<Guid>())).Returns((Track?)null);

        var sut = CreateSut();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            sut.Execute(playlist.Id, Guid.NewGuid()));
    }

    [Fact]
    public void Execute_Should_Not_Call_Update_When_Playlist_Not_Found()
    {
        // Arrange
        _playlistRepoMock.Setup(r => r.GetPlaylistByID(It.IsAny<Guid>())).Returns((Playlist?)null);
        var sut = CreateSut();

        // Act
        try { sut.Execute(Guid.NewGuid(), Guid.NewGuid()); } catch { /* expected */ }

        // Assert
        _playlistRepoMock.Verify(r => r.UpdatePlaylist(It.IsAny<Playlist>()), Times.Never);
    }

    // ── duplicate track (idempotent via Playlist.AddTrack) ────────────────────
    [Fact]
    public void Execute_Should_Not_Add_Duplicate_Track_To_Playlist()
    {
        // Arrange
        var playlist = Playlist.Create("P");
        var track    = MakeTrack();
        playlist.AddTrack(track); // already in playlist

        _playlistRepoMock.Setup(r => r.GetPlaylistByID(playlist.Id)).Returns(playlist);
        _trackRepoMock.Setup(r => r.GetTrackById(track.Id)).Returns(track);

        var sut = CreateSut();

        // Act
        sut.Execute(playlist.Id, track.Id);

        // Assert — still only one track
        Assert.Single(playlist.Tracks);
    }
}
```

---

### 9.7 ApplicationService — `RemoveTrackFromPlaylistUseCaseTests.cs`

File: `tests/AlconMusicPlayer.ApplicationService.Tests/RemoveTrackFromPlaylistUseCaseTests.cs`

```csharp
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using Moq;

namespace AlconMusicPlayer.ApplicationService.Tests;

public class RemoveTrackFromPlaylistUseCaseTests
{
    private readonly Mock<IPlaylistRepository> _repoMock = new();

    private RemoveTrackFromPlaylistUseCase CreateSut() =>
        new(_repoMock.Object);

    private static Track MakeTrack() =>
        Track.Create("Song", "song.mp3", Artist.Create("A"), Album.Create("B"), Genre.Pop, 120);

    // ── happy path ────────────────────────────────────────────────────────────
    [Fact]
    public void Execute_Should_Remove_Track_And_Call_Update()
    {
        // Arrange
        var playlist = Playlist.Create("P");
        var track    = MakeTrack();
        playlist.AddTrack(track);

        _repoMock.Setup(r => r.GetPlaylistByID(playlist.Id)).Returns(playlist);

        var sut = CreateSut();

        // Act
        sut.Execute(playlist.Id, track.Id);

        // Assert
        Assert.Empty(playlist.Tracks);
        _repoMock.Verify(r => r.UpdatePlaylist(playlist), Times.Once);
    }

    [Fact]
    public void Execute_Should_Be_NoOp_When_Track_Not_In_Playlist()
    {
        // Arrange
        var playlist = Playlist.Create("P");
        var track    = MakeTrack();
        playlist.AddTrack(track);

        _repoMock.Setup(r => r.GetPlaylistByID(playlist.Id)).Returns(playlist);

        var sut = CreateSut();

        // Act — remove a different track id
        sut.Execute(playlist.Id, Guid.NewGuid());

        // Assert — original track still present, update still called
        Assert.Single(playlist.Tracks);
        _repoMock.Verify(r => r.UpdatePlaylist(playlist), Times.Once);
    }

    // ── not-found guard ───────────────────────────────────────────────────────
    [Fact]
    public void Execute_Should_Throw_When_Playlist_Not_Found()
    {
        // Arrange
        _repoMock.Setup(r => r.GetPlaylistByID(It.IsAny<Guid>())).Returns((Playlist?)null);
        var sut = CreateSut();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            sut.Execute(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Execute_Should_Not_Call_Update_When_Playlist_Not_Found()
    {
        // Arrange
        _repoMock.Setup(r => r.GetPlaylistByID(It.IsAny<Guid>())).Returns((Playlist?)null);
        var sut = CreateSut();

        // Act
        try { sut.Execute(Guid.NewGuid(), Guid.NewGuid()); } catch { /* expected */ }

        // Assert
        _repoMock.Verify(r => r.UpdatePlaylist(It.IsAny<Playlist>()), Times.Never);
    }
}
```

---

### 9.8 Infra — `InMemoryPlaylistRepositoryTests.cs`

File: `tests/AlconMusicPlayer.Infra.Tests/InMemoryPlaylistRepositoryTests.cs`

```csharp
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Infra.Repositories;

namespace AlconMusicPlayer.Infra.Tests;

public class InMemoryPlaylistRepositoryTests
{
    private static InMemoryPlaylistRepository MakeSut() => new();

    private static Playlist MakePlaylist(string name = "Test") => Playlist.Create(name);

    // ── AddPlaylist ───────────────────────────────────────────────────────────
    [Fact]
    public void AddPlaylist_Should_Persist_Playlist()
    {
        var sut      = MakeSut();
        var playlist = MakePlaylist();

        sut.AddPlaylist(playlist);

        Assert.Single(sut.GetAllPlaylists());
    }

    [Fact]
    public void AddPlaylist_Should_Not_Add_Duplicate_By_Id()
    {
        var sut      = MakeSut();
        var playlist = MakePlaylist();

        sut.AddPlaylist(playlist);
        sut.AddPlaylist(playlist); // same object

        Assert.Single(sut.GetAllPlaylists());
    }

    [Fact]
    public void AddPlaylist_Should_Throw_When_Null()
    {
        var sut = MakeSut();

        Assert.Throws<ArgumentNullException>(() => sut.AddPlaylist(null!));
    }

    // ── GetAllPlaylists ───────────────────────────────────────────────────────
    [Fact]
    public void GetAllPlaylists_Should_Return_Empty_When_Nothing_Added()
    {
        var sut = MakeSut();

        Assert.Empty(sut.GetAllPlaylists());
    }

    [Fact]
    public void GetAllPlaylists_Should_Return_All_Added_Playlists()
    {
        var sut = MakeSut();
        sut.AddPlaylist(MakePlaylist("P1"));
        sut.AddPlaylist(MakePlaylist("P2"));
        sut.AddPlaylist(MakePlaylist("P3"));

        Assert.Equal(3, sut.GetAllPlaylists().Count());
    }

    // ── GetPlaylistByID ───────────────────────────────────────────────────────
    [Fact]
    public void GetPlaylistByID_Should_Return_Correct_Playlist()
    {
        var sut      = MakeSut();
        var playlist = MakePlaylist("Rock");
        sut.AddPlaylist(playlist);

        var result = sut.GetPlaylistByID(playlist.Id);

        Assert.Equal(playlist.Id, result!.Id);
    }

    [Fact]
    public void GetPlaylistByID_Should_Return_Null_When_Not_Found()
    {
        var sut = MakeSut();

        var result = sut.GetPlaylistByID(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── RemovePlaylist ────────────────────────────────────────────────────────
    [Fact]
    public void RemovePlaylist_Should_Remove_Playlist_By_Id()
    {
        var sut      = MakeSut();
        var playlist = MakePlaylist();
        sut.AddPlaylist(playlist);

        sut.RemovePlaylist(playlist.Id);

        Assert.Empty(sut.GetAllPlaylists());
    }

    [Fact]
    public void RemovePlaylist_Should_Be_NoOp_When_Id_Not_Found()
    {
        var sut = MakeSut();
        sut.AddPlaylist(MakePlaylist("P1"));

        sut.RemovePlaylist(Guid.NewGuid()); // unknown id

        Assert.Single(sut.GetAllPlaylists()); // original still there
    }

    // ── UpdatePlaylist ────────────────────────────────────────────────────────
    [Fact]
    public void UpdatePlaylist_Should_Replace_Existing_Entry()
    {
        var sut      = MakeSut();
        var playlist = MakePlaylist("Old");
        sut.AddPlaylist(playlist);

        playlist.RenamePlaylist("New");
        sut.UpdatePlaylist(playlist);

        var stored = sut.GetPlaylistByID(playlist.Id)!;
        Assert.Equal("New", stored.Name);
    }

    [Fact]
    public void UpdatePlaylist_Should_Throw_When_Playlist_Not_In_Repository()
    {
        var sut      = MakeSut();
        var playlist = MakePlaylist();  // never added

        Assert.Throws<InvalidOperationException>(() => sut.UpdatePlaylist(playlist));
    }

    [Fact]
    public void UpdatePlaylist_Should_Throw_When_Null()
    {
        var sut = MakeSut();

        Assert.Throws<ArgumentNullException>(() => sut.UpdatePlaylist(null!));
    }
}
```

---

### 9.9 Infra — `InMemoryTrackRepositoryTests.cs`

File: `tests/AlconMusicPlayer.Infra.Tests/InMemoryTrackRepositoryTests.cs`

```csharp
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Infra.Repositories;

namespace AlconMusicPlayer.Infra.Tests;

public class InMemoryTrackRepositoryTests
{
    // ── helpers ───────────────────────────────────────────────────────────────
    private static Artist  MakeArtist(string name = "Artist") => Artist.Create(name);
    private static Album   MakeAlbum(string name = "Album")   => Album.Create(name);

    private static Track MakeTrack(string title, Artist? artist = null, Album? album = null,
                                   Genre genre = Genre.Pop, int duration = 120) =>
        Track.Create(title, $"{title}.mp3", artist ?? MakeArtist(), album ?? MakeAlbum(), genre, duration);

    private static InMemoryTrackRepository MakeSut(params Track[] tracks) =>
        new(tracks.ToList());

    // ── GetAllTracks ──────────────────────────────────────────────────────────
    [Fact]
    public void GetAllTracks_Should_Return_All_Seeded_Tracks()
    {
        var t1  = MakeTrack("T1");
        var t2  = MakeTrack("T2");
        var sut = MakeSut(t1, t2);

        var result = sut.GetAllTracks().ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetAllTracks_Should_Return_Empty_When_No_Tracks_Seeded()
    {
        var sut = MakeSut();

        Assert.Empty(sut.GetAllTracks());
    }

    // ── GetTrackById ──────────────────────────────────────────────────────────
    [Fact]
    public void GetTrackById_Should_Return_Correct_Track()
    {
        var track = MakeTrack("Song");
        var sut   = MakeSut(track);

        var result = sut.GetTrackById(track.Id);

        Assert.Equal(track.Id, result!.Id);
    }

    [Fact]
    public void GetTrackById_Should_Return_Null_When_Not_Found()
    {
        var sut = MakeSut(MakeTrack("T"));

        Assert.Null(sut.GetTrackById(Guid.NewGuid()));
    }

    // ── GetTracksByArtist ─────────────────────────────────────────────────────
    [Fact]
    public void GetTracksByArtist_Should_Return_Only_Tracks_By_Given_Artist()
    {
        var artist1 = MakeArtist("AR Rahman");
        var artist2 = MakeArtist("Pink Floyd");
        var t1 = MakeTrack("Song1", artist1);
        var t2 = MakeTrack("Song2", artist1);
        var t3 = MakeTrack("Song3", artist2);
        var sut = MakeSut(t1, t2, t3);

        var result = sut.GetTracksByArtist(artist1.Id).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(artist1.Id, t.Artist.Id));
    }

    [Fact]
    public void GetTracksByArtist_Should_Return_Empty_When_Artist_Has_No_Tracks()
    {
        var sut = MakeSut(MakeTrack("T"));

        var result = sut.GetTracksByArtist(Guid.NewGuid());

        Assert.Empty(result);
    }

    // ── GetTracksByAlbum ──────────────────────────────────────────────────────
    [Fact]
    public void GetTracksByAlbum_Should_Return_Only_Tracks_In_Given_Album()
    {
        var album1 = MakeAlbum("Roja");
        var album2 = MakeAlbum("Bombay");
        var t1 = MakeTrack("T1", null, album1);
        var t2 = MakeTrack("T2", null, album1);
        var t3 = MakeTrack("T3", null, album2);
        var sut = MakeSut(t1, t2, t3);

        var result = sut.GetTracksByAlbum(album1.Id).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(album1.Id, t.Album.Id));
    }

    // ── SearchTrack ───────────────────────────────────────────────────────────
    [Fact]
    public void SearchTrack_Should_Return_Matching_Tracks_Case_Insensitive()
    {
        var t1  = MakeTrack("Roja Jaaneman");
        var t2  = MakeTrack("Humma Humma");
        var sut = MakeSut(t1, t2);

        var result = sut.SearchTrack("roja").ToList();

        Assert.Single(result);
        Assert.Equal("Roja Jaaneman", result[0].Title);
    }

    [Fact]
    public void SearchTrack_Should_Return_Empty_When_No_Match()
    {
        var sut = MakeSut(MakeTrack("Song A"));

        Assert.Empty(sut.SearchTrack("xyz"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SearchTrack_Should_Return_Empty_When_Query_IsNullOrWhitespace(string? query)
    {
        var sut = MakeSut(MakeTrack("Song"));

        Assert.Empty(sut.SearchTrack(query!));
    }

    [Fact]
    public void SearchTrack_Should_Return_Multiple_Matches()
    {
        var sut = MakeSut(MakeTrack("Rock Song 1"), MakeTrack("Rock Song 2"), MakeTrack("Pop Song"));

        var result = sut.SearchTrack("Rock").ToList();

        Assert.Equal(2, result.Count);
    }
}
```

---

### 9.10 Infra — `MusicLibraryServiceTests.cs`

File: `tests/AlconMusicPlayer.Infra.Tests/MusicLibraryServiceTests.cs`

```csharp
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using AlconMusicPlayer.Infra.Repositories;
using AlconMusicPlayer.Infra.Services;
using Moq;

namespace AlconMusicPlayer.Infra.Tests;

public class MusicLibraryServiceTests
{
    // ── helpers ───────────────────────────────────────────────────────────────
    private static Artist MakeArtist(string name = "Artist") => Artist.Create(name);
    private static Album  MakeAlbum(string name = "Album")   => Album.Create(name);

    private static Track MakeTrack(string title, Artist artist, Album album) =>
        Track.Create(title, $"{title}.mp3", artist, album, Genre.Rock, 200);

    private static MusicLibraryService MakeSutWithTracks(params Track[] tracks) =>
        new(new InMemoryTrackRepository(tracks.ToList()));

    // ── GetAllTracks ──────────────────────────────────────────────────────────
    [Fact]
    public void GetAllTracks_Should_Return_All_Tracks()
    {
        var artist = MakeArtist();
        var album  = MakeAlbum();
        var sut    = MakeSutWithTracks(
            MakeTrack("T1", artist, album),
            MakeTrack("T2", artist, album));

        Assert.Equal(2, sut.GetAllTracks().Count());
    }

    [Fact]
    public void GetAllTracks_Should_Return_Empty_When_No_Tracks()
    {
        var sut = MakeSutWithTracks();

        Assert.Empty(sut.GetAllTracks());
    }

    // ── GetAllArtists ─────────────────────────────────────────────────────────
    [Fact]
    public void GetAllArtists_Should_Return_Distinct_Artists()
    {
        var artist1 = MakeArtist("AR Rahman");
        var artist2 = MakeArtist("Pink Floyd");
        var album   = MakeAlbum();
        var sut     = MakeSutWithTracks(
            MakeTrack("T1", artist1, album),
            MakeTrack("T2", artist1, album),   // same artist again
            MakeTrack("T3", artist2, album));

        var artists = sut.GetAllArtists().ToList();

        Assert.Equal(2, artists.Count);
    }

    [Fact]
    public void GetAllArtists_Should_Return_Empty_When_No_Tracks()
    {
        var sut = MakeSutWithTracks();

        Assert.Empty(sut.GetAllArtists());
    }

    // ── GetAllAlbums ──────────────────────────────────────────────────────────
    [Fact]
    public void GetAllAlbums_Should_Return_Distinct_Albums()
    {
        var artist = MakeArtist();
        var album1 = MakeAlbum("Roja");
        var album2 = MakeAlbum("Bombay");
        var sut    = MakeSutWithTracks(
            MakeTrack("T1", artist, album1),
            MakeTrack("T2", artist, album1),   // same album again
            MakeTrack("T3", artist, album2));

        var albums = sut.GetAllAlbums().ToList();

        Assert.Equal(2, albums.Count);
    }

    // ── GetAllTracksByArtist ──────────────────────────────────────────────────
    [Fact]
    public void GetAllTracksByArtist_Should_Return_Matching_Tracks()
    {
        var artist1 = MakeArtist("A1");
        var artist2 = MakeArtist("A2");
        var album   = MakeAlbum();
        var t1      = MakeTrack("T1", artist1, album);
        var t2      = MakeTrack("T2", artist1, album);
        var t3      = MakeTrack("T3", artist2, album);
        var sut     = MakeSutWithTracks(t1, t2, t3);

        var result = sut.GetAllTracksByArtist(artist1.Id).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(artist1.Id, t.Artist.Id));
    }

    // ── GetAllTracksByAlbum ───────────────────────────────────────────────────
    [Fact]
    public void GetAllTracksByAlbum_Should_Return_Matching_Tracks()
    {
        var artist = MakeArtist();
        var album1 = MakeAlbum("A1");
        var album2 = MakeAlbum("A2");
        var t1     = MakeTrack("T1", artist, album1);
        var t2     = MakeTrack("T2", artist, album2);
        var sut    = MakeSutWithTracks(t1, t2);

        var result = sut.GetAllTracksByAlbum(album1.Id).ToList();

        Assert.Single(result);
        Assert.Equal(album1.Id, result[0].Album.Id);
    }

    // ── SearchTrack ───────────────────────────────────────────────────────────
    [Fact]
    public void SearchTrack_Should_Delegate_To_Repository_And_Return_Matches()
    {
        var artist = MakeArtist();
        var album  = MakeAlbum();
        var sut    = MakeSutWithTracks(
            MakeTrack("Roja Jaaneman", artist, album),
            MakeTrack("Humma Humma",   artist, album));

        var result = sut.SearchTrack("roja").ToList();

        Assert.Single(result);
        Assert.Equal("Roja Jaaneman", result[0].Title);
    }

    // ── mock-based: verify delegation ─────────────────────────────────────────
    [Fact]
    public void GetAllTracks_Should_Delegate_To_TrackRepository()
    {
        var repoMock = new Mock<ITrackRepository>();
        repoMock.Setup(r => r.GetAllTracks()).Returns([]);
        var sut = new MusicLibraryService(repoMock.Object);

        sut.GetAllTracks();

        repoMock.Verify(r => r.GetAllTracks(), Times.Once);
    }
}
```

---

### 9.11 Infra — `PlaylistServiceTests.cs`

File: `tests/AlconMusicPlayer.Infra.Tests/PlaylistServiceTests.cs`

```csharp
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Infra.Repositories;
using AlconMusicPlayer.Infra.Services;

namespace AlconMusicPlayer.Infra.Tests;

public class PlaylistServiceTests
{
    // ── helpers ───────────────────────────────────────────────────────────────
    private static Track MakeTrack(string title = "Track") =>
        Track.Create(title, $"{title}.mp3", Artist.Create("A"), Album.Create("B"), Genre.Pop, 120);

    /// <summary>Builds a service backed by real in-memory repos.</summary>
    private static (PlaylistService sut, InMemoryPlaylistRepository playlistRepo) MakeSut(
        params Track[] seedTracks)
    {
        var playlistRepo = new InMemoryPlaylistRepository();
        var trackRepo    = new InMemoryTrackRepository(seedTracks.ToList());
        var sut          = new PlaylistService(playlistRepo, trackRepo);
        return (sut, playlistRepo);
    }

    // ── AddPlaylist ───────────────────────────────────────────────────────────
    [Fact]
    public void AddPlaylist_Should_Persist_New_Playlist()
    {
        var (sut, _) = MakeSut();

        sut.AddPlaylist("Favorites");

        Assert.Single(sut.GetAllPlaylists());
        Assert.Equal("Favorites", sut.GetAllPlaylists().First().Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AddPlaylist_Should_Throw_When_Name_IsNullOrWhitespace(string? name)
    {
        var (sut, _) = MakeSut();

        Assert.Throws<ArgumentException>(() => sut.AddPlaylist(name!));
    }

    // ── GetAllPlaylists ───────────────────────────────────────────────────────
    [Fact]
    public void GetAllPlaylists_Should_Return_Empty_Initially()
    {
        var (sut, _) = MakeSut();

        Assert.Empty(sut.GetAllPlaylists());
    }

    // ── RemovePlaylist ────────────────────────────────────────────────────────
    [Fact]
    public void RemovePlaylist_Should_Delete_Playlist_By_Id()
    {
        var (sut, repo) = MakeSut();
        sut.AddPlaylist("P1");
        var playlist = repo.GetAllPlaylists().First();

        sut.RemovePlaylist(playlist.Id);

        Assert.Empty(sut.GetAllPlaylists());
    }

    // ── AddTrack ──────────────────────────────────────────────────────────────
    [Fact]
    public void AddTrack_Should_Add_Track_To_Playlist()
    {
        var track   = MakeTrack("Song A");
        var (sut, repo) = MakeSut(track);
        sut.AddPlaylist("P");
        var playlist = repo.GetAllPlaylists().First();

        sut.AddTrack(playlist.Id, track.Id);

        // Reload from repo to confirm persistence
        var stored = repo.GetPlaylistByID(playlist.Id)!;
        Assert.Single(stored.Tracks);
        Assert.Equal(track.Id, stored.Tracks[0].Id);
    }

    [Fact]
    public void AddTrack_Should_Throw_When_Playlist_Not_Found()
    {
        var track   = MakeTrack();
        var (sut, _) = MakeSut(track);

        Assert.Throws<InvalidOperationException>(() =>
            sut.AddTrack(Guid.NewGuid(), track.Id));
    }

    [Fact]
    public void AddTrack_Should_Throw_When_Track_Not_Found()
    {
        var (sut, repo) = MakeSut(); // no tracks seeded
        sut.AddPlaylist("P");
        var playlist = repo.GetAllPlaylists().First();

        Assert.Throws<InvalidOperationException>(() =>
            sut.AddTrack(playlist.Id, Guid.NewGuid()));
    }

    // ── RemoveTrack ───────────────────────────────────────────────────────────
    [Fact]
    public void RemoveTrack_Should_Remove_Track_From_Playlist()
    {
        var track   = MakeTrack("Song B");
        var (sut, repo) = MakeSut(track);
        sut.AddPlaylist("P");
        var playlist = repo.GetAllPlaylists().First();
        sut.AddTrack(playlist.Id, track.Id);

        sut.RemoveTrack(playlist.Id, track.Id);

        var stored = repo.GetPlaylistByID(playlist.Id)!;
        Assert.Empty(stored.Tracks);
    }

    [Fact]
    public void RemoveTrack_Should_Throw_When_Playlist_Not_Found()
    {
        var (sut, _) = MakeSut();

        Assert.Throws<InvalidOperationException>(() =>
            sut.RemoveTrack(Guid.NewGuid(), Guid.NewGuid()));
    }

    // ── RenamePlaylist ────────────────────────────────────────────────────────
    [Fact]
    public void RenamePlaylist_Should_Update_Name_In_Repository()
    {
        var (sut, repo) = MakeSut();
        sut.AddPlaylist("Old");
        var playlist = repo.GetAllPlaylists().First();

        sut.RenamePlaylist(playlist.Id, "New");

        var stored = repo.GetPlaylistByID(playlist.Id)!;
        Assert.Equal("New", stored.Name);
    }

    [Fact]
    public void RenamePlaylist_Should_Throw_When_Playlist_Not_Found()
    {
        var (sut, _) = MakeSut();

        Assert.Throws<InvalidOperationException>(() =>
            sut.RenamePlaylist(Guid.NewGuid(), "Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RenamePlaylist_Should_Throw_When_New_Name_IsNullOrWhitespace(string? name)
    {
        var (sut, repo) = MakeSut();
        sut.AddPlaylist("P");
        var playlist = repo.GetAllPlaylists().First();

        Assert.Throws<ArgumentException>(() => sut.RenamePlaylist(playlist.Id, name!));
    }
}
```

## 10. Naming Convention

Use this format:

- test class: `<ClassName>Tests`
- test method: `MethodName_Should_DoSomething_When_Condition`

Examples:

- `CreatePlaylistUseCaseTests`
- `Execute_Should_CreatePlaylist_When_NameIsValid`
- `PlayTrack_Should_SetCurrentTrack_When_TrackExists`

This makes test intent easy to read.

## 11. Arrange-Act-Assert Pattern

Follow this structure in every test:

### Arrange
Set up the objects, mocks, and input data.

### Act
Call the method being tested.

### Assert
Verify the result.

Example:

```csharp
[Fact]
public void Add_Should_Increase_Count_When_Item_Is_Valid()
{
    // Arrange

    // Act

    // Assert
}
```

## 12. When to Use Mocks

Use mocks when the class under test depends on:

- repositories
- services
- external systems
- anything not part of the logic being directly tested

Do not mock:

- simple value objects
- plain domain entities
- collections
- pure logic classes unless necessary

## 13. Run the Tests

Run all tests:

```powershell
dotnet test -f net8.0
```

Run a specific test project:

```powershell
dotnet test tests/AlconMusicPlayer.Domain.Tests/AlconMusicPlayer.Domain.Tests.csproj -f net8.0
```

Run with detailed output:

```powershell
dotnet test -f net8.0 --logger "console;verbosity=detailed"
```

## 14. Suggested Learning Path for This Solution

Follow this order:

### Step 1
Create test projects and add them to the solution.

### Step 2
Write tests for the use cases:

- `CreatePlaylistUseCase`
- `AddTrackToPlaylistUseCase`
- `RemoveTrackFromPlaylistUseCase`

### Step 3
Write tests for in-memory repositories and services.

### Step 4
Write ViewModel tests for:

- search/filter behavior in `LibraryViewModel`
- queue navigation in `NowPlayingViewModel`
- playlist navigation in `MainViewModel`

### Step 5
Add edge-case tests:

- null inputs
- empty playlist names
- duplicate additions
- removing non-existing tracks
- empty track library

## 15. Recommended Commit Strategy

Use small commits while learning.

Example sequence:

1. `Add xUnit test projects`
2. `Add project references for test projects`
3. `Add tests for CreatePlaylistUseCase`
4. `Add tests for AddTrackToPlaylistUseCase`
5. `Add tests for NowPlayingViewModel`

This makes review and rollback easier.

## 16. Common Mistakes to Avoid

- putting tests under `src/`
- testing UI rendering instead of ViewModel logic
- writing many assertions without clear intent
- testing multiple behaviors in one test
- depending on shared mutable test state
- using real infrastructure when a mock is enough

## 17. Final Recommendation for This Repository

Start with these three test projects first:

- `tests/AlconMusicPlayer.ApplicationService.Tests`
- `tests/AlconMusicPlayer.Domain.Tests`
- `tests/AlconMusicPlayer.Infra.Tests`

Add `WPF.Tests` only after the core layers are covered.

That gives the fastest learning and the best value.

---

If you want, the next step can be to scaffold the `tests/` folder, generate the xUnit projects, and wire them into the solution on the current branch.
