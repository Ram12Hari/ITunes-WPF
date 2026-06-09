# WPF ViewModel Testing Plan

## Goal

Add a dedicated xUnit test project for WPF view models on .NET 8 with minimal third-party dependencies and a phased rollout based on risk and behavior density.

## Project Added

- New test project: `tests/AlconMusicPlayer.WPF.Tests`
- Target framework: `net8.0-windows`
- Test framework: xUnit
- Mocking library: Moq
- Deliberately excluded: extra WPF-specific test packages and coverage packages, to keep dependencies minimal

## Dependency Decisions

1. **Keep dependencies small**
   - Added only `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, and `Moq`.
   - Did not add `coverlet.collector` because the request prioritized minimal third-party dependencies.

2. **Use concrete use cases where useful**
   - For playlist-related behaviors, tests instantiate real use case classes and mock repository boundaries.
   - This keeps the view model tests close to actual application behavior without pulling in infrastructure.

3. **Avoid UI-coupled modal testing in the first pass**
   - `LibraryViewModel.AddToNewPlaylistCommand` opens `NewPlaylistDialog` directly.
   - That path is not ideal for a pure unit test, so it is intentionally deferred until dialog creation is abstracted behind an interface.

4. **Phase by business risk, not file count**
   - View models with command orchestration, filtering, navigation, and playback state were prioritized.
   - Simple collection loaders such as `ArtistsViewModel` and `AlbumsViewModel` are lower priority.

## ViewModel Priority Matrix

| Priority | ViewModel | Reason | Phase |
|---|---|---|---|
| High | `NowPlayingViewModel` | Playback state, queue navigation, command enablement | Phase 1 |
| High | `PlaylistsViewModel` | Playlist creation workflow and validation feedback | Phase 1 |
| High | `SettingsViewModel` | Theme switching side effects | Phase 1 |
| High | `LibraryViewModel` | Search, artist/album filtering, add-to-playlist, now-playing integration | Phase 2 |
| High | `PlaylistViewModel` | Load/remove/add flows and back navigation | Phase 2 |
| High | `MainViewModel` | Cross-view coordination and screen switching | Phase 2 |
| Low | `ArtistsViewModel` | Thin list loader with simple selection state | Phase 3 |
| Low | `AlbumsViewModel` | Thin list loader with simple selection state | Phase 3 |

## Implementation Steps Completed

### Phase 1
- Created the WPF view model test project.
- Added shared test data helpers for `Artist`, `Album`, `Track`, and `Playlist` creation.
- Added tests for:
  - `NowPlayingViewModel`
  - `PlaylistsViewModel`
  - `SettingsViewModel`

### Phase 2
- Added tests for:
  - `LibraryViewModel`
  - `PlaylistViewModel`
  - `MainViewModel`
- Grouped tests by phase using folders to make the rollout explicit.

### Phase 3 Backlog
- Add lightweight tests for `ArtistsViewModel` and `AlbumsViewModel`.
- Refactor dialog creation in `LibraryViewModel` behind an abstraction so `AddToNewPlaylistCommand` can be unit tested cleanly.

## Expected Test Coverage Areas

### Phase 1 coverage
- Constructor initialization
- Command execution and enablement
- State transitions
- Validation and error message flow

### Phase 2 coverage
- Filtering and search behavior
- Playlist add/remove behavior
- View-to-view orchestration in `MainViewModel`
- Playback delegation from library and playlist screens

## Notes

- The new test project references the WPF project directly so the tests exercise the real view model implementations.
- The tests stay at the view model boundary and avoid infrastructure dependencies.
- If WPF-thread-specific test issues appear later, add the smallest possible STA/WPF test support only when proven necessary.
