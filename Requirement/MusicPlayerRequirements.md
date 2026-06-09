# Music Player - WPF Application

## Project Overview

Create an iTunes-like music player using WPF. The application allows users to browse from a list of songs, create playlists, add/remove titles from playlists, and browse by artist and albums.

## Objectives

The goal is to match the user interface, layout, and styling as much as possible while incorporating the following WPF elements:

- **MVVM Pattern** (Model-View-ViewModel)
- **Styling** and themes
- **Data Binding**
- **Controls** (buttons, lists, etc.)
- **Data Grid**
- **Panels** (StackPanel, Grid, etc.)
- **Navigation**
- **Input Controls**
- **Commands**

## Requirements

### Core Features

1. **Browse Songs** - Display a list of available songs
2. **Create Playlist** - Allow users to create custom playlists
3. **Add/Remove Titles** - Manage songs within playlists
4. **Browse by Artist** - Filter and view songs by artist
5. **Browse by Albums** - Filter and view songs by album

### Technical Requirements

- List of songs for display can be:
  - Read from a file locally, **OR**
  - Hard coded in the application
- **Extra Credit**: Unit testing (view-model and model)

### Important Notes

- **No need to playback the music**


## UI Layout Reference

### Layout 1: Main Song List View
┌─────────────────────────────────────────────────────────────────────────────────────┐
│ ●●● 🔀 ⏮ ▶ ⏭ 🔁 All I Want for Christmas Is You │
├──────────────┬──────────────────────────────────────────────────────────────────────┤
│ 🔍 Search │ Songs │
│ │ ┌────────────────────────────────────────────────────────────────┐ │
│ Library │ │ Title Time Artist Album Genre Plays │ │
│ ○ Recently │ │ Digital Booklet U2 Songs of Innocence Rock │ │
│ Added │ │ ▶ All I Want.. 4:01 U2 Songs of Innocence Rock │ │
│ 🎤 Artists │ │ New Record.. 0:25 1 │ │
│ 📀 Albums │ │ New Record.. 0:33 1 │ │
│ 🎵 Songs │ │ New Record.. 0:23 1 │ │
│ │ │ New Record.. 0:19 │ │
│ Playlists │ └────────────────────────────────────────────────────────────────┘ │
│ 📋 All │ │
│ Playlists │ │
│ 🎵 Playlist │ │
└──────────────┴──────────────────────────────────────────────────────────────────────┘

### Layout 2: Context Menu - Add to Playlist
┌─────────────────────────────────────────────────────────────────────────────────────┐
│ ●●● 🔀 ⏮ ▶ ⏭ 🔁 All I Want for Christmas Is You │
├──────────────┬──────────────────────────────────────────────────────────────────────┤
│ 🔍 Search │ Songs │
│ │ ┌────────────────────────────────────────────────────────────────┐ │
│ Library │ │ Title Time Artist Album Genre Plays │ │
│ ○ Recently │ │ Digital Booklet U2 Songs of Innocence Rock │ │
│ Added │ │ ▶ All I Want.. ┌──────────────────────────────┐ │ │
│ 🎤 Artists │ │ New Record.. │ Add to Last Playlist │ 1 │ │
│ 📀 Albums │ │ New Record.. │ Add to Playlist ▶ │ New Playlist │ │
│ 🎵 Songs │ │ New Record.. │ Play Next │ 🎵 Playlist │ │
│ │ │ New Record.. │ Play Later │ │ │
│ Playlists │ │ └──────────────────────────────┘ │ │
│ 📋 All │ └────────────────────────────────────────────────────────────────┘ │
│ Playlists │ │
│ 🎵 Playlist │ │
└──────────────┴──────────────────────────────────────────────────────────────────────┘

### Layout 3: Playlist Detail View
┌─────────────────────────────────────────────────────────────────────────────────────┐
│ ●●● 🔀 ⏮ ▶ ⏭ 🔁 All I Want for Christmas Is You │
├──────────────┬──────────────────────────────────────────────────────────────────────┤
│ 🔍 Search │ │
│ │ ┌──────────┐ │
│ Library │ │ │ ┌────────────────────────────────────────────────┐ │
│ ○ Recently │ │ 🎵 │ │ Trial │ │
│ Added │ │ │ └────────────────────────────────────────────────┘ │
│ 🎤 Artists │ │ │ 1 SONG · 4 MINUTES │
│ 📀 Albums │ │ │ │
│ 🎵 Songs │ └──────────┘ Add Description │
│ │ │
│ Playlists │ ┌──────────┐ ⋯ │
│ 📋 All │ │ ▶ Play │ │
│ Playlists │ └──────────┘ │
│ 🎵 Playlist │ │
│ 🎵 Playlist 2│ ┌────────────────────────────────────────────────────────────────┐ │
│ │ │ Song Artist Album Time │ │
│ │ │ 🎵 All I Want for Christmas Is You 4:01 │ │
│ │ │ │ │
│ │ └────────────────────────────────────────────────────────────────┘ │
└──────────────┴──────────────────────────────────────────────────────────────────────┘

## Key UI Components to Implement

### 1. Left Sidebar Navigation
- Search box at top
- Library section with:
  - Recently Added
  - Artists
  - Albums
  - Songs
- Playlists section with:
  - All Playlists
  - Individual playlists (dynamic list)

### 2. Top Control Bar
- Window controls (minimize, maximize, close)
- Playback controls (shuffle, previous, play/pause, next, repeat)
- Now playing song title
- Volume slider
- Additional controls (AirPlay, lyrics, queue)

### 3. Main Content Area
- **Songs View**: DataGrid with columns:
  - Title
  - Time
  - Artist
  - Album
  - Genre
  - Plays (play count)
- **Playlist View**: 
  - Playlist artwork/icon
  - Editable playlist name
  - Song count and duration
  - Description field
  - Play button
  - Song list

### 4. Context Menu
- Add to Last Playlist
- Add to Playlist (with submenu showing available playlists)
- Play Next
- Play Later

### 5. Visual States
- Selected song (highlighted in red/accent color)
- Hover states
- Playing indicator (▶ symbol)
- Editable text fields