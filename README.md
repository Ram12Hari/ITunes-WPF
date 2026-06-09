# Alcon Music Player

A **WPF desktop music player** built with C# (.NET 8) following **Clean Architecture** and **MVVM** principles. Designed as a library/playlist management tool inspired by iTunes — browse tracks, manage playlists, and navigate by artist and album.

---

## Architecture

The solution enforces strict dependency boundaries across four layers:

| Layer | Project | Responsibility |
|-------|---------|---------------|
| Domain | `AlconMusicPlayer.Domain` | Entities, enums, repository interfaces |
| Application | `AlconMusicPlayer.ApplicationService` | Use cases, service contracts |
| Infrastructure | `AlconMusicPlayer.Infra` | In-memory repositories, seed data, services |
| Presentation | `AlconMusicPlayer.WPF` | Views, ViewModels, DI wiring, file logging |

## Features

- **Library browsing** — DataGrid displaying tracks across multiple albums and artists
- **Playlist management** — Create, rename, delete playlists; add/remove tracks
- **Sidebar navigation** — Filter by artist, album, or playlist
- **Theme switching** — Light and dark theme support
- **Context menu** — Right-click to add tracks to playlists
- **Structured logging** — Custom file logger provider with level filtering

## Technical Highlights

- SOLID principles enforced across all layers
- Repository pattern with swappable in-memory implementations
- Use Case pattern — one class per operation (`CreatePlaylistUseCase`, `AddTrackToPlaylistUseCase`, etc.)
- Dependency Injection via `Microsoft.Extensions.DependencyInjection`
- xUnit test suite covering Domain, Application, Infrastructure, and ViewModel layers
- IEC 62304 compliance documentation (SRS, SAD, SDD, SDP, STP, Risk Analysis)

## Tech Stack

- .NET 8 / WPF
- C# with nullable reference types
- xUnit (unit testing)
- Zero third-party runtime dependencies

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows (WPF requires Windows)

### Build & Run

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project src/AlconMusicPlayer.WPF

# Run all tests
dotnet test
```

## Project Structure

```
├── src/
│   ├── AlconMusicPlayer.Domain/            # Entities & interfaces
│   ├── AlconMusicPlayer.ApplicationService/ # Use cases & service contracts
│   ├── AlconMusicPlayer.Infra/             # Repositories & services
│   └── AlconMusicPlayer.WPF/              # WPF presentation layer
├── tests/
│   ├── AlconMusicPlayer.Domain.Tests/
│   ├── AlconMusicPlayer.ApplicationService.Tests/
│   ├── AlconMusicPlayer.Infra.Tests/
│   └── AlconMusicPlayer.WPF.Tests/
├── Docs/                                   # Development guides & decisions
├── IEC_Standard_Docs/                      # IEC 62304 compliance documents
└── Requirement/                            # Requirements & design diagrams
```

## Screenshots

See [SCREENSHOTS.md](SCREENSHOTS.md) for app screenshots.

## License

This project is licensed under the [MIT License](LICENSE).

---

*Built as a Clean Architecture demonstration with full traceability from requirements through design to implementation.*
