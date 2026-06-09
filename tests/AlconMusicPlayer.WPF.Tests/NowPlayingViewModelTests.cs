using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.Tests.TestData;
using AlconMusicPlayer.WPF.ViewModels;
using Moq;

namespace AlconMusicPlayer.WPF.Tests;

public class NowPlayingViewModelTests
{
    [Fact]
    public void Constructor_WithTracks_LoadsFirstTrackAndStartsPlayback()
    {
        var tracks = CreateQueue();
        var musicService = new Mock<IMusicLibraryService>();
        musicService.Setup(service => service.GetAllTracks()).Returns(tracks);

        var viewModel = new NowPlayingViewModel(musicService.Object);

        Assert.Same(tracks[0], viewModel.CurrentTrack);
        Assert.True(viewModel.IsPlaying);
        Assert.Equal($"{tracks[0].Title} • {tracks[0].Artist.Name}", viewModel.NowPlayingDisplay);
    }

    [Fact]
    public void PlayTrack_SetsQueueAndAllowsNextAndPreviousNavigation()
    {
        var tracks = CreateQueue();
        var musicService = new Mock<IMusicLibraryService>();
        musicService.Setup(service => service.GetAllTracks()).Returns(tracks);
        var viewModel = new NowPlayingViewModel(musicService.Object);

        viewModel.PlayTrack(tracks[1], tracks);

        Assert.Same(tracks[1], viewModel.CurrentTrack);
        Assert.True(viewModel.NextCommand.CanExecute(null));
        Assert.True(viewModel.PreviousCommand.CanExecute(null));

        viewModel.NextCommand.Execute(null);
        Assert.Same(tracks[2], viewModel.CurrentTrack);

        viewModel.PreviousCommand.Execute(null);
        Assert.Same(tracks[1], viewModel.CurrentTrack);
    }

    [Fact]
    public void PauseAndPlayCommands_TogglePlaybackState()
    {
        var tracks = CreateQueue();
        var musicService = new Mock<IMusicLibraryService>();
        musicService.Setup(service => service.GetAllTracks()).Returns(tracks);
        var viewModel = new NowPlayingViewModel(musicService.Object);

        viewModel.PauseCommand.Execute(null);
        Assert.False(viewModel.IsPlaying);

        viewModel.PlayCommand.Execute(null);
        Assert.True(viewModel.IsPlaying);
    }

    private static List<Track> CreateQueue()
    {
        var artist = TestEntityFactory.CreateArtist("Queue Artist");
        var album = TestEntityFactory.CreateAlbum("Queue Album");

        return
        [
            TestEntityFactory.CreateTrack("Track One", artist, album),
            TestEntityFactory.CreateTrack("Track Two", artist, album),
            TestEntityFactory.CreateTrack("Track Three", artist, album)
        ];
    }
}
