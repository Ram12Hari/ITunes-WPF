# Software Architecture Document (SAD)

> **Document ID**: IEC-DOC-003  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clause**: 5.3  
> **Status**: Draft  
> **Approved by**: _Pending_

---

## 1. Purpose

This document describes the software architecture of the **Alcon Music Player**, identifying all
software items, their responsibilities, and the interfaces between them. It provides the traceability
bridge between requirements ([SRS.md](SRS.md)) and detailed design ([SDD.md](SDD.md)).

---

## 2. Architectural Overview

The system is organised as a **Clean Architecture** solution with four software items (projects).
Dependencies flow strictly inward — outer layers depend on inner layers, never the reverse.

```
┌─────────────────────────────────────────────────────┐
│                 AlconMusicPlayer.WPF                 │  ← Presentation Layer
│        Views · ViewModels · DI Root · Resources      │
└───────────────────────┬─────────────────────────────┘
                        │ references
          ┌─────────────┴──────────────┐
          │                            │
┌─────────▼──────────┐   ┌────────────▼──────────────┐
│  AlconMusicPlayer  │   │   AlconMusicPlayer.Infra   │  ← Infrastructure Layer
│  .ApplicationService│  │  Repositories · Services  │
│  Use Cases · Interfaces│ SeedData                  │
└─────────┬──────────┘   └────────────┬──────────────┘
          │ references                 │ references
          └──────────────┬────────────┘
                         │
          ┌──────────────▼─────────────────┐
          │    AlconMusicPlayer.Domain      │  ← Domain Layer (no dependencies)
          │  Entities · Interfaces · Enums  │
          └─────────────────────────────────┘
```

---

## 3. Software Items

### 3.1 AlconMusicPlayer.Domain

| Attribute | Value |
|-----------|-------|
| Type | Class Library |
| Target Framework | `net8.0` |
| Dependencies | None |
| Responsibility | Core business entities and repository contracts |

**Key components:**
- `Entities/Track.cs` — immutable track data (Title, Artist, Album, Duration, Genre, PlayCount)
- `Entities/Playlist.cs` — playlist aggregate (add/remove track, name validation)
- `Entities/Artist.cs`, `Album.cs`, `Genre.cs` — supporting entities
- `Interfaces/ITrackRepository.cs` — contract for track data access
- `Interfaces/IPlaylistRepository.cs` — contract for playlist persistence

### 3.2 AlconMusicPlayer.ApplicationService

| Attribute | Value |
|-----------|-------|
| Type | Class Library |
| Target Framework | `net8.0` |
| Dependencies | Domain, `Microsoft.Extensions.Logging.Abstractions` |
| Responsibility | Application use cases and service interfaces |

**Key components:**
- `UseCases/CreatePlaylistUseCase.cs` — validates and creates a playlist
- `UseCases/AddTrackToPlaylistUseCase.cs` — validates and adds a track to a playlist
- `UseCases/RemoveTrackFromPlaylistUseCase.cs` — removes a track from a playlist
- `Interfaces/IMusicLibraryService.cs` — contract for reading tracks, artists, albums
- `Interfaces/IPlaylistService.cs` — contract for managing playlists
- `Interfaces/IThemeService.cs` — contract for theme switching

### 3.3 AlconMusicPlayer.Infra

| Attribute | Value |
|-----------|-------|
| Type | Class Library |
| Target Framework | `net8.0` |
| Dependencies | Domain, ApplicationService |
| Responsibility | Concrete data access implementations and seed data |

**Key components:**
- `Data/SeedData.cs` — returns `(Tracks, Artists, Albums)` tuple with 35 pre-loaded tracks
- `Repositories/InMemoryTrackRepository.cs` — implements `ITrackRepository`
- `Repositories/InMemoryPlaylistRepository.cs` — implements `IPlaylistRepository`
- `Services/MusicLibraryService.cs` — implements `IMusicLibraryService`
- `Services/PlaylistService.cs` — implements `IPlaylistService`

### 3.4 AlconMusicPlayer.WPF

| Attribute | Value |
|-----------|-------|
| Type | WPF Application (`WinExe`) |
| Target Framework | `net8.0-windows` |
| Dependencies | Domain, ApplicationService, Infra, `Microsoft.Extensions.DependencyInjection` |
| Responsibility | All UI, ViewModels, DI composition root, resources/styles |

**Key components:**
- `App.xaml.cs` — DI composition root; registers all services, ViewModels, logging
- `MainWindow.xaml` — shell window with sidebar + content area + Now Playing bar
- `ViewModels/MainViewModel.cs` — navigation controller; holds `CurrentView`
- `ViewModels/LibraryViewModel.cs` — song list + search + context menu
- `ViewModels/ArtistsViewModel.cs` — artist browsing + filtering
- `ViewModels/AlbumsViewModel.cs` — album browsing + filtering
- `ViewModels/PlaylistsViewModel.cs` — playlist list + create playlist
- `ViewModels/PlaylistViewModel.cs` — playlist detail + remove track
- `ViewModels/NowPlayingViewModel.cs` — currently selected track display
- `ViewModels/SettingsViewModel.cs` — application settings (theme)
- `Services/ThemeService.cs` — implements `IThemeService`
- `Logging/ConsoleLoggerProvider.cs` — console sink (see `logging-guide.md`)
- `Logging/FileLoggerProvider.cs` — file sink (see `logging-guide.md`)
- `Resources/` — `Colors.xaml`, `Styles.xaml`, `DataGridStyles.xaml`, `Icons.xaml`

---

## 4. Interfaces Between Software Items

| Interface | Defined In | Implemented In | Used By |
|-----------|-----------|---------------|---------|
| `ITrackRepository` | Domain | Infra | ApplicationService, WPF |
| `IPlaylistRepository` | Domain | Infra | ApplicationService, WPF |
| `IMusicLibraryService` | ApplicationService | Infra | WPF ViewModels |
| `IPlaylistService` | ApplicationService | Infra | WPF ViewModels |
| `IThemeService` | ApplicationService | WPF | WPF ViewModels |
| `ILogger<T>` | `Microsoft.Extensions.Logging.Abstractions` | WPF (providers) | ApplicationService, WPF |

---

## 5. MVVM Layer Responsibilities

| Layer | Knows About | Must Not Know About |
|-------|------------|-------------------|
| View (.xaml) | ViewModel (DataContext) | Domain entities, services |
| ViewModel (.cs) | Application interfaces, Domain entities | Views, Infrastructure |
| Model (Domain entities) | Nothing | UI, WPF, logging |

---

## 6. Navigation Architecture

Navigation is ViewModel-first (ADR-003):
- `MainViewModel.CurrentView` is bound to a `ContentControl` in `MainWindow.xaml`
- `DataTemplate` entries in `App.xaml` map each ViewModel type to its corresponding View
- No `Frame`, `Page`, or navigation service is used

---

## 7. Dependency Injection Architecture

All dependencies are wired in `App.xaml.cs` using `Microsoft.Extensions.DependencyInjection`:
- Repositories: `Singleton` (in-memory — data must persist for app lifetime)
- Services: `Singleton`
- Use Cases: `Transient` (stateless — new instance per resolution)
- ViewModels: `Transient` (except `MainViewModel` and `NowPlayingViewModel` — `Singleton`)
- Logging providers: registered via `services.AddLogging()`

---

## 8. Requirement Traceability

| SRS Requirement | Satisfied By |
|----------------|-------------|
| SRS-F-001 to F-006 | `LibraryViewModel`, `LibraryView` |
| SRS-F-010 to F-012 | `ArtistsViewModel`, `ArtistsView` |
| SRS-F-015 to F-016 | `AlbumsViewModel`, `AlbumsView` |
| SRS-F-020 to F-028 | `PlaylistsViewModel`, `PlaylistViewModel`, use cases |
| SRS-F-030 to F-032 | `MainViewModel`, `MainWindow` |
| SRS-F-035 to F-037 | `LibraryViewModel` (context menu commands) |
| SRS-F-040 to F-041 | `ThemeService`, `SettingsViewModel`, `Resources/` |
| SRS-NF-003 | `FileLoggerProvider`, `ConsoleLoggerProvider` |
| SRS-NF-004 | MVVM architecture across all ViewModels |
| SRS-NF-005 | `App.xaml.cs` DI composition root |

---

## 9. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial architecture document |
