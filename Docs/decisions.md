# Decisions Log — Alcon Music Player

> **Purpose**: Architecture Decision Records (ADRs). Documents **why** key decisions were made so future sessions don't re-debate them.

---

## ADR-001: Use MVVM Pattern

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: The requirements explicitly mandate MVVM with data binding, commands, and separation of concerns.
- **Decision**: Strict MVVM — no code-behind logic in Views. All interaction handled via commands and bindings.
- **Consequences**: Requires `ViewModelBase`, `RelayCommand`, and DataTemplate-based navigation. More upfront structure but cleaner long-term.

---

## ADR-002: Hardcoded Song Data (No File I/O)

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Requirements allow either file-based or hardcoded data. For simplicity and zero external dependencies, hardcoded is preferred for v1.
- **Decision**: Implement a dedicated `SeedData.cs` class in the Infrastructure layer (`Infra/Data/`) with 35 tracks across 7 albums and 6 artists (Tamil + English mix). `SeedData.Build()` returns a tuple `(Tracks, Artists, Albums)` consumed by the repositories.
- **Consequences**: No file parsing logic needed. Easy to swap later via the `IMusicLibraryService` interface if file-based loading is desired.

---

## ADR-003: ViewModel-First Navigation

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Need to switch between Songs, Artists, Albums, Playlists, and Playlist Detail views.
- **Decision**: `MainViewModel` holds a `CurrentView` property (type `ViewModelBase`). A `ContentControl` in `MainWindow.xaml` binds to it. DataTemplates map ViewModel types to Views.
- **Consequences**: Clean navigation without frame/page complexity. Easy to test — just assert which ViewModel is current.

---

## ADR-004: Service Interfaces for Testability

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Extra credit includes unit testing. Services behind interfaces can be mocked.
- **Decision**: All data access goes through `IMusicLibraryService` and `IPlaylistService` interfaces. ViewModels depend on interfaces, not concrete classes.
- **Consequences**: Enables unit testing of ViewModels with mock services. Slightly more files but worth the testability.

---

## ADR-005: Dark Theme (iTunes-like)

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: iTunes uses a dark UI with red/pink accent colors. The requirements say "match the user interface as much as possible."
- **Decision**: Use a dark color scheme with a red/accent highlight for selected items.
- **Consequences**: Need custom styles for all controls (DataGrid, ListBox, TextBox, etc.) since WPF defaults are light-themed.

---

## ADR-006: Target .NET 8 LTS

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Need to pick between .NET 8 (LTS) or .NET 9.
- **Decision**: .NET 8 LTS. TargetFramework = `net8.0-windows` for WPF, `net8.0` for class libraries.
- **Consequences**: LTS support until November 2026. Stable and widely available.

---

## ADR-007: No Audio Playback

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Requirements explicitly state "No need to playback the music."
- **Decision**: Playback controls (play, pause, skip, etc.) are visual-only. They update UI state (now-playing indicator, button icons) but don't actually play audio.
- **Consequences**: No dependency on NAudio, MediaElement, or any audio library. Simplifies the project significantly.

---

## ADR-008: Clean Architecture — 4-Project Solution

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: User requested separation into Domain, Application, Infrastructure, and WPF projects following SOLID principles. A single-project structure doesn't enforce proper dependency boundaries.
- **Decision**: Split into 4 projects:
  - **AlconMusicPlayer.Domain** — Entities, enums, core interfaces. No dependencies.
  - **AlconMusicPlayer.ApplicationLayer** — Use cases, service interfaces, DTOs. References Domain only.
  - **AlconMusicPlayer.Infrastructure** — Concrete service implementations, seed data. References Application + Domain.
  - **AlconMusicPlayer.WPF** — Views, ViewModels, resources, converters. References Application + Infrastructure + Domain.
- **Consequences**: Enforces dependency inversion at the project level. Domain is pure and portable. Infrastructure can be swapped without touching business logic. More projects to manage but much better separation of concerns.

---

## ADR-009: SOLID Principles as Design Constraints

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: User explicitly requested SOLID principles alongside MVVM.
- **Decision**: Apply all five SOLID principles:
  - **SRP**: One responsibility per class (use cases, ViewModels, services).
  - **OCP**: New views/data sources added via new classes, not modifications.
  - **LSP**: Service implementations are interchangeable via interfaces.
  - **ISP**: Separate small interfaces (`IMusicLibraryService`, `IPlaylistService`). No `INavigationService` — navigation is a UI concern handled by `MainViewModel.CurrentView` + DataTemplates (ADR-003).
  - **DIP**: ViewModels depend on abstractions, not concrete services. DI container handles wiring.
- **Consequences**: More interfaces and smaller classes. Highly testable. Clear contracts between layers.

---

## ADR-010: Use Case Pattern in Application Layer

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Need a way to organize business logic that respects SRP. Putting everything in services creates bloated classes.
- **Decision**: Each discrete business operation is a separate use case class with a single `Execute()` method (e.g., `CreatePlaylistUseCase`, `AddSongToPlaylistUseCase`). Use cases live in the Application layer and orchestrate calls to service interfaces.
- **Consequences**: Very granular and testable. Each use case can be tested in isolation. ViewModels call use cases (or service interfaces directly for simple queries).

---

## ADR-020: Switch to Microsoft.Extensions.DependencyInjection

- **Date**: April 2, 2026
- **Status**: Accepted — supersedes ADR-011
- **Context**: Manual DI was chosen when the app had 2 services and 1 ViewModel. The architecture has since grown to include repositories, services, use cases, and multiple ViewModels. Hand-wiring all dependencies in `App.xaml.cs` is now fragile and order-dependent, and will only get worse as more ViewModels are added.
- **Decision**: Use `Microsoft.Extensions.DependencyInjection` (the NuGet package `Microsoft.Extensions.DependencyInjection`) in the WPF project. Register all repositories, services, use cases, and ViewModels in `App.xaml.cs` using `IServiceCollection`. Resolve the root `MainViewModel` via `ServiceProvider.GetRequiredService<MainViewModel>()`.
- **Consequences**: `App.xaml.cs` becomes a clean registration list rather than a manual construction graph. Adding new classes requires only one new `services.AddTransient<>()` line. `Microsoft.Extensions.DependencyInjection` is a first-party Microsoft package (not a heavy third-party library) — aligned with the spirit of ADR-012, which targets unnecessary external dependencies.

---

## ADR-011: Manual DI — No 3rd-Party Container

- **Date**: March 31, 2026
- **Status**: ~~Accepted~~ **Superseded by ADR-020** (April 2, 2026)
- **Context**: Need to wire up services and ViewModels across 4 projects. User wants to minimize 3rd-party libraries.
- **Decision**: Manual constructor injection in `App.xaml.cs` (composition root). No DI container NuGet package.
- **Consequences**: Superseded once the dependency graph grew large enough that manual wiring became fragile. See ADR-020.

---

## ADR-015: Global Resources for UI Consistency

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Dark theme requires custom styling for every control. Inline styles would cause duplication and inconsistency.
- **Decision**: All colors, styles, and icons live in `Resources/` as ResourceDictionaries, merged into `App.xaml`. Views use `{StaticResource}` only — no inline colors, font sizes, or brushes.
- **Consequences**: Single source of truth for theming. Easy to tweak or swap themes later.

---

## ADR-012: Minimize 3rd-Party Libraries

- **Date**: March 31, 2026
- **Status**: Accepted (updated April 2, 2026)
- **Context**: User wants to keep external dependencies to a minimum.
- **Decision**: Allowed NuGet packages:
  - **`Microsoft.Extensions.DependencyInjection`** in the WPF project (ADR-020 — first-party Microsoft, replaces manual wiring)
  - **xUnit** + xUnit runners in the test project only
  - Everything else is hand-rolled: `ViewModelBase`, `RelayCommand`, test fakes/stubs
  - No CommunityToolkit.Mvvm, no Prism, no Moq, no other packages
- **Consequences**: Minimal external surface. Both allowed packages are either first-party Microsoft or the de-facto standard test framework.

---

## ADR-013: xUnit for Testing

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Unit testing is extra credit. Need to pick a test framework.
- **Decision**: Use **xUnit** with `[Fact]` and `[Theory]` attributes. Mock services manually (simple fakes/stubs) to avoid adding Moq as a dependency.
- **Consequences**: No Moq = slightly more boilerplate for test doubles, but consistent with the "minimize libraries" goal.

---

## ADR-014: Code Comment Style

- **Date**: March 31, 2026
- **Status**: Accepted
- **Context**: Need consistent commenting without noise.
- **Decision**: Comment **why**, not **what**. Use `// ADR-NNN:` prefix to trace code decisions to this document. XML doc comments only on public interface members. No section-marker comments.
- **Consequences**: Cleaner code. Decisions are discoverable via ADR references.

---

## ADR-019: `PlayCount` as a Hardcoded Seed Property on `Track`

- **Date**: April 2, 2026
- **Status**: Accepted
- **Context**: The requirements show a "Plays" column in the songs DataGrid. Strictly speaking, play count is a *user listening history* concern — it belongs to a playback/history service, not the track itself. However, since there is no audio playback and no persistence in v1, modelling a full history layer would be over-engineering.
- **Decision**: Add a `PlayCount` property (`int`) directly to the `Track` entity, seeded with realistic hardcoded values in `InMemoryTrackRepository`. It is display-only — incremented play count on click is out of scope for v1.
- **Consequences**: Slightly impure domain model (play count is really user state, not track state), but pragmatic for v1. Easy to move to a separate `ListeningHistory` service in v2 without breaking the rest of the architecture — the `Track` property would simply be removed or become computed.

---

## ADR-018: Only Implement Use Cases Where Business Logic Exists

- **Date**: April 2, 2026
- **Status**: Accepted
- **Context**: The Application layer was planned to have use cases for every operation (GetAllTracks, Search, GetArtists, GetAlbums, CreatePlaylist, AddTrack, RemoveTrack). However, ViewModels currently call service interfaces directly, meaning pure-query use cases would be trivial pass-throughs with no logic — adding ceremony without value.
- **Decision**: Only implement use cases where real business logic exists. ViewModels call service interfaces directly for simple queries. The rule: if a use case body is just `return _service.Foo()`, skip it.
  - **Implemented as use cases** (contain validation/business rules):
    - `CreatePlaylistUseCase` — validates name is not empty or duplicate
    - `AddTrackToPlaylistUseCase` — validates track is not already in the playlist
    - `RemoveTrackFromPlaylistUseCase` — validates playlist and track ID exist before delegating; takes `(Guid playlistId, Guid trackId)` — no need to pass the full `Track` object for a remove operation
  - **Dropped** (ViewModels call `IMusicLibraryService` / `IPlaylistService` directly):
    - `GetAllTracksUseCase`, `SearchTracksUseCase`, `GetArtistsUseCase`, `GetAlbumsUseCase` — pure queries with no logic to add
- **Consequences**: Use cases justify their existence. The three playlist-mutation use cases are individually testable with business rule assertions. Avoids a layer of pass-through indirection for reads.

---

## ADR-017: Introduce Repository Pattern for Clean Architecture Demonstration

- **Date**: April 2, 2026
- **Status**: Accepted
- **Context**: The initial implementation embedded data ownership inside the service classes (`MusicLibraryService` held `_tracks`, `PlaylistService` held `_playlists`). This is an SRP violation: services should *orchestrate business logic*, repositories should *own and persist data*. Even though the data is in-memory for v1, the structural separation should be correct to properly demonstrate Clean Architecture.
- **Decision**: Introduce a Repository layer between services and raw data:
  - **Domain layer**: `ITrackRepository` and `IPlaylistRepository` interfaces — the innermost contracts, owned by the domain.
  - **Infrastructure layer**: `InMemoryTrackRepository` and `InMemoryPlaylistRepository` — concrete implementations that own the in-memory collections and seed data.
  - **Application/Infrastructure services**: `MusicLibraryService` and `PlaylistService` delegate data access to their respective repositories via the domain interfaces.
- **Consequences**: Correct layer responsibilities. Repositories are swappable independently of service logic (e.g., swap `InMemoryTrackRepository` for `SqliteTrackRepository` without touching `MusicLibraryService`). Slightly more files, but each has a single, clear responsibility.

---

## ADR-016: `Track` as the Core Domain Entity (not `Song`)

- **Date**: April 1, 2026
- **Status**: Accepted
- **Context**: The initial planning documents referred to the primary domain entity as `Song`. During implementation, the more precise term `Track` was adopted — a music library stores tracks (individual recordings on an album), while "song" is a more informal term.
- **Decision**: The core entity is named `Track`. All service interfaces, ViewModel properties, and collections use `Track` terminology (`GetAllTracks`, `AddTrack`, `IReadOnlyList<Track>`, etc.).
- **Consequences**: More accurate domain language. All documentation updated to reflect `Track`. The `IMusicLibraryService` and `IPlaylistService` interfaces use `Track`-based method names. No functional impact — purely a naming decision.

---

<!-- 
## ADR Template

## ADR-NNN: [Title]

- **Date**: [Date]
- **Status**: Proposed | Accepted | Deprecated | Superseded
- **Context**: [What is the problem or situation?]
- **Decision**: [What was decided?]
- **Consequences**: [What are the trade-offs?]
-->
