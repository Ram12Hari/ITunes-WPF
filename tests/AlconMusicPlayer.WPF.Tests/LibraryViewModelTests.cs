using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using AlconMusicPlayer.WPF.Tests.TestData;
using AlconMusicPlayer.WPF.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace AlconMusicPlayer.WPF.Tests;

public class LibraryViewModelTests
{
    [Fact]
    public void Constructor_LoadsTracksAndPlaylists()
    {
        var harness = CreateHarness();

        Assert.Equal(3, harness.ViewModel.Tracks.Count);
        Assert.Single(harness.ViewModel.AvailablePlaylists);
    }

    [Fact]
    public void FilterByArtist_AppliesArtistFilterAndSetsActiveLabel()
    {
        var harness = CreateHarness();
        var targetArtist = harness.Tracks[0].Artist;
        harness.ViewModel.SearchText = "Album Two";

        harness.ViewModel.FilterByArtist(targetArtist);

        Assert.Equal(string.Empty, harness.ViewModel.SearchText);
        Assert.Equal($"Artist: {targetArtist.Name}", harness.ViewModel.ActiveFilterLabel);
        Assert.All(harness.ViewModel.Tracks, track => Assert.Equal(targetArtist.Id, track.Artist.Id));
    }

    [Fact]
    public void SearchText_FiltersTracksCaseInsensitive()
    {
        var harness = CreateHarness();

        harness.ViewModel.SearchText = "second";

        var track = Assert.Single(harness.ViewModel.Tracks);
        Assert.Equal("Second Song", track.Title);
    }

    [Fact]
    public void AddToPlaylistCommand_WithSelectedTrack_AddsTrackAndClearsError()
    {
        var harness = CreateHarness(withEmptyPlaylist: true);
        var track = harness.Tracks[0];
        var playlist = Assert.Single(harness.Playlists);
        harness.ViewModel.SelectedTrack = track;

        harness.ViewModel.AddToPlaylistCommand.Execute(playlist);

        Assert.Equal(string.Empty, harness.ViewModel.ErrorMessage);
        Assert.Contains(track, playlist.Tracks);
    }

    [Fact]
    public void PlayTrackCommand_DelegatesToNowPlayingViewModel()
    {
        var harness = CreateHarness();
        var track = harness.Tracks[2];

        harness.ViewModel.PlayTrackCommand.Execute(track);

        Assert.Same(track, harness.NowPlayingViewModel.CurrentTrack);
        Assert.True(harness.NowPlayingViewModel.IsPlaying);
    }

    private static LibraryHarness CreateHarness(bool withEmptyPlaylist = false)
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
            withEmptyPlaylist
                ? TestEntityFactory.CreatePlaylist("Favorites")
                : TestEntityFactory.CreatePlaylist("Favorites", tracks[1])
        };

        var musicService = new Mock<IMusicLibraryService>();
        musicService.Setup(service => service.GetAllTracks()).Returns(tracks);
        musicService.Setup(service => service.GetAllArtists()).Returns([artistOne, artistTwo]);
        musicService.Setup(service => service.GetAllAlbums()).Returns([albumOne, albumTwo]);

        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(service => service.GetAllPlaylists()).Returns(() => playlists.ToList());

        var playlistRepository = new Mock<IPlaylistRepository>();
        playlistRepository.Setup(repository => repository.GetAllPlaylists()).Returns(() => playlists.ToList());
        playlistRepository.Setup(repository => repository.GetPlaylistByID(It.IsAny<Guid>()))
            .Returns<Guid>(id => playlists.SingleOrDefault(playlist => playlist.Id == id));
        playlistRepository.Setup(repository => repository.UpdatePlaylist(It.IsAny<Playlist>()));
        playlistRepository.Setup(repository => repository.AddPlaylist(It.IsAny<Playlist>()))
            .Callback<Playlist>(playlist => playlists.Add(playlist));

        var trackRepository = new Mock<ITrackRepository>();
        trackRepository.Setup(repository => repository.GetTrackById(It.IsAny<Guid>()))
            .Returns<Guid>(id => tracks.SingleOrDefault(track => track.Id == id));

        var nowPlayingViewModel = new NowPlayingViewModel(musicService.Object);
        var createPlaylistUseCase = new CreatePlaylistUseCase(playlistRepository.Object, Mock.Of<ILogger<CreatePlaylistUseCase>>());
        var addTrackUseCase = new AddTrackToPlaylistUseCase(
            playlistRepository.Object,
            trackRepository.Object,
            Mock.Of<ILogger<AddTrackToPlaylistUseCase>>());

        var viewModel = new LibraryViewModel(
            musicService.Object,
            playlistService.Object,
            createPlaylistUseCase,
            addTrackUseCase,
            nowPlayingViewModel,
            Mock.Of<ILogger<LibraryViewModel>>());

        return new LibraryHarness(viewModel, nowPlayingViewModel, tracks, playlists);
    }

    private sealed record LibraryHarness(
        LibraryViewModel ViewModel,
        NowPlayingViewModel NowPlayingViewModel,
        List<Track> Tracks,
        List<Playlist> Playlists);
}
