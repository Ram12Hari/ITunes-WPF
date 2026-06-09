# Software Requirements Specification (SRS)

> **Document ID**: IEC-DOC-002  
> **Version**: 1.0  
> **Date**: 2026-04-09  
> **IEC 62304 Clause**: 5.2  
> **Status**: Draft  
> **Source**: `Requirement/MusicPlayerRequirements.md`  
> **Approved by**: _Pending_

---

## 1. Purpose

This document formally defines the software requirements for the **Alcon Music Player**. Each
requirement is uniquely identified, traceable to test cases in [STP.md](STP.md), and reviewable.

---

## 2. System Overview

An iTunes-like WPF desktop music player built with C# and .NET 8. Users can browse songs, manage
playlists, and filter by artist and album. No audio playback is required.

---

## 3. Functional Requirements

### 3.1 Library — Song Browsing

| ID | Requirement | Priority |
|----|------------|---------|
| SRS-F-001 | The system shall display a list of all available songs in a DataGrid with columns: Title, Time, Artist, Album, Genre, Plays | Must |
| SRS-F-002 | The system shall display at least 35 songs sourced from hardcoded seed data | Must |
| SRS-F-003 | The system shall allow the user to search songs by title using a search text box | Must |
| SRS-F-004 | The system shall filter the displayed song list in real-time as the user types in the search box | Must |
| SRS-F-005 | The system shall allow clearing the search filter via a clear/reset action | Must |
| SRS-F-006 | The system shall visually indicate the currently selected song (accent highlight) | Must |

### 3.2 Artist Browsing

| ID | Requirement | Priority |
|----|------------|---------|
| SRS-F-010 | The system shall display a list of all artists | Must |
| SRS-F-011 | The system shall display the albums for a selected artist | Must |
| SRS-F-012 | The system shall display the songs for a selected artist or album combination | Must |

### 3.3 Album Browsing

| ID | Requirement | Priority |
|----|------------|---------|
| SRS-F-015 | The system shall display a list of all albums | Must |
| SRS-F-016 | The system shall display the songs belonging to a selected album | Must |

### 3.4 Playlist Management

| ID | Requirement | Priority |
|----|------------|---------|
| SRS-F-020 | The system shall display a list of all user-created playlists in the sidebar | Must |
| SRS-F-021 | The system shall allow the user to create a new playlist by entering a name | Must |
| SRS-F-022 | The system shall reject playlist creation if the name is empty | Must |
| SRS-F-023 | The system shall reject playlist creation if a playlist with the same name already exists | Must |
| SRS-F-024 | The system shall display an error message when playlist creation fails | Must |
| SRS-F-025 | The system shall display the songs within a selected playlist | Must |
| SRS-F-026 | The system shall allow adding a song to a playlist via a context menu | Must |
| SRS-F-027 | The system shall allow removing a song from a playlist | Must |
| SRS-F-028 | The system shall display the number of songs and total duration in the playlist detail view | Should |

### 3.5 Navigation

| ID | Requirement | Priority |
|----|------------|---------|
| SRS-F-030 | The system shall provide sidebar navigation to: Songs, Artists, Albums, and Playlists views | Must |
| SRS-F-031 | The system shall switch the main content area when the user selects a sidebar item | Must |
| SRS-F-032 | The system shall display a Now Playing bar at the top showing the currently selected track title | Must |

### 3.6 Context Menu

| ID | Requirement | Priority |
|----|------------|---------|
| SRS-F-035 | The system shall show a context menu on right-click of a song row | Must |
| SRS-F-036 | The context menu shall contain an "Add to Playlist" submenu listing all available playlists | Must |
| SRS-F-037 | The context menu shall contain an "Add to Last Playlist" option | Should |

### 3.7 Settings / Theme

| ID | Requirement | Priority |
|----|------------|---------|
| SRS-F-040 | The system shall apply a dark theme by default (iTunes-like dark UI with red/accent highlight) | Must |
| SRS-F-041 | The system shall provide a Settings view accessible from the sidebar | Should |

---

## 4. Non-Functional Requirements

| ID | Requirement | Category |
|----|------------|---------|
| SRS-NF-001 | The application shall start up and display the main window within 3 seconds on standard hardware | Performance |
| SRS-NF-002 | The application shall not crash or throw unhandled exceptions under normal use | Reliability |
| SRS-NF-003 | All unhandled exceptions shall be logged (see `logging-guide.md`) | Reliability |
| SRS-NF-004 | The application shall use the MVVM pattern — no business logic in View code-behind | Maintainability |
| SRS-NF-005 | The application shall use dependency injection for all service dependencies | Maintainability |
| SRS-NF-006 | The application shall be buildable with `dotnet build` without manual pre-steps | Buildability |
| SRS-NF-007 | The application shall not require an internet connection or external data source | Portability |

---

## 5. Constraints

| ID | Constraint |
|----|-----------|
| SRS-C-001 | Target framework: .NET 8 LTS (`net8.0-windows`) |
| SRS-C-002 | No actual audio playback is required or implemented |
| SRS-C-003 | Song data may be hardcoded (file-based loading is optional) |
| SRS-C-004 | Only approved NuGet packages may be used (see SDP.md §5.4) |

---

## 6. Data Requirements

| ID | Requirement |
|----|------------|
| SRS-D-001 | The system shall include at least 35 tracks across at least 7 albums and 6 artists |
| SRS-D-002 | Each track shall have: Title, Artist, Album, Duration (seconds), FilePath, Genre, PlayCount |
| SRS-D-003 | Artists shall include both Tamil and English genres |

---

## 7. Revision History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.0 | 2026-04-09 | Development Team | Initial formal requirements extracted from MusicPlayerRequirements.md |
