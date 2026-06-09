using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.Domain.Interfaces;
using AlconMusicPlayer.WPF.Tests.TestData;
using AlconMusicPlayer.WPF.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace AlconMusicPlayer.WPF.Tests;

public class PlaylistsViewModelTests
{
    [Fact]
    public void Constructor_LoadsExistingPlaylists()
    {
        var existing = new List<Playlist>
        {
            TestEntityFactory.CreatePlaylist("Favorites"),
            TestEntityFactory.CreatePlaylist("Chill")
        };

        var viewModel = CreateSut(existing);

        Assert.Equal(2, viewModel.Playlists.Count);
        Assert.Contains(viewModel.Playlists, playlist => playlist.Name == "Favorites");
        Assert.Contains(viewModel.Playlists, playlist => playlist.Name == "Chill");
    }

    [Fact]
    public void CreatePlaylistCommand_WithValidName_AddsPlaylistAndClearsFormState()
    {
        var playlists = new List<Playlist>();
        var viewModel = CreateSut(playlists);
        viewModel.NewPlaylistName = "Road Trip";

        Assert.True(viewModel.CreatePlaylistCommand.CanExecute(null));

        viewModel.CreatePlaylistCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.NewPlaylistName);
        Assert.Equal(string.Empty, viewModel.ErrorMessage);
        Assert.Contains(viewModel.Playlists, playlist => playlist.Name == "Road Trip");
    }

    [Fact]
    public void CreatePlaylistCommand_WhenUseCaseFails_SetsErrorMessage()
    {
        var playlists = new List<Playlist>
        {
            TestEntityFactory.CreatePlaylist("Road Trip")
        };
        var viewModel = CreateSut(playlists);
        viewModel.NewPlaylistName = "Road Trip";

        viewModel.CreatePlaylistCommand.Execute(null);

        Assert.Equal("A playlist named \"Road Trip\" already exists.", viewModel.ErrorMessage);
        Assert.Single(viewModel.Playlists);
    }

    private static PlaylistsViewModel CreateSut(List<Playlist> playlistStore)
    {
        var playlistService = new Mock<IPlaylistService>();
        playlistService.Setup(service => service.GetAllPlaylists()).Returns(() => playlistStore.ToList());

        var playlistRepository = new Mock<IPlaylistRepository>();
        playlistRepository.Setup(repository => repository.GetAllPlaylists()).Returns(() => playlistStore.ToList());
        playlistRepository
            .Setup(repository => repository.AddPlaylist(It.IsAny<Playlist>()))
            .Callback<Playlist>(playlist => playlistStore.Add(playlist));

        var createPlaylistUseCase = new CreatePlaylistUseCase(
            playlistRepository.Object,
            Mock.Of<ILogger<CreatePlaylistUseCase>>());

        return new PlaylistsViewModel(
            playlistService.Object,
            createPlaylistUseCase,
            Mock.Of<ILogger<PlaylistsViewModel>>());
    }
}
