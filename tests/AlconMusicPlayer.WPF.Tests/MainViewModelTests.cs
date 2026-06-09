using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using AlconMusicPlayer.WPF.Tests.TestData;
using AlconMusicPlayer.WPF.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace AlconMusicPlayer.WPF.Tests;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_SetsLibraryAsDefaultView()
    {
        var harness = CreateHarness();

        Assert.Same(harness.LibraryViewModel, harness.MainViewModel.CurrentView);
    }

    [Fact]
    public void ShowPlaylistsCommand_ReloadsPlaylistsAndShowsPlaylistsView()
    {
        var harness = CreateHarness();
        var extraPlaylist = TestEntityFactory.CreatePlaylist("Gym Mix");
        harness.Playlists.Add(extraPlaylist);
        harness.PlaylistsViewModel.SelectedPlaylist = harness.Playlists[0];

        harness.MainViewModel.ShowPlaylistsCommand.Execute(null);

        Assert.Same(harness.PlaylistsViewModel, harness.MainViewModel.CurrentView);
        Assert.Null(harness.PlaylistsViewModel.SelectedPlaylist);
        Assert.Contains(harness.PlaylistsViewModel.Playlists, playlist => playlist.Name == "Gym Mix");
    }

    [Fact]
    public void SelectingArtist_FiltersLibraryAndReturnsToLibraryView()
    {
        var harness = CreateHarness();
        var artist = harness.Tracks[0].Artist;

        harness.ArtistsViewModel.SelectedArtist = artist;

        Assert.Same(harness.LibraryViewModel, harness.MainViewModel.CurrentView);
        Assert.Equal($"Artist: {artist.Name}", harness.LibraryViewModel.ActiveFilterLabel);
        Assert.All(harness.LibraryViewModel.Tracks, track => Assert.Equal(artist.Id, track.Artist.Id));
    }

    [Fact]
    public void SelectingPlaylist_LoadsPlaylistView_AndBackReturnsToPlaylists()
    {
        var harness = CreateHarness();
        var playlist = harness.Playlists[0];

        harness.PlaylistsViewModel.SelectedPlaylist = playlist;

        Assert.Same(harness.PlaylistViewModel, harness.MainViewModel.CurrentView);
        Assert.Equal(playlist.Name, harness.PlaylistViewModel.PlaylistName);

        harness.PlaylistViewModel.BackCommand.Execute(null);

        Assert.Same(harness.PlaylistsViewModel, harness.MainViewModel.CurrentView);
        Assert.Null(harness.PlaylistsViewModel.SelectedPlaylist);
    }

    private static MainHarness CreateHarness()
    {
        var artistOne = TestEntityFactory.CreateArtist("Artist One");
        var artistTwo = TestEntityFactory.CreateArtist("Artist Two");
        var albumOne = TestEntityFactory.CreateAlbum("Album One");
        var albumTwo = TestEntityFactory.CreateAlbum("Album Two");
        var tracks = new List<Track>
        {
            TestEntityFactory.CreateTrack("First Song", artistOne, albumOne),
            TestEntityFactory.CreateTrack("Second Song", artistTwo, albumTwo),
            TestEntityFactory.CreateTrack("Third Song", artistOne, albumTwo)
        };
        var playlists = new List<Playlist>
        {
            TestEntityFactory.CreatePlaylist("Favorites", tracks[0])
        };

        var musicService = new Mock<IMusicLibraryService>();
        musicService.Setup(service => service.GetAllTracks()).Returns(tracks);
        musicService.Setup(service => service.GetAllArtists()).Returns([artistOne, artistTwo]);
        musicService.Setup(service => service.GetAllAlbums()).Returns([albumOne, albumTwo]);

        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(service => service.GetAllPlaylists()).Returns(() => playlists.ToList());

        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");
        themeService.SetupGet(service => service.AvailableThemes).Returns(["Light", "Dark"]);

        var playlistRepository = new Mock<IPlaylistRepository>();
        playlistRepository.Setup(repository => repository.GetAllPlaylists()).Returns(() => playlists.ToList());
        playlistRepository.Setup(repository => repository.GetPlaylistByID(It.IsAny<Guid>()))
            .Returns<Guid>(id => playlists.SingleOrDefault(playlist => playlist.Id == id));
        playlistRepository.Setup(repository => repository.AddPlaylist(It.IsAny<Playlist>()))
            .Callback<Playlist>(playlist => playlists.Add(playlist));
        playlistRepository.Setup(repository => repository.UpdatePlaylist(It.IsAny<Playlist>()));

        var trackRepository = new Mock<ITrackRepository>();
        trackRepository.Setup(repository => repository.GetTrackById(It.IsAny<Guid>()))
            .Returns<Guid>(id => tracks.SingleOrDefault(track => track.Id == id));

        var createPlaylistUseCase = new CreatePlaylistUseCase(playlistRepository.Object, Mock.Of<ILogger<CreatePlaylistUseCase>>());
        var addTrackUseCase = new AddTrackToPlaylistUseCase(
            playlistRepository.Object,
            trackRepository.Object,
            Mock.Of<ILogger<AddTrackToPlaylistUseCase>>());
        var removeTrackUseCase = new RemoveTrackFromPlaylistUseCase(
            playlistRepository.Object,
            Mock.Of<ILogger<RemoveTrackFromPlaylistUseCase>>());

        var nowPlayingViewModel = new NowPlayingViewModel(musicService.Object);
        var libraryViewModel = new LibraryViewModel(
            musicService.Object,
            playlistService.Object,
            createPlaylistUseCase,
            addTrackUseCase,
            nowPlayingViewModel,
            Mock.Of<ILogger<LibraryViewModel>>());
        var artistsViewModel = new ArtistsViewModel(musicService.Object);
        var albumsViewModel = new AlbumsViewModel(musicService.Object);
        var playlistsViewModel = new PlaylistsViewModel(
            playlistService.Object,
            createPlaylistUseCase,
            Mock.Of<ILogger<PlaylistsViewModel>>());
        var playlistViewModel = new PlaylistViewModel(
            addTrackUseCase,
            removeTrackUseCase,
            nowPlayingViewModel,
            Mock.Of<ILogger<PlaylistViewModel>>());
        var settingsViewModel = new SettingsViewModel(themeService.Object);

        var mainViewModel = new MainViewModel(
            libraryViewModel,
            artistsViewModel,
            albumsViewModel,
            playlistsViewModel,
            playlistViewModel,
            nowPlayingViewModel,
            settingsViewModel);

        return new MainHarness(
            mainViewModel,
            libraryViewModel,
            artistsViewModel,
            playlistsViewModel,
            playlistViewModel,
            tracks,
            playlists);
    }

    private sealed record MainHarness(
        MainViewModel MainViewModel,
        LibraryViewModel LibraryViewModel,
        ArtistsViewModel ArtistsViewModel,
        PlaylistsViewModel PlaylistsViewModel,
        PlaylistViewModel PlaylistViewModel,
        List<Track> Tracks,
        List<Playlist> Playlists);
}
