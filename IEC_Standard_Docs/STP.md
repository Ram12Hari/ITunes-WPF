# Software Test Plan (STP)

> **Document ID**: IEC-DOC-005  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clauses**: 5.5 (Unit), 5.6 (Integration), 5.7 (System)  
> **Status**: Draft  
> **Approved by**: _Pending_

---

## 1. Purpose

This document defines the test strategy, test cases, and traceability matrix for the
**Alcon Music Player**. Test results are recorded in [STR.md](STR.md).

---

## 2. Test Strategy

| Level | Scope | Tool | Location |
|-------|-------|------|----------|
| Unit | Individual classes/methods in Domain and ApplicationService | xUnit | `tests/` |
| Integration | Use cases interacting with in-memory repositories | xUnit | `tests/` |
| System | End-to-end ViewModel behaviour via manual test scripts | Manual | This document §5 |

---

## 3. Test Environment

| Item | Value |
|------|-------|
| Operating System | Windows 10/11 |
| .NET SDK | 8.0 |
| Test Framework | xUnit 2.9+ |
| Test Runner | `dotnet test` / Visual Studio Test Explorer |
| Build Configuration | Debug |

Run all automated tests:
```bash
dotnet test
```

---

## 4. Unit & Integration Test Cases

### 4.1 Domain — Track Entity

| Test ID | Method Under Test | Input | Expected Outcome | SRS Ref |
|---------|------------------|-------|-----------------|---------|
| UT-D-001 | `Track` constructor | Valid data | Properties set correctly | SRS-D-002 |
| UT-D-002 | `Track` constructor | Null title | `ArgumentException` thrown | SRS-D-002 |

### 4.2 Domain — Playlist Entity

| Test ID | Method Under Test | Input | Expected Outcome | SRS Ref |
|---------|------------------|-------|-----------------|---------|
| UT-D-010 | `Playlist.Create` | Valid name | Playlist created with trimmed name | SRS-F-021 |
| UT-D-011 | `Playlist.Create` | Empty name | `ArgumentException` thrown | SRS-F-022 |
| UT-D-012 | `Playlist.Create` | Whitespace-only name | `ArgumentException` thrown | SRS-F-022 |
| UT-D-013 | `Playlist.AddTrack` | Valid track | Track appears in playlist | SRS-F-026 |
| UT-D-014 | `Playlist.AddTrack` | Duplicate track | Track added only once (idempotent) | SRS-F-026 |
| UT-D-015 | `Playlist.RemoveTrack` | Existing track ID | Track removed from playlist | SRS-F-027 |
| UT-D-016 | `Playlist.RemoveTrack` | Non-existent track ID | No exception — no-op | SRS-F-027 |

### 4.3 Domain — Artist Entity

| Test ID | Method Under Test | Input | Expected Outcome | SRS Ref |
|---------|------------------|-------|-----------------|---------|
| UT-D-020 | `Artist` constructor | Valid name | Properties set correctly | SRS-D-001 |
| UT-D-021 | `Artist` constructor | Null/empty name | `ArgumentException` thrown | SRS-D-001 |

### 4.4 Domain — Album Entity

| Test ID | Method Under Test | Input | Expected Outcome | SRS Ref |
|---------|------------------|-------|-----------------|---------|
| UT-D-025 | `Album` constructor | Valid data | Properties set correctly | SRS-D-001 |

### 4.5 ApplicationService — CreatePlaylistUseCase

| Test ID | Method Under Test | Input | Expected Outcome | SRS Ref |
|---------|------------------|-------|-----------------|---------|
| UT-AS-001 | `Execute` | Valid unique name | Playlist added to repository | SRS-F-021 |
| UT-AS-002 | `Execute` | Empty string | `ArgumentException` thrown | SRS-F-022 |
| UT-AS-003 | `Execute` | Whitespace | `ArgumentException` thrown | SRS-F-022 |
| UT-AS-004 | `Execute` | Duplicate name (exact) | `InvalidOperationException` thrown | SRS-F-023 |
| UT-AS-005 | `Execute` | Duplicate name (different case) | `InvalidOperationException` thrown | SRS-F-023 |

### 4.6 ApplicationService — AddTrackToPlaylistUseCase

| Test ID | Method Under Test | Input | Expected Outcome | SRS Ref |
|---------|------------------|-------|-----------------|---------|
| UT-AS-010 | `Execute` | Valid playlist ID + valid track ID | Track added to playlist | SRS-F-026 |
| UT-AS-011 | `Execute` | Invalid playlist ID | `InvalidOperationException` thrown | SRS-F-026 |
| UT-AS-012 | `Execute` | Invalid track ID | `InvalidOperationException` thrown | SRS-F-026 |

### 4.7 ApplicationService — RemoveTrackFromPlaylistUseCase

| Test ID | Method Under Test | Input | Expected Outcome | SRS Ref |
|---------|------------------|-------|-----------------|---------|
| UT-AS-015 | `Execute` | Valid playlist + existing track ID | Track removed from playlist | SRS-F-027 |
| UT-AS-016 | `Execute` | Invalid playlist ID | `InvalidOperationException` thrown | SRS-F-027 |
| UT-AS-017 | `Execute` | Valid playlist + non-existent track ID | No exception — no-op | SRS-F-027 |

---

## 5. System Test Cases (Manual)

These tests verify end-to-end behaviour through the WPF UI. Execute after a successful `dotnet build`.

### 5.1 Application Startup

| Test ID | Steps | Expected Result | SRS Ref |
|---------|-------|----------------|---------|
| ST-001 | Launch application via `dotnet run --project` | Main window displays within 3 seconds with song list visible | SRS-NF-001 |
| ST-002 | Check `logs/` directory after startup | `alcon-music-player.log` exists and contains startup entry | SRS-NF-003 |

### 5.2 Song Library

| Test ID | Steps | Expected Result | SRS Ref |
|---------|-------|----------------|---------|
| ST-010 | Open app, click Songs in sidebar | DataGrid shows ≥35 tracks with Title, Time, Artist, Album, Genre, Plays columns | SRS-F-001, SRS-F-002 |
| ST-011 | Type "rahman" in search box | List filters to AR Rahman tracks in real time | SRS-F-003, SRS-F-004 |
| ST-012 | Click clear search | All tracks restored | SRS-F-005 |
| ST-013 | Click any track row | Row highlighted in accent colour | SRS-F-006 |

### 5.3 Artist and Album Browsing

| Test ID | Steps | Expected Result | SRS Ref |
|---------|-------|----------------|---------|
| ST-020 | Click Artists in sidebar | List of 6 artists displayed | SRS-F-010 |
| ST-021 | Select an artist | Albums for that artist displayed | SRS-F-011 |
| ST-022 | Select an album under the artist | Tracks for that album displayed | SRS-F-012 |
| ST-023 | Click Albums in sidebar | All albums listed | SRS-F-015 |
| ST-024 | Select an album | Tracks for that album displayed | SRS-F-016 |

### 5.4 Playlist Management

| Test ID | Steps | Expected Result | SRS Ref |
|---------|-------|----------------|---------|
| ST-030 | Click Playlists in sidebar | Playlist list and create field visible | SRS-F-020 |
| ST-031 | Enter "My Playlist", click Create | New playlist appears in sidebar list | SRS-F-021 |
| ST-032 | Leave name empty, click Create | Error message: name must not be empty | SRS-F-022 |
| ST-033 | Enter "My Playlist" again, click Create | Error message: duplicate name | SRS-F-023 |
| ST-034 | Click "My Playlist" in sidebar | Playlist detail view opens, shows 0 songs | SRS-F-025 |

### 5.5 Context Menu

| Test ID | Steps | Expected Result | SRS Ref |
|---------|-------|----------------|---------|
| ST-040 | Right-click a track in song list | Context menu appears | SRS-F-035 |
| ST-041 | Hover "Add to Playlist" | Submenu lists available playlists | SRS-F-036 |
| ST-042 | Select playlist from submenu | Track appears in that playlist's detail view | SRS-F-026 |

### 5.6 Logging Verification

| Test ID | Steps | Expected Result | SRS Ref |
|---------|-------|----------------|---------|
| ST-050 | Create a playlist, check log file | Log contains `[INF] ... Playlist created:` entry | SRS-NF-003 |
| ST-051 | Try to create duplicate playlist, check log | Log contains `[WRN] ... Duplicate playlist name rejected:` entry | SRS-NF-003 |
| ST-052 | Close application, check log file | Log contains shutdown entry | SRS-NF-003 |

---

## 6. Traceability Matrix

| SRS ID | Description | Test ID(s) |
|--------|------------|-----------|
| SRS-F-001 | Songs DataGrid with correct columns | ST-010 |
| SRS-F-002 | ≥35 songs displayed | ST-010 |
| SRS-F-003/004 | Real-time search filter | ST-011 |
| SRS-F-005 | Clear search | ST-012 |
| SRS-F-006 | Selected song highlight | ST-013 |
| SRS-F-010/011/012 | Artist browsing | ST-020, ST-021, ST-022 |
| SRS-F-015/016 | Album browsing | ST-023, ST-024 |
| SRS-F-020/021 | Create playlist | ST-030, ST-031 |
| SRS-F-022 | Reject empty name | ST-032, UT-AS-002, UT-AS-003 |
| SRS-F-023 | Reject duplicate name | ST-033, UT-AS-004, UT-AS-005 |
| SRS-F-025/026 | Add track to playlist | ST-034, ST-041/042, UT-AS-010 |
| SRS-F-027 | Remove track from playlist | UT-AS-015, UT-AS-016 |
| SRS-F-035/036 | Context menu | ST-040, ST-041 |
| SRS-NF-001 | Startup within 3 seconds | ST-001 |
| SRS-NF-003 | Logging enabled | ST-050, ST-051, ST-052 |
| SRS-NF-004 | MVVM pattern | Architecture review (SAD.md) |
| SRS-NF-005 | Dependency injection | Architecture review (SAD.md) |
| SRS-NF-006 | Builds with `dotnet build` | CI / manual build |
| SRS-D-001 | ≥35 tracks / 7 albums / 6 artists | ST-010, ST-020 |

---

## 7. Pass/Fail Criteria

- All unit and integration tests (`dotnet test`) must pass with 0 failures
- All system tests must result in "Pass"
- No open severity-1 or severity-2 problem reports (see [SPRP.md](SPRP.md))
- Results recorded in [STR.md](STR.md)

---

## 8. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial test plan |
