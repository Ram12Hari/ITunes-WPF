using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using AlconMusicPlayer.WPF.Tests.TestData;
using AlconMusicPlayer.WPF.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace AlconMusicPlayer.WPF.Tests;

public class PlaylistViewModelTests
{
    [Fact]
    public void Load_PopulatesPlaylistNameAndTracks()
    {
        var track = TestEntityFactory.CreateTrack("Loaded Song");
        var playlist = TestEntityFactory.CreatePlaylist("Loaded Playlist", track);
        var viewModel = CreateSut(playlist, [track]);

        viewModel.Load(playlist);

        Assert.Equal("Loaded Playlist", viewModel.PlaylistName);
        var loadedTrack = Assert.Single(viewModel.Tracks);
        Assert.Same(track, loadedTrack);
    }

    [Fact]
    public void AddTrack_WhenUseCaseSucceeds_ReloadsTracks()
    {
        var track = TestEntityFactory.CreateTrack("New Track");
        var playlist = TestEntityFactory.CreatePlaylist("Favorites");
        var viewModel = CreateSut(playlist, [track]);
        viewModel.Load(playlist);

        viewModel.AddTrack(track.Id);

        Assert.Equal(string.Empty, viewModel.ErrorMessage);
        var addedTrack = Assert.Single(viewModel.Tracks);
        Assert.Same(track, addedTrack);
    }

    [Fact]
    public void RemoveTrackCommand_WhenTrackSelected_RemovesTrack()
    {
        var track = TestEntityFactory.CreateTrack("Existing Track");
        var playlist = TestEntityFactory.CreatePlaylist("Favorites", track);
        var viewModel = CreateSut(playlist, [track]);
        viewModel.Load(playlist);
        viewModel.SelectedTrack = track;

        Assert.True(viewModel.RemoveTrackCommand.CanExecute(null));

        viewModel.RemoveTrackCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.ErrorMessage);
        Assert.Empty(viewModel.Tracks);
    }

    [Fact]
    public void BackCommand_RaisesBackRequested()
    {
        var playlist = TestEntityFactory.CreatePlaylist("Back Test");
        var viewModel = CreateSut(playlist, []);
        var backRequested = false;
        viewModel.BackRequested += () => backRequested = true;

        viewModel.BackCommand.Execute(null);

        Assert.True(backRequested);
    }

    private static PlaylistViewModel CreateSut(Playlist playlist, IReadOnlyList<Track> libraryTracks)
    {
        var playlistRepository = new Mock<IPlaylistRepository>();
        playlistRepository.Setup(repository => repository.GetPlaylistByID(playlist.Id)).Returns(playlist);
        playlistRepository.Setup(repository => repository.UpdatePlaylist(It.IsAny<Playlist>()));

        var trackRepository = new Mock<ITrackRepository>();
        trackRepository.Setup(repository => repository.GetTrackById(It.IsAny<Guid>()))
            .Returns<Guid>(id => libraryTracks.SingleOrDefault(track => track.Id == id));

        var musicService = new Mock<IMusicLibraryService>();
        musicService.Setup(service => service.GetAllTracks()).Returns(libraryTracks);
        var nowPlayingViewModel = new NowPlayingViewModel(musicService.Object);

        var addTrackUseCase = new AddTrackToPlaylistUseCase(
            playlistRepository.Object,
            trackRepository.Object,
            Mock.Of<ILogger<AddTrackToPlaylistUseCase>>());
        var removeTrackUseCase = new RemoveTrackFromPlaylistUseCase(
            playlistRepository.Object,
            Mock.Of<ILogger<RemoveTrackFromPlaylistUseCase>>());

        return new PlaylistViewModel(
            addTrackUseCase,
            removeTrackUseCase,
            nowPlayingViewModel,
            Mock.Of<ILogger<PlaylistViewModel>>());
    }
}
