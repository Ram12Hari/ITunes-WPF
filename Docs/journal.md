# Development Journal — Alcon Music Player

> **Purpose**: Chronological log of all development sessions. Each entry captures what was discussed, decided, and built. Newest entries at the top.

---

## Session 1 — March 31, 2026

### Context
- First session. User provided the project requirements in `Requirement/MusicPlayerRequirements.md`.
- Requirements describe an iTunes-like WPF music player using MVVM.

### What Was Done
- Reviewed the full requirements document including 3 ASCII UI layouts:
  1. **Main Song List View** — sidebar navigation + DataGrid of songs
  2. **Context Menu** — right-click to add songs to playlists
  3. **Playlist Detail View** — playlist header with icon, name, description, song list
- Created project documentation files for cross-session context awareness:
  - `instruction.md` — project overview, conventions, rules
  - `journal.md` — this file
  - `architecture.md` — MVVM structure and component design
  - `decisions.md` — architecture decision records
  - `status.md` — progress tracker

### Decisions Made
- Song data will be hardcoded (no file I/O for v1)
- MVVM pattern is mandatory — no code-behind logic
- No audio playback required
- Documentation-first approach: establish context files before coding
- **4-project Clean Architecture** (ADR-008): Domain, Application, Infrastructure, WPF
- **SOLID principles** enforced across all layers (ADR-009)
- **Use Case pattern** in Application layer — one class per operation (ADR-010)
- **Manual DI** — no 3rd-party container, wired in App.xaml.cs (ADR-011, ADR-012)
- **xUnit** confirmed as test framework, with hand-rolled fakes (no Moq) (ADR-013)
- **Comment style**: short, decision-focused, `// ADR-NNN:` prefix for traceability (ADR-014)
- Dependency rule: Domain → nothing, Application → Domain, Infrastructure → Application+Domain, WPF → all
- **Minimize 3rd-party libs**: only xUnit in test project, zero NuGet in main projects

### Key Observations from Requirements
- UI has 3 main areas: top control bar, left sidebar, main content area
- Sidebar has two sections: Library (Recently Added, Artists, Albums, Songs) and Playlists
- DataGrid columns: Title, Time, Artist, Album, Genre, Plays
- Context menu supports adding to playlists with a submenu
- Playlist detail view has editable name and description
- Visual states include: selected (red/accent highlight), hover, playing indicator (▶)

### Open Questions
_All resolved in this session:_
- **.NET 8 LTS** confirmed (ADR-006)
- **Dark theme** confirmed (ADR-005)
- **15-20 sample songs** confirmed
- **xUnit** for testing with hand-rolled fakes (ADR-013)
- **Global resources** for all UI styling (ADR-015)

### Next Steps
- Confirm tech stack decisions with user
- Scaffold the WPF project structure
- Begin implementing Models, then ViewModels, then Views

## Session 4 — April 5, 2026

### Context
- Picking up from Session 3. All architecture decisions were documented; repository pattern and service refactors were planned but not yet coded.
- Goal: verify actual code state, reconcile docs against code, implement `SeedData.cs`.

### What Was Done
- **Full codebase audit** — read all source files across all 4 projects to verify actual implementation state vs. documentation.
- **Confirmed implemented** (previously marked pending in docs):
  - `Genre.cs` enum (Rock, Classical, Pop, Ambient, Undefined) — in `Domain/Entities/`
  - `Genre` and `PlayCount` properties on `Track.cs` — `PlayCount` is private set via `IncrementPlayCount()`
  - `ITrackRepository.cs` — `GetAllTracks`, `GetTrackById`, `GetTracksByAlbum`, `GetTracksByArtist`, `SearchTrack`
  - `IPlaylistRepository.cs` — `GetAllPlaylists`, `GetPlaylistByID`, `AddPlaylist`, `RemovePlaylist`, `UpdatePlaylist`
  - `InMemoryTrackRepository.cs` and `InMemoryPlaylistRepository.cs` — both implemented, repositories own their collections
  - `MusicLibraryService.cs` — already refactored to delegate to `ITrackRepository`
  - `PlaylistService.cs` — already refactored to delegate to `IPlaylistRepository` and `ITrackRepository`
- **Confirmed pending**:
  - `InMemoryTrackRepository._tracks` is still an empty list `[]` — `SeedData.Build()` not yet wired into constructor
  - `App.xaml.cs` is empty — DI not configured; app won't run yet
  - ArtistsViewModel, AlbumsViewModel, PlaylistsViewModel, PlaylistViewModel — not yet implemented
  - Use cases (`CreatePlaylistUseCase` etc.) — not yet implemented
- **Decisions clarified**:
  - `INavigationService` — decided this app does not need it. `MainViewModel.CurrentView` + XAML `DataTemplate` routing is sufficient. Navigation is a UI concern; it doesn't belong in the Application layer.
  - `Album` entity — has `Name` property (not `Title`), no `Artist` or `Year`. Artist lives on `Track`.
- **`SeedData.cs` implemented** in `AlconMusicPlayer.Infra/Data/`:
  - 35 tracks, 7 albums, 6 artists — Tamil (A.R. Rahman: Roja, Bombay; Ilaiyaraaja: Mouna Ragam; Yuvan: 7G Rainbow Colony) + English (Pink Floyd: DSOTM; Coldplay: A Rush of Blood to the Head; Radiohead: OK Computer)
  - Uses factory methods (`Artist.Create`, `Album.Create`, `Track.Create`) — respects private constructors on all entities
  - Internal structure uses `Dictionary<string, Artist>` and `Dictionary<string, Album>` for named lookup during track construction; returns `IReadOnlyList<>` tuples to callers
  - `SeedData.Build()` signature: `(IReadOnlyList<Track>, IReadOnlyList<Artist>, IReadOnlyList<Album>)`
- **All `.md` files updated** (`status.md`, `architecture.md`, `decisions.md`, `instruction.md`) to reflect actual code state.

### Decisions Made
- **`INavigationService` dropped** — not needed for this app. Recorded in ADR-009 (ISP bullet updated) and removed from architecture.md / status.md.
- **`SeedData.cs` placement confirmed** — Infrastructure layer (`Infra/Data/`). Data is a data concern, not a business rule. Separate from repositories to keep `InMemoryTrackRepository` focused on query operations only.

### Issues Encountered
- Previous seed data drafts used object initializers (`new Track { ... }`) which bypassed private constructors — corrected to factory method calls.
- `Album` was assumed to have `Title` and `Artist` properties — actual code has only `Name`. Docs and seed data corrected.
- `Track.Duration` is `int` (seconds), not `TimeSpan` — `DurationDisplay` (TimeSpan) is the computed property. Seed data corrected.
- `InMemoryTrackRepository` had no constructor to receive tracks — fixed: constructor `(List<Track> tracks)` added.
- `IPlaylistService` was missing `RenamePlaylist` — `Playlist` entity had the method but the service interface and implementation did not expose it. Fixed: `RenamePlaylist(Guid playlistID, string newName)` added to `IPlaylistService` and implemented in `PlaylistService`.

### Next Steps
- Wire `SeedData.Build()` into `InMemoryTrackRepository` constructor (inject tracks at construction time)
- Implement `App.xaml.cs` DI using `Microsoft.Extensions.DependencyInjection` — register all repositories, services, and ViewModels (ADR-020)
- Add `Microsoft.Extensions.DependencyInjection` NuGet package to `AlconMusicPlayer.WPF.csproj`
- Implement `ArtistsViewModel` + `ArtistsView.xaml`
- Implement `AlbumsViewModel` + `AlbumsView.xaml`
- Implement `PlaylistsViewModel` + `PlaylistsView.xaml`
- Implement `PlaylistViewModel` + `PlaylistView.xaml`
- Wire sidebar navigation in `MainViewModel`
- Add Genre and Plays columns to `LibraryView.xaml` DataGrid
- Implement use cases: `CreatePlaylistUseCase`, `AddTrackToPlaylistUseCase`, `RemoveTrackFromPlaylistUseCase`

---

## Session 3 — April 2, 2026

### Context
- Architectural review discussion: the current `MusicLibraryService` and `PlaylistService` own their data collections directly, violating SRP. Services should orchestrate logic; repositories should own data.
- Goal: introduce the Repository pattern to properly demonstrate Clean Architecture — even with in-memory data.

### What Was Done
- **Documentation only** — code implementation deferred to next session.
- Updated all `.md` files to reflect the new architecture.

### Decisions Made
- **ADR-017**: Introduce Repository pattern. `ITrackRepository` and `IPlaylistRepository` interfaces live in the Domain layer. `InMemoryTrackRepository` and `InMemoryPlaylistRepository` will live in Infrastructure. Services will be refactored to delegate raw data access to their respective repositories.
- **ADR-020**: Switch to `Microsoft.Extensions.DependencyInjection`. Manual DI (ADR-011) is no longer manageable — the dependency graph now includes repositories, services, use cases, and multiple ViewModels. First-party Microsoft package, consistent with the spirit of ADR-012.
- **ADR-018**: Use cases only where real business logic exists. Pure queries (`GetAllTracks`, `Search`, `GetArtists`, `GetAlbums`) go directly from ViewModel → service interface. Only three playlist-mutation use cases implemented: `CreatePlaylistUseCase`, `AddTrackToPlaylistUseCase`, `RemoveTrackFromPlaylistUseCase` — each adds validation the service layer alone does not enforce.
- **ADR-013 reaffirmed**: Keep hand-rolled fakes for testing. With only two service interfaces and three use cases under test, Moq adds a dependency without proportional benefit at this scale.

### Next Steps
- Create `Genre.cs` enum in `AlconMusicPlayer.Domain/Enums/` (Rock, Pop, Electronic, Jazz, Classical, HipHop, Ambient, Downtempo)
- Add `Genre` and `PlayCount` properties to `Track.cs`
- Update `Playlist.RemoveTrack(Track)` → `Playlist.RemoveTrack(Guid trackId)` — remove by ID, no need to pass full object
- Create `ITrackRepository.cs` and `IPlaylistRepository.cs` in `AlconMusicPlayer.Domain/Interfaces/`
- Create `InMemoryTrackRepository.cs` in `AlconMusicPlayer.Infrastructure/Repositories/` (move seed data here, seed Genre + PlayCount values)
- Create `InMemoryPlaylistRepository.cs` in `AlconMusicPlayer.Infrastructure/Repositories/`
- Refactor `MusicLibraryService` to take `ITrackRepository` via constructor injection
- Refactor `PlaylistService` to take `IPlaylistRepository` via constructor injection
- Implement `CreatePlaylistUseCase`, `AddTrackToPlaylistUseCase`, `RemoveTrackFromPlaylistUseCase` in Application layer
- Add `Microsoft.Extensions.DependencyInjection` NuGet package to `AlconMusicPlayer.WPF.csproj`
- Refactor `App.xaml.cs` to use `IServiceCollection` — register all repositories, services, use cases, and ViewModels
- Add Genre and Plays columns to `LibraryView.xaml` DataGrid
- Fix the outstanding `dotnet build` error

---



### Context
- Picking up from Session 1. Documentation was complete; no code had been written yet.
- Goal: scaffold all 4 projects, implement domain + application + infrastructure layers, and create initial WPF skeleton.

### What Was Done
- **Solution scaffolding**: Created `AlconMusicPlayer.sln`, all 4 `.csproj` files, and set up project references (Domain ← Application ← Infrastructure ← WPF).
- **Domain layer** (`AlconMusicPlayer.Domain`):
  - `Track.cs` — core entity with Id, Title, Artist, Album, TrackNumber, Duration, FilePath, and computed `DurationDisplay`.
  - `Artist.cs` — Id, Name.
  - `Album.cs` — Id, Title, Artist, Year (nullable int).
  - `Playlist.cs` — Id, Name, Tracks (IReadOnlyList), with AddTrack / RemoveTrack / Rename / MoveTrack methods.
  - _(Genre enum deferred — not yet needed by current DataGrid)_
- **Application layer** (`AlconMusicPlayer.ApplicationLayer`):
  - `IMusicLibraryService.cs` — GetAllTracks, GetAllAlbums, GetAllArtists, GetTracksByAlbum, GetTracksByArtist, Search.
  - `IPlaylistService.cs` — GetAll, Create, Delete, AddTrack, RemoveTrack.
  - _(INavigationService, use cases, and DTOs deferred)_
- **Infrastructure layer** (`AlconMusicPlayer.Infrastructure`):
  - `MusicLibraryService.cs` — implements IMusicLibraryService; seed data (20 tracks: Radiohead OK Computer + Kid A, Portishead Dummy, Massive Attack Mezzanine) embedded in private `BuildSeedData()`.
  - `PlaylistService.cs` — implements IPlaylistService; in-memory CRUD.
- **WPF layer** (`AlconMusicPlayer.WPF`):
  - `ViewModelBase.cs` — INotifyPropertyChanged with SetProperty helper.
  - `RelayCommand.cs` — ICommand with parameterless and parameterized constructors; hooks into `CommandManager.RequerySuggested`.
  - `MainViewModel.cs` — holds `CurrentView` property; initialises to `LibraryViewModel`.
  - `LibraryViewModel.cs` — loads all tracks via `IMusicLibraryService`, supports live search filtering, exposes `SearchCommand` and `ClearSearchCommand`.
  - `MainWindow.xaml` — shell grid (60px top bar, 200px sidebar, expandable content area). All areas are styled placeholders; `ContentControl` binds to `MainViewModel.CurrentView`.
  - `LibraryView.xaml` — DataGrid with Title, Duration, Artist, Album, TrackNumber columns. Search TextBox bound to `SearchText` with `UpdateSourceTrigger=PropertyChanged`.
  - `App.xaml` — merges Colors.xaml → Icons.xaml → Styles.xaml → DataGridStyles.xaml. Registers `LibraryViewModel → LibraryView` DataTemplate.
  - `App.xaml.cs` — manual DI composition root: creates MusicLibraryService + PlaylistService, constructs MainViewModel, shows MainWindow.
  - `Colors.xaml`, `Styles.xaml`, `DataGridStyles.xaml`, `Icons.xaml` — dark theme resource dictionaries.

### Decisions Made
- **ADR-016**: Entity named `Track` (not `Song`) — more precise domain language; a music library contains tracks on albums, not abstract "songs".
- Seed data embedded directly in `MusicLibraryService.BuildSeedData()` rather than a separate `SeedData.cs` class — keeps it local to the service, still trivially replaceable via the interface.
- `LibraryViewModel` named for what it shows (the library) rather than the view it maps to — allows the same ViewModel to be reused later for filtered/browsed subsets.

### Issues Encountered
- Build is currently failing (`dotnet build` exits with code 1). Root cause not yet diagnosed — likely a XAML resource reference or project reference issue. Investigate before Phase 6 continues.

### Next Steps
- Diagnose and fix the build error
- Implement `ArtistsViewModel` + `ArtistsView.xaml`
- Implement `AlbumsViewModel` + `AlbumsView.xaml`
- Implement `PlaylistsViewModel` + `PlaylistsView.xaml`
- Implement `PlaylistViewModel` + `PlaylistView.xaml`
- Wire up sidebar navigation (INavigationService + NavigationService)
- Add `Genre` enum and `PlayCount` to `Track`
- Build out the top control bar (playback controls visual, now-playing label)

---



## Session N — [Date]

### Context
- [What was the starting point?]

### What Was Done
- [Bullet list of work completed]

### Decisions Made
- [Key decisions and rationale]

### Issues Encountered
- [Problems hit and how they were resolved]

### Next Steps
- [What to do in the next session]
-->
