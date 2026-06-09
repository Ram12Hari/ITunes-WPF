# Logging Implementation Guide — Alcon Music Player

> **Goal**: Add structured, extensible logging to console and file using `Microsoft.Extensions.Logging`
> abstractions. The design is open for extension (Event Viewer, TraceSource, Seq, etc.) without
> modifying any existing code.

---

## Design Principles

### Why `Microsoft.Extensions.Logging`?
The `ILogger<T>` / `ILoggerProvider` abstractions from `Microsoft.Extensions.Logging` are already
available transitively via the existing `Microsoft.Extensions.DependencyInjection` package in the
WPF project. Using this abstraction means:

- **Open/Closed Principle** — add new sinks (Event Viewer, Seq, etc.) by registering a new
  `ILoggerProvider`. No existing code changes required.
- **Dependency Inversion** — use cases and ViewModels depend on `ILogger<T>` (abstraction),
  never on a concrete file writer or console writer.
- **Single Responsibility** — each `ILoggerProvider` owns one output destination only.
- **Zero production overhead** — if logging is disabled for a level, the message is never formatted.

### Extension Model

```
ILogger<T>  ←  injected into use cases / ViewModels
     │
     ▼
LoggerFactory  (built by services.AddLogging())
     │
     ├── FileLoggerProvider       → logs/alcon-music-player.log
     ├── ConsoleLoggerProvider    → stdout
     │
     │   (future — register without touching existing code)
     ├── EventViewerLoggerProvider
     └── TraceSourceLoggerProvider
```

---

## Prerequisites

- .NET 8 SDK
- `Microsoft.Extensions.DependencyInjection` v10+ already referenced in `AlconMusicPlayer.WPF`
- No additional NuGet packages needed in the WPF project (logging comes transitively)
- One new first-party NuGet package needed in `AlconMusicPlayer.ApplicationService` (see Step 1)

---

## Step 1 — Add `Microsoft.Extensions.Logging.Abstractions` to ApplicationService

Use cases live in `AlconMusicPlayer.ApplicationService`, which currently has **no** NuGet packages.
To accept `ILogger<T>` as a constructor parameter, that project needs the abstractions package.

Edit [src/AlconMusicPlayer.ApplicationService/AlconMusicPlayer.ApplicationService.csproj](src/AlconMusicPlayer.ApplicationService/AlconMusicPlayer.ApplicationService.csproj):

```xml
<ItemGroup>
  <!-- Interfaces only — no runtime logging implementation pulled in here -->
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
</ItemGroup>
```

> **Why Abstractions only?** The ApplicationService layer must not know about concrete sinks
> (file, console). The Abstractions package contains only interfaces — `ILogger<T>`, `ILoggerFactory`,
> `LogLevel`, `NullLogger<T>` — keeping the dependency clean.

Verify restore:
```bash
dotnet restore src/AlconMusicPlayer.ApplicationService/AlconMusicPlayer.ApplicationService.csproj
```

---

## Step 2 — Create `AppLogLevel.cs` (optional shared enum)

> Skip this step if you are happy using `Microsoft.Extensions.Logging.LogLevel` directly everywhere.

This is only useful if you want to decouple domain/application layers from `Microsoft.Extensions.Logging`
entirely in future. For now, using `LogLevel` directly in the ApplicationService is acceptable because
the Abstractions package is already referenced.

---

## Step 3 — Create `ConsoleLoggerProvider`

**New file**: `src/AlconMusicPlayer.WPF/Logging/ConsoleLoggerProvider.cs`

```csharp
using Microsoft.Extensions.Logging;

namespace AlconMusicPlayer.WPF.Logging;

/// <summary>
/// Writes log entries to stdout. One provider = one output sink (SRP).
/// Register additional ILoggerProvider implementations to add more sinks — no code here changes.
/// </summary>
public sealed class ConsoleLoggerProvider : ILoggerProvider
{
    private readonly LogLevel _minimumLevel;

    public ConsoleLoggerProvider(LogLevel minimumLevel = LogLevel.Information)
    {
        _minimumLevel = minimumLevel;
    }

    public ILogger CreateLogger(string categoryName) =>
        new ConsoleLogger(categoryName, _minimumLevel);

    public void Dispose() { }
}

internal sealed class ConsoleLogger : ILogger
{
    private readonly string _category;
    private readonly LogLevel _minimumLevel;

    internal ConsoleLogger(string category, LogLevel minimumLevel)
    {
        _category = category;
        _minimumLevel = minimumLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        // Short category name — trim namespace prefix for readability
        var shortCategory = _category.Contains('.')
            ? _category[((_category.LastIndexOf('.') + 1))..]
            : _category;

        var message = formatter(state, exception);
        var line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{LevelLabel(logLevel)}] {shortCategory}: {message}";

        Console.ForegroundColor = LevelColor(logLevel);
        Console.WriteLine(line);
        if (exception is not null)
            Console.WriteLine(exception);
        Console.ResetColor();
    }

    private static string LevelLabel(LogLevel level) => level switch
    {
        LogLevel.Trace       => "TRC",
        LogLevel.Debug       => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning     => "WRN",
        LogLevel.Error       => "ERR",
        LogLevel.Critical    => "CRT",
        _                    => "???"
    };

    private static ConsoleColor LevelColor(LogLevel level) => level switch
    {
        LogLevel.Warning  => ConsoleColor.Yellow,
        LogLevel.Error    => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        _                 => ConsoleColor.Gray
    };
}
```

---

## Step 4 — Create `FileLoggerProvider`

**New file**: `src/AlconMusicPlayer.WPF/Logging/FileLoggerProvider.cs`

```csharp
using Microsoft.Extensions.Logging;

namespace AlconMusicPlayer.WPF.Logging;

/// <summary>
/// Appends log entries to a single rolling text file.
/// Append-per-write (no open handle) keeps disposal trivial and avoids file-locking issues.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly LogLevel _minimumLevel;

    public FileLoggerProvider(string filePath, LogLevel minimumLevel = LogLevel.Information)
    {
        _filePath = filePath;
        _minimumLevel = minimumLevel;

        // Ensure the log directory exists before any write attempt
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, _filePath, _minimumLevel);

    public void Dispose() { }
}

internal sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly string _filePath;
    private readonly LogLevel _minimumLevel;

    // Static lock shared across all FileLogger instances — all target the same file
    private static readonly Lock _lock = new();

    internal FileLogger(string category, string filePath, LogLevel minimumLevel)
    {
        _category = category;
        _filePath = filePath;
        _minimumLevel = minimumLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var shortCategory = _category.Contains('.')
            ? _category[((_category.LastIndexOf('.') + 1))..]
            : _category;

        var message = formatter(state, exception);
        var line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{LevelLabel(logLevel)}] {shortCategory}: {message}";
        if (exception is not null)
            line += Environment.NewLine + exception;

        lock (_lock)
            File.AppendAllText(_filePath, line + Environment.NewLine);
    }

    private static string LevelLabel(LogLevel level) => level switch
    {
        LogLevel.Trace       => "TRC",
        LogLevel.Debug       => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning     => "WRN",
        LogLevel.Error       => "ERR",
        LogLevel.Critical    => "CRT",
        _                    => "???"
    };
}
```

> **Future sink example — Event Viewer**: Create `EventViewerLoggerProvider` implementing
> `ILoggerProvider`. Its `CreateLogger()` returns an `EventViewerLogger` that writes to
> `System.Diagnostics.EventLog`. Register it in `App.xaml.cs` alongside the existing providers.
> **Zero changes to `FileLoggerProvider`, `ConsoleLoggerProvider`, or any use case.**

---

## Step 5 — Wire Providers in `App.xaml.cs`

Edit [src/AlconMusicPlayer.WPF/App.xaml.cs](src/AlconMusicPlayer.WPF/App.xaml.cs):

```csharp
using Microsoft.Extensions.Logging;
// ...existing usings...

public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        RegisterService(services);
        _serviceProvider = services.BuildServiceProvider();

        // ADR-020: Log lifecycle events at the composition root, not inside business classes
        var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Application starting up — version {Version}",
            typeof(App).Assembly.GetName().Version);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    private static void RegisterService(IServiceCollection services)
    {
        // Logging — register providers here; call AddProvider() for each new sink
        var logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "alcon-music-player.log");
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddProvider(new ConsoleLoggerProvider(LogLevel.Information));
            builder.AddProvider(new FileLoggerProvider(logFilePath, LogLevel.Information));
            // To add Event Viewer in future:
            // builder.AddProvider(new EventViewerLoggerProvider(...));
        });

        // ...rest of existing registrations unchanged...
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var logger = _serviceProvider.GetService<ILogger<App>>();
        logger?.LogInformation("Application shutting down.");

        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
```

---

## Step 6 — Inject `ILogger<T>` into Use Cases

Use cases are the right place to log business events (playlist created, track added/removed).
Errors are re-thrown — the *caller* (ViewModel) decides whether to swallow or surface them.

### `CreatePlaylistUseCase.cs`

```csharp
using Microsoft.Extensions.Logging;

public class CreatePlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ILogger<CreatePlaylistUseCase> _logger;

    public CreatePlaylistUseCase(
        IPlaylistRepository playlistRepository,
        ILogger<CreatePlaylistUseCase> logger)
    {
        _playlistRepository = playlistRepository;
        _logger = logger;
    }

    public void Execute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Playlist name must not be empty.", nameof(name));

        bool duplicate = _playlistRepository
            .GetAllPlaylists()
            .Any(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            _logger.LogWarning("Duplicate playlist name rejected: '{Name}'", name.Trim());
            throw new InvalidOperationException($"A playlist named \"{name}\" already exists.");
        }

        var playlist = Playlist.Create(name.Trim());
        _playlistRepository.AddPlaylist(playlist);
        _logger.LogInformation("Playlist created: '{Name}' (ID: {Id})", playlist.Name, playlist.Id);
    }
}
```

### `AddTrackToPlaylistUseCase.cs`

```csharp
using Microsoft.Extensions.Logging;

public class AddTrackToPlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ITrackRepository _trackRepository;
    private readonly ILogger<AddTrackToPlaylistUseCase> _logger;

    public AddTrackToPlaylistUseCase(
        IPlaylistRepository playlistRepository,
        ITrackRepository trackRepository,
        ILogger<AddTrackToPlaylistUseCase> logger)
    {
        _playlistRepository = playlistRepository;
        _trackRepository = trackRepository;
        _logger = logger;
    }

    public void Execute(Guid playlistId, Guid trackId)
    {
        var playlist = _playlistRepository.GetPlaylistByID(playlistId)
            ?? throw new InvalidOperationException($"Playlist {playlistId} not found.");

        var track = _trackRepository.GetTrackById(trackId)
            ?? throw new InvalidOperationException($"Track {trackId} not found.");

        playlist.AddTrack(track);
        _playlistRepository.UpdatePlaylist(playlist);
        _logger.LogInformation("Track '{Title}' added to playlist '{Playlist}'",
            track.Title, playlist.Name);
    }
}
```

### `RemoveTrackFromPlaylistUseCase.cs`

```csharp
using Microsoft.Extensions.Logging;

public class RemoveTrackFromPlaylistUseCase
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ILogger<RemoveTrackFromPlaylistUseCase> _logger;

    public RemoveTrackFromPlaylistUseCase(
        IPlaylistRepository playlistRepository,
        ILogger<RemoveTrackFromPlaylistUseCase> logger)
    {
        _playlistRepository = playlistRepository;
        _logger = logger;
    }

    public void Execute(Guid playlistId, Guid trackId)
    {
        var playlist = _playlistRepository.GetPlaylistByID(playlistId)
            ?? throw new InvalidOperationException($"Playlist {playlistId} not found.");

        playlist.RemoveTrack(trackId);
        _playlistRepository.UpdatePlaylist(playlist);
        _logger.LogInformation("Track {TrackId} removed from playlist '{Playlist}'",
            trackId, playlist.Name);
    }
}
```

---

## Step 7 — Log Errors in ViewModels

ViewModels already catch exceptions to surface `ErrorMessage` to the UI. Add logging there —
do **not** add logging in both the use case *and* the ViewModel for the same exception.
Use cases log *business warnings* (duplicate name); ViewModels log *unexpected errors*.

### Example — `PlaylistsViewModel.cs`

```csharp
public class PlaylistsViewModel : ViewModelBase
{
    private readonly IPlaylistService _playlistService;
    private readonly CreatePlaylistUseCase _createPlaylistUseCase;
    private readonly ILogger<PlaylistsViewModel> _logger;

    public PlaylistsViewModel(
        IPlaylistService playlistService,
        CreatePlaylistUseCase createPlaylistUseCase,
        ILogger<PlaylistsViewModel> logger)
    {
        _playlistService = playlistService;
        _createPlaylistUseCase = createPlaylistUseCase;
        _logger = logger;
        // ...rest unchanged...
    }

    private void CreatePlaylist()
    {
        try
        {
            _createPlaylistUseCase.Execute(NewPlaylistName);
            NewPlaylistName = "";
            ErrorMessage = "";
            Reload();
        }
        catch (InvalidOperationException ex)
        {
            // Business rule violation — use case already logged the warning
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            // Unexpected error — log it here
            _logger.LogError(ex, "Unexpected error creating playlist '{Name}'", NewPlaylistName);
            ErrorMessage = "An unexpected error occurred.";
        }
    }
}
```

> **Rule of thumb**: Use cases log `Warning` for predictable domain violations. ViewModels log
> `Error` only for *unexpected* exceptions that the use case did not anticipate.

---

## Step 8 — Build and Verify

```bash
# Restore new package in ApplicationService
dotnet restore

# Build all projects
dotnet build

# Run the app
dotnet run --project src/AlconMusicPlayer.WPF

# Verify log file was created
Get-Content "src\AlconMusicPlayer.WPF\bin\Debug\net8.0-windows\logs\alcon-music-player.log"
```

**Expected console output** (example):
```
2026-04-09 08:00:01 [INF] App: Application starting up — version 1.0.0.0
2026-04-09 08:00:05 [INF] CreatePlaylistUseCase: Playlist created: 'My Mix' (ID: 3f2a...)
2026-04-09 08:00:10 [WRN] CreatePlaylistUseCase: Duplicate playlist name rejected: 'My Mix'
```

**Expected file** (`logs/alcon-music-player.log`):
```
2026-04-09 08:00:01 [INF] App: Application starting up — version 1.0.0.0
...same lines as console...
```

---

## Extending to a New Sink (e.g., Windows Event Viewer)

1. Create `src/AlconMusicPlayer.WPF/Logging/EventViewerLoggerProvider.cs` implementing `ILoggerProvider`
2. Its `CreateLogger()` returns an `EventViewerLogger : ILogger` that calls `EventLog.WriteEntry()`
3. In `App.xaml.cs`, add one line inside `AddLogging()`:
   ```csharp
   builder.AddProvider(new EventViewerLoggerProvider("AlconMusicPlayer"));
   ```
4. **That's it.** No other files change.

The same pattern applies for ETW (TraceSource), a remote log aggregator (Seq, OpenTelemetry),
or a WPF in-app log viewer bound to an `ObservableCollection<string>`.

---

## File Summary

| File | Action |
|------|--------|
| `AlconMusicPlayer.ApplicationService.csproj` | Add `Microsoft.Extensions.Logging.Abstractions` NuGet |
| `WPF/Logging/ConsoleLoggerProvider.cs` | **New** — console sink |
| `WPF/Logging/FileLoggerProvider.cs` | **New** — file sink |
| `WPF/App.xaml.cs` | Register `AddLogging()` + log startup/shutdown |
| `ApplicationService/UseCases/CreatePlaylistUseCase.cs` | Inject `ILogger<T>`, log success + warning |
| `ApplicationService/UseCases/AddTrackToPlaylistUseCase.cs` | Inject `ILogger<T>`, log success |
| `ApplicationService/UseCases/RemoveTrackFromPlaylistUseCase.cs` | Inject `ILogger<T>`, log success |
| `WPF/ViewModels/PlaylistsViewModel.cs` | Inject `ILogger<T>`, log unexpected errors |
