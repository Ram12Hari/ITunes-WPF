using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Interfaces;
using AlconMusicPlayer.Infra.Data;
using AlconMusicPlayer.Infra.Repositories;
using AlconMusicPlayer.Infra.Services;
using AlconMusicPlayer.WPF.Logging;
using AlconMusicPlayer.WPF.Services;
using AlconMusicPlayer.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;

namespace AlconMusicPlayer.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        RegisterService(services);
        _serviceProvider = services.BuildServiceProvider();

        var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Application Starting up - version {Version}", typeof(App).Assembly.GetName().Version);


        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    private static void RegisterService(IServiceCollection serviceProvider)
    {
        var logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "alcon-music-player.log");
        serviceProvider.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddProvider(new ConsoleLoggerProvider(LogLevel.Information));
            builder.AddProvider(new FileLoggerProvider(logFilePath, LogLevel.Information));
        }
        );


        // ADR-002: SeedData.Build() returns a tuple — destructure to get only tracks
        var tracks = SeedData.Build();

        // Repositories
        serviceProvider.AddSingleton<ITrackRepository>(new InMemoryTrackRepository(tracks.ToList()));
        serviceProvider.AddSingleton<IPlaylistRepository, InMemoryPlaylistRepository>();

        // Services
        serviceProvider.AddSingleton<IMusicLibraryService, MusicLibraryService>();
        serviceProvider.AddSingleton<IPlaylistService, PlaylistService>();

        // Theme service — singleton so theme persists across navigation
        serviceProvider.AddSingleton<IThemeService, ThemeService>();

        // Use cases — ADR-018: only playlist-mutation use cases with real business rules
        serviceProvider.AddTransient<CreatePlaylistUseCase>();
        serviceProvider.AddTransient<AddTrackToPlaylistUseCase>();
        serviceProvider.AddTransient<RemoveTrackFromPlaylistUseCase>();

        // ViewModels — all Singleton because:
        // 1. MainViewModel (singleton) captures every child VM via constructor injection;
        //    a Transient child held by a Singleton is never recreated (captive dependency).
        // 2. Each VM carries UI state (selected items, search text, loaded data, event
        //    subscriptions from MainViewModel) that must survive navigation switches.
        serviceProvider.AddSingleton<MainViewModel>();
        serviceProvider.AddSingleton<LibraryViewModel>();
        serviceProvider.AddSingleton<ArtistsViewModel>();
        serviceProvider.AddSingleton<AlbumsViewModel>();
        serviceProvider.AddSingleton<PlaylistsViewModel>();
        serviceProvider.AddSingleton<PlaylistViewModel>();
        serviceProvider.AddSingleton<SettingsViewModel>();
        serviceProvider.AddSingleton<NowPlayingViewModel>();

        serviceProvider.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var logger = _serviceProvider.GetService<ILogger<App>>();
        logger?.LogInformation("Application shutting down.");
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}

