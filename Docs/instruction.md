# Project Instructions — Alcon Music Player

> **Purpose**: This file provides context for any AI assistant or developer picking up this project in a new session. Read this file first.

## Project Summary

An **iTunes-like WPF Music Player** built in C# using the **MVVM pattern**. The app allows users to browse songs, create/manage playlists, and browse by artist and album. **No actual audio playback is required.**

## Key Documents

| File | Purpose |
|------|---------|
| `instruction.md` | **Start here.** Project overview, conventions, and rules. |
| `journal.md` | Chronological log of all sessions, decisions, and work done. |
| `architecture.md` | MVVM structure, project layout, and component responsibilities. |
| `decisions.md` | Architecture Decision Records (ADRs) — why we chose what we chose. |
| `status.md` | Current progress tracker — what's done, what's next, known issues. |
| `wpf-guide.md` | Step-by-step WPF wiring guide — concepts, build order, full code for all Views/ViewModels. |
| `wpf-feature-gaps.md` | Implementation guide for 3 UX features: context menu add-to-playlist, new playlist dialog, artist/album sidebar filtering. |
| `Requirement/MusicPlayerRequirements.md` | Original requirements with UI layout references. |

## Tech Stack

- **Framework**: WPF (.NET 8 LTS)
- **Language**: C#
- **Architecture**: Clean Architecture (4-project solution)
- **Pattern**: MVVM (Model-View-ViewModel) in the WPF layer
- **Principles**: SOLID (SRP, OCP, LSP, ISP, DIP)
- **DI**: `Microsoft.Extensions.DependencyInjection` (first-party Microsoft package — ADR-020)
- **Testing**: xUnit with hand-rolled fakes (no Moq)
- **NuGet packages**: `Microsoft.Extensions.DependencyInjection` (WPF project) + xUnit / xUnit runners (test project only). No other packages.
- **IDE**: Visual Studio / VS Code
- **Build**: `dotnet` CLI or Visual Studio

## Solution Structure (4 Projects)

| Project | Layer | Responsibility | References |
|---------|-------|---------------|------------|
| `AlconMusicPlayer.Domain` | Domain | Entities, enums, core interfaces | Nothing |
| `AlconMusicPlayer.ApplicationLayer` | Application | Use cases, service interfaces, DTOs | Domain |
| `AlconMusicPlayer.Infrastructure` | Infrastructure | Service implementations, seed data | Application, Domain |
| `AlconMusicPlayer.WPF` | Presentation | Views, ViewModels, resources, DI setup | Application, Infrastructure, Domain |

## Conventions

### Code Conventions
- Follow standard C# naming conventions (PascalCase for public members, _camelCase for private fields)
- ViewModels inherit from a `ViewModelBase` class implementing `INotifyPropertyChanged`
- Commands use `RelayCommand` / `DelegateCommand` pattern
- No code-behind in Views where possible — all logic in ViewModels
- XAML files use `DataBinding` exclusively (no event handlers in code-behind unless absolutely necessary)
- All service dependencies are injected via constructor injection (DIP)
- Each use case class has a single `Execute()` method (SRP)

### Commenting Conventions
- Keep comments **short and purposeful** — no restating what the code already says
- Comment **why**, not **what** (decisions, trade-offs, non-obvious reasoning)
- Use `// ADR-NNN:` prefix when a code decision traces to an architecture decision record
- XML doc comments (`///`) on public interfaces and service contracts only
- No comment noise: no `// Constructor`, `// Properties`, `// Methods` section markers
- Example:
  ```csharp
  // ADR-002: Hardcoded data — swap via IMusicLibraryService for file-based loading
  private static readonly List<Track> _tracks = new() { ... };
  ```

### File / Folder Naming
- Entities (Domain): singular nouns (`Track.cs`, `Playlist.cs`, `Artist.cs`)
- Use Cases (Application): `[Action][Entity]UseCase.cs` (e.g., `CreatePlaylistUseCase.cs`)
- Service Interfaces (Application): `I[Name]Service.cs`
- Service Implementations (Infrastructure): `[Name]Service.cs`
- ViewModels (WPF): `[View]ViewModel.cs` (e.g., `MainViewModel.cs`, `LibraryViewModel.cs`)
- Views (WPF): `[Name]View.xaml` (e.g., `MainWindow.xaml`, `LibraryView.xaml`)

### MVVM Rules
1. **Views** know about ViewModels (via DataContext binding) but never about Models directly
2. **ViewModels** depend on Application interfaces/use cases but never about Views or Infrastructure
3. **Domain Entities** are pure data — no UI or framework awareness
4. Navigation is handled via ViewModel switching, not frame-based navigation
5. DI composition root is in `App.xaml.cs` (WPF project only)

### Dependency Rules
- **Domain** depends on NOTHING — it is the innermost layer
- **Application** depends only on Domain
- **Infrastructure** depends on Application + Domain (implements interfaces)
- **WPF** depends on all layers but only for DI wiring; ViewModels use Application interfaces only

### Data Source
- Track data is **hardcoded** — 35 tracks across 7 albums and 6 artists (Tamil: A.R. Rahman, Ilaiyaraaja, Yuvan Shankar Raja + English: Pink Floyd, Coldplay, Radiohead)
- Implemented in `SeedData.cs` (`AlconMusicPlayer.Infra/Data/`) — `SeedData.Build()` returns a typed tuple `(IReadOnlyList<Track>, IReadOnlyList<Artist>, IReadOnlyList<Album>)` consumed by repositories
- Data includes: Title, Artist, Album, Duration (int seconds), FilePath, Genre, PlayCount

### UI / Styling
- **Dark theme** (iTunes-like dark UI with red/accent highlight for selections)
- Global resources in `Resources/` folder (merged into `App.xaml`):
  - `Colors.xaml` — color palette (background, foreground, accent, hover, selected)
  - `Styles.xaml` — reusable control styles (DataGrid, ListBox, Button, TextBox, ScrollBar, etc.)
  - `DataGridStyles.xaml` — DataGrid-specific templates and row styles
  - `Icons.xaml` — vector icon geometries (playback controls, sidebar icons)
- All views reference global resources via `{StaticResource}` or `{DynamicResource}` — no inline colors/styles
- Custom control templates for consistent look across the app

## How to Build & Run

```bash
# From the src/ directory (where AlconMusicPlayer.sln lives):
cd AlconMusicPlayer/src
dotnet build
dotnet run --project AlconMusicPlayer.WPF
```

## How to Continue Development

1. Read `status.md` to see current progress
2. Read `journal.md` for the latest session entry
3. Check `decisions.md` if you need to understand why something was done a certain way
4. Refer to `Requirement/MusicPlayerRequirements.md` for the original spec and UI layouts
