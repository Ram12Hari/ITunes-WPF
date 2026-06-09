# Software Detailed Design (SDD)

> **Document ID**: IEC-DOC-004  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clause**: 5.4  
> **Status**: Draft  
> **Approved by**: _Pending_

---

## 1. Purpose

This document describes the detailed design of each software unit in the Alcon Music Player.
It bridges the architectural software items defined in [SAD.md](SAD.md) with the actual
implementation, and provides the rationale for key design decisions.

---

## 2. Domain Layer — AlconMusicPlayer.Domain

### 2.1 `Track` Entity

**File**: `src/AlconMusicPlayer.Domain/Entities/Track.cs`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique identifier, set on construction |
| `Title` | `string` | Display name of the track |
| `Artist` | `Artist` | Reference to the artist entity |
| `Album` | `Album` | Reference to the album entity |
| `Duration` | `int` | Duration in seconds |
| `FilePath` | `string` | Logical path (not validated — no I/O) |
| `Genre` | `Genre` | Enum value |
| `PlayCount` | `int` | Number of times selected |

**Design decisions:**
- Immutable after construction — no public setters on identity/data fields
- `PlayCount` may be incremented via a dedicated method to maintain encapsulation

### 2.2 `Playlist` Entity

**File**: `src/AlconMusicPlayer.Domain/Entities/Playlist.cs`

| Method | Behaviour |
|--------|-----------|
| `Playlist.Create(name)` | Static factory — trims name, assigns new `Guid` |
| `AddTrack(track)` | Appends track; ignores if already present (idempotent) |
| `RemoveTrack(trackId)` | Removes by ID; no-op if not found |

**Design decisions:**
- Internal track list is `List<Track>` exposed as `IReadOnlyList<Track>` — prevents external mutation
- Duplicate guard in `AddTrack` is intentional — use case layer validates existence separately

### 2.3 Repository Interfaces

| Interface | Key Methods |
|-----------|------------|
| `ITrackRepository` | `GetAllTracks()`, `GetTrackById(Guid)` |
| `IPlaylistRepository` | `GetAllPlaylists()`, `GetPlaylistByID(Guid)`, `AddPlaylist()`, `UpdatePlaylist()` |

---

## 3. Application Layer — AlconMusicPlayer.ApplicationService

### 3.1 Use Cases

Each use case follows the pattern: validate inputs → query repository → apply domain logic → persist.

#### `CreatePlaylistUseCase`
1. Validate name is not null or whitespace → throw `ArgumentException`
2. Query all playlists → check for case-insensitive duplicate → throw `InvalidOperationException` + log `Warning`
3. Call `Playlist.Create(name)` → `IPlaylistRepository.AddPlaylist()` → log `Information`

#### `AddTrackToPlaylistUseCase`
1. Resolve playlist by ID → throw `InvalidOperationException` if not found
2. Resolve track by ID → throw `InvalidOperationException` if not found
3. Call `playlist.AddTrack(track)` → `IPlaylistRepository.UpdatePlaylist()` → log `Information`

#### `RemoveTrackFromPlaylistUseCase`
1. Resolve playlist by ID → throw `InvalidOperationException` if not found
2. Call `playlist.RemoveTrack(trackId)` → `IPlaylistRepository.UpdatePlaylist()` → log `Information`

### 3.2 Service Interfaces

| Interface | Responsibility |
|-----------|---------------|
| `IMusicLibraryService` | Provides read-only access to tracks, artists, albums |
| `IPlaylistService` | Provides read access to all playlists; wraps repository queries |
| `IThemeService` | Applies and retrieves the current application theme |

---

## 4. Infrastructure Layer — AlconMusicPlayer.Infra

### 4.1 `SeedData`

**File**: `src/AlconMusicPlayer.Infra/Data/SeedData.cs`

- `SeedData.Build()` returns `(IReadOnlyList<Track> tracks, IReadOnlyList<Artist> artists, IReadOnlyList<Album> albums)`
- 35 tracks across 7 albums, 6 artists (A.R. Rahman, Ilaiyaraaja, Yuvan Shankar Raja, Pink Floyd, Coldplay, Radiohead)
- Constructed once at startup and injected into repositories

### 4.2 In-Memory Repositories

| Class | Storage | Thread Safety |
|-------|---------|--------------|
| `InMemoryTrackRepository` | `List<Track>` (read-only) | Read-safe; no mutations |
| `InMemoryPlaylistRepository` | `List<Playlist>` | Not thread-safe — single-threaded WPF UI use only |

### 4.3 Services

| Class | Implements | Key Behaviour |
|-------|-----------|--------------|
| `MusicLibraryService` | `IMusicLibraryService` | Delegates to `ITrackRepository`; returns filtered/sorted results |
| `PlaylistService` | `IPlaylistService` | Delegates to `IPlaylistRepository`; returns all playlists |

---

## 5. WPF Presentation Layer — AlconMusicPlayer.WPF

### 5.1 ViewModelBase

**File**: `src/AlconMusicPlayer.WPF/ViewModels/Base/ViewModelBase.cs`

- Implements `INotifyPropertyChanged`
- Provides `SetProperty<T>(ref T field, T value)` helper — raises `PropertyChanged` only when value differs

### 5.2 RelayCommand

**File**: `src/AlconMusicPlayer.WPF/ViewModels/Base/RelayCommand.cs`

- Implements `ICommand`
- Constructor accepts `Action execute` and optional `Func<bool> canExecute`
- `RaiseCanExecuteChanged()` invalidates the command for UI re-evaluation

### 5.3 ViewModel Design Decisions

| ViewModel | Key Responsibilities | Injects |
|-----------|---------------------|---------|
| `MainViewModel` | Navigation (`CurrentView`), search delegation | All child ViewModels |
| `LibraryViewModel` | Track list, search filter, context menu | `IMusicLibraryService`, `IPlaylistService` |
| `ArtistsViewModel` | Artist list, album drill-down, filtered tracks | `IMusicLibraryService` |
| `AlbumsViewModel` | Album list, filtered tracks | `IMusicLibraryService` |
| `PlaylistsViewModel` | Playlist list, create playlist | `IPlaylistService`, `CreatePlaylistUseCase` |
| `PlaylistViewModel` | Playlist detail, remove track | `IPlaylistService`, `RemoveTrackFromPlaylistUseCase` |
| `NowPlayingViewModel` | Selected track display | - |
| `SettingsViewModel` | Theme switching | `IThemeService` |

### 5.4 Logging Design

See [logging-guide.md](../logging-guide.md) for the full implementation guide.

| Class | Implements | Output |
|-------|-----------|--------|
| `ConsoleLoggerProvider` | `ILoggerProvider` | `stdout` with colour-coded levels |
| `FileLoggerProvider` | `ILoggerProvider` | `logs/alcon-music-player.log` (append, per-write) |

**Log levels used:**
- `Information` — successful business operations (playlist created, track added)
- `Warning` — predictable domain violations (duplicate name, not found)
- `Error` — unexpected exceptions caught in ViewModels
- `Information` — application startup and shutdown lifecycle

---

## 6. Error Handling Strategy

| Layer | Handles | Action |
|-------|---------|--------|
| Domain entities | Invalid state | Throw `ArgumentException` / `InvalidOperationException` |
| Use cases | Validation / not-found | Throw + log `Warning` |
| ViewModels | Business exceptions | Catch, set `ErrorMessage`, display to user |
| ViewModels | Unexpected exceptions | Catch, log `Error`, display generic message |
| Global | Unhandled exceptions | Register `Application.DispatcherUnhandledException` in `App.xaml.cs` |

---

## 7. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial detailed design |
