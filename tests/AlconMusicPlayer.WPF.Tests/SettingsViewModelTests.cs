using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.WPF.ViewModels;
using Moq;

namespace AlconMusicPlayer.WPF.Tests;

public class SettingsViewModelTests
{
    [Fact]
    public void Constructor_InitializesThemeStateFromService()
    {
        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");
        themeService.SetupGet(service => service.AvailableThemes).Returns(["Light", "Dark"]);

        var viewModel = new SettingsViewModel(themeService.Object);

        Assert.Equal("Dark", viewModel.SelectedTheme);
        Assert.Equal(["Light", "Dark"], viewModel.AvailableThemes);
    }

    [Fact]
    public void SelectedTheme_WhenChanged_CallsThemeService()
    {
        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");
        themeService.SetupGet(service => service.AvailableThemes).Returns(["Light", "Dark"]);
        var viewModel = new SettingsViewModel(themeService.Object);

        viewModel.SelectedTheme = "Light";

        Assert.Equal("Light", viewModel.SelectedTheme);
        themeService.Verify(service => service.SetTheme("Light"), Times.Once);
    }

    [Fact]
    public void ResetCommand_SetsLightTheme()
    {
        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");
        themeService.SetupGet(service => service.AvailableThemes).Returns(["Light", "Dark"]);
        var viewModel = new SettingsViewModel(themeService.Object);

        viewModel.ResetCommand.Execute(null);

        Assert.Equal("Light", viewModel.SelectedTheme);
        themeService.Verify(service => service.SetTheme("Light"), Times.Once);
    }

    [Fact]
    public void SelectedTheme_WhenUnchanged_DoesNotCallThemeService()
    {
        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");
        themeService.SetupGet(service => service.AvailableThemes).Returns(["Light", "Dark"]);
        var viewModel = new SettingsViewModel(themeService.Object);

        viewModel.SelectedTheme = "Dark";

        themeService.Verify(service => service.SetTheme(It.IsAny<string>()), Times.Never);
    }
}
