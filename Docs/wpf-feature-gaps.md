# WPF Feature Gaps — Implementation Guide

> **Purpose**: Step-by-step code for 3 missing UX features:
> 1. Add a track to an existing playlist (context menu)
> 2. Add a track to a new playlist (context menu → dialog)
> 3. Clicking an Artist or Album in the sidebar filters the track list

---

## Overview of Changes

| File | What Changes |
|---|---|
| `Base/RelayCommand.cs` | Add generic `RelayCommand<T>` for parameterized commands |
| `Dialogs/NewPlaylistDialog.xaml` | Simple input dialog for creating a new playlist |
| `LibraryViewModel.cs` | Add `AvailablePlaylists`, `SelectedTrack`, `ActiveFilterLabel`, context menu commands, filter methods |
| `ArtistsViewModel.cs` | Add `SelectedArtist` property |
| `AlbumsViewModel.cs` | Add `SelectedAlbum` property |
| `MainViewModel.cs` | Subscribe to artist/album selection → filter LibraryViewModel |
| `LibraryView.xaml` | Add filter label bar + context menu on rows |
| `ArtistsView.xaml` | Bind `SelectedItem` to `SelectedArtist` |
| `AlbumsView.xaml` | Bind `SelectedItem` to `SelectedAlbum` |
| `App.xaml.cs` | Fix `SeedData.Build()` tuple bug + register new dependencies |

---

## Step 1 — Add `RelayCommand<T>` to Base

A parameterized command — needed so the context menu can pass a `Playlist` object as the command parameter.

Create / update `src/AlconMusicPlayer.WPF/ViewModels/Base/RelayCommand.cs` — **add this class** alongside the existing `RelayCommand`:

```csharp
// Add below existing RelayCommand class in RelayCommand.cs

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) =>
        _canExecute?.Invoke((T?)parameter) ?? true;

    public void Execute(object? parameter) =>
        _execute((T?)parameter);
}
```

---

## Step 2 — New Playlist Input Dialog

A minimal WPF Window that accepts a playlist name from the user.

Create folder: `src/AlconMusicPlayer.WPF/Dialogs/`

**`Dialogs/NewPlaylistDialog.xaml`**

```xml
<Window x:Class="AlconMusicPlayer.WPF.Dialogs.NewPlaylistDialog"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="New Playlist" Height="140" Width="360"
        WindowStartupLocation="CenterOwner"
        ResizeMode="NoResize"
        Background="#1C1C1E">
    <StackPanel Margin="16">
        <TextBlock Text="Playlist name:"
                   Foreground="#F2F2F7" Margin="0,0,0,8" />
        <TextBox x:Name="NameBox"
                 Background="#2C2C2E" Foreground="#F2F2F7"
                 BorderBrush="#3A3A3C" Padding="8,6"
                 KeyDown="NameBox_KeyDown" />
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
            <Button Content="Cancel" Width="75" Margin="0,0,8,0"
                    Click="Cancel_Click"
                    Background="#3A3A3C" Foreground="#F2F2F7"
                    BorderThickness="0" Padding="8,6" />
            <Button Content="Create" Width="75"
                    Click="Create_Click"
                    Background="#FF453A" Foreground="#F2F2F7"
                    BorderThickness="0" Padding="8,6" />
        </StackPanel>
    </StackPanel>
</Window>
```

**`Dialogs/NewPlaylistDialog.xaml.cs`**

```csharp
using System.Windows;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.Dialogs;

public partial class NewPlaylistDialog : Window
{
    /// <summary>The name entered by the user. Null if cancelled.</summary>
    public string? PlaylistName { get; private set; }

    public NewPlaylistDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => NameBox.Focus();
    }

    private void Create_Click(object sender, RoutedEventArgs e) => TryAccept();
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void NameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)  TryAccept();
        if (e.Key == Key.Escape) DialogResult = false;
    }

    private void TryAccept()
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) return;
        PlaylistName = NameBox.Text.Trim();
        DialogResult = true;
    }
}
```

---

## Step 3 — LibraryViewModel (full replacement)

Adds:
- `SelectedTrack` — bound to DataGrid's selected row
- `AvailablePlaylists` — drives the context menu submenu
- `AddToPlaylistCommand` — `RelayCommand<Playlist>` — adds selected track to a given playlist
- `AddToNewPlaylistCommand` — shows the dialog, creates playlist, adds track
- `FilterByArtist(Artist?)` / `FilterByAlbum(Album?)` — called by MainViewModel
- `ActiveFilterLabel` — displayed in the view ("Showing: Pink Floyd")
- `ClearFilterCommand` — resets artist/album filter
- `ApplyFilter()` now combines search text + artist/album filter

```csharp
// src/AlconMusicPlayer.WPF/ViewModels/LibraryViewModel.cs

using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.Dialogs;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicService;
    private readonly IPlaylistService _playlistService;
    private readonly CreatePlaylistUseCase _createPlaylistUseCase;
    private readonly AddTrackToPlaylistUseCase _addTrackUseCase;

    private List<Track> _allTracks = [];
    private Artist? _activeArtistFilter;
    private Album? _activeAlbumFilter;

    // --- Tracks ---
    private ObservableCollection<Track> _tracks = [];
    public ObservableCollection<Track> Tracks
    {
        get => _tracks;
        set => SetProperty(ref _tracks, value);
    }

    private Track? _selectedTrack;
    public Track? SelectedTrack
    {
        get => _selectedTrack;
        set => SetProperty(ref _selectedTrack, value);
    }

    // --- Search ---
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); ApplyFilter(); }
    }

    // --- Filter label (shown when an artist/album filter is active) ---
    private string _activeFilterLabel = "";
    public string ActiveFilterLabel
    {
        get => _activeFilterLabel;
        set => SetProperty(ref _activeFilterLabel, value);
    }

    // --- Playlists (drives context menu submenu) ---
    public ObservableCollection<Playlist> AvailablePlaylists { get; } = [];

    // --- Error feedback ---
    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    // --- Commands ---
    public ICommand ClearSearchCommand    { get; }
    public ICommand ClearFilterCommand    { get; }
    public ICommand AddToPlaylistCommand  { get; }   // RelayCommand<Playlist>
    public ICommand AddToNewPlaylistCommand { get; } // opens dialog

    public LibraryViewModel(
        IMusicLibraryService musicService,
        IPlaylistService playlistService,
        CreatePlaylistUseCase createPlaylistUseCase,
        AddTrackToPlaylistUseCase addTrackUseCase)
    {
        _musicService         = musicService;
        _playlistService      = playlistService;
        _createPlaylistUseCase = createPlaylistUseCase;
        _addTrackUseCase      = addTrackUseCase;

        ClearSearchCommand      = new RelayCommand(() => SearchText = "");
        ClearFilterCommand      = new RelayCommand(ClearFilter, () => !string.IsNullOrEmpty(ActiveFilterLabel));
        AddToPlaylistCommand    = new RelayCommand<Playlist>(AddSelectedTrackToPlaylist,
                                      p => SelectedTrack != null && p != null);
        AddToNewPlaylistCommand = new RelayCommand(AddSelectedTrackToNewPlaylist,
                                      () => SelectedTrack != null);

        LoadTracks();
        RefreshPlaylists();
    }

    // Called by MainViewModel when user clicks an Artist in the sidebar
    public void FilterByArtist(Artist artist)
    {
        _activeArtistFilter = artist;
        _activeAlbumFilter  = null;
        ActiveFilterLabel   = $"Artist: {artist.Name}";
        SearchText          = "";
        ApplyFilter();
    }

    // Called by MainViewModel when user clicks an Album in the sidebar
    public void FilterByAlbum(Album album)
    {
        _activeAlbumFilter  = album;
        _activeArtistFilter = null;
        ActiveFilterLabel   = $"Album: {album.Name}";
        SearchText          = "";
        ApplyFilter();
    }

    public void RefreshPlaylists()
    {
        AvailablePlaylists.Clear();
        foreach (var p in _playlistService.GetAllPlaylists())
            AvailablePlaylists.Add(p);
    }

    private void LoadTracks()
    {
        _allTracks = _musicService.GetAllTracks().ToList();
        ApplyFilter();
    }

    private void ClearFilter()
    {
        _activeArtistFilter = null;
        _activeAlbumFilter  = null;
        ActiveFilterLabel   = "";
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<Track> result = _allTracks;

        // Artist/Album filter first
        if (_activeArtistFilter != null)
            result = result.Where(t => t.Artist.Id == _activeArtistFilter.Id);
        else if (_activeAlbumFilter != null)
            result = result.Where(t => t.Album.Id == _activeAlbumFilter.Id);

        // Then search text on top
        if (!string.IsNullOrWhiteSpace(SearchText))
            result = result.Where(t =>
                t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)  ||
                t.Artist.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Album.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Tracks = new ObservableCollection<Track>(result);
    }

    private void AddSelectedTrackToPlaylist(Playlist? playlist)
    {
        if (SelectedTrack == null || playlist == null) return;
        try
        {
            _addTrackUseCase.Execute(playlist.Id, SelectedTrack.Id);
            ErrorMessage = "";
            RefreshPlaylists();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void AddSelectedTrackToNewPlaylist()
    {
        if (SelectedTrack == null) return;

        // Show dialog — code-behind is acceptable for a modal dialog window
        var dialog = new NewPlaylistDialog { Owner = System.Windows.Application.Current.MainWindow };
        if (dialog.ShowDialog() != true || dialog.PlaylistName == null) return;

        try
        {
            _createPlaylistUseCase.Execute(dialog.PlaylistName);
            // Fetch the newly created playlist by name to get its ID
            var newPlaylist = _playlistService.GetAllPlaylists()
                .FirstOrDefault(p => p.Name == dialog.PlaylistName)
                ?? throw new InvalidOperationException("Playlist was not created.");

            _addTrackUseCase.Execute(newPlaylist.Id, SelectedTrack.Id);
            ErrorMessage = "";
            RefreshPlaylists();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
```

---

## Step 4 — ArtistsViewModel (add SelectedArtist)

```csharp
// src/AlconMusicPlayer.WPF/ViewModels/ArtistsViewModel.cs

using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;

namespace AlconMusicPlayer.WPF.ViewModels;

public class ArtistsViewModel : ViewModelBase
{
    public ObservableCollection<Artist> Artists { get; } = [];

    private Artist? _selectedArtist;
    public Artist? SelectedArtist
    {
        get => _selectedArtist;
        set => SetProperty(ref _selectedArtist, value);
        // MainViewModel listens to this via PropertyChanged
    }

    public ArtistsViewModel(IMusicLibraryService musicService)
    {
        foreach (var artist in musicService.GetAllArtists())
            Artists.Add(artist);
    }
}
```

---

## Step 5 — AlbumsViewModel (add SelectedAlbum)

```csharp
// src/AlconMusicPlayer.WPF/ViewModels/AlbumsViewModel.cs

using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;

namespace AlconMusicPlayer.WPF.ViewModels;

public class AlbumsViewModel : ViewModelBase
{
    public ObservableCollection<Album> Albums { get; } = [];

    private Album? _selectedAlbum;
    public Album? SelectedAlbum
    {
        get => _selectedAlbum;
        set => SetProperty(ref _selectedAlbum, value);
        // MainViewModel listens to this via PropertyChanged
    }

    public AlbumsViewModel(IMusicLibraryService musicService)
    {
        foreach (var album in musicService.GetAllAlbums())
            Albums.Add(album);
    }
}
```

---

## Step 6 — MainViewModel (wire filtering + playlist refresh)

```csharp
// src/AlconMusicPlayer.WPF/ViewModels/MainViewModel.cs

using AlconMusicPlayer.WPF.ViewModels.Base;
using System.ComponentModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly LibraryViewModel   _libraryViewModel;
    private readonly ArtistsViewModel   _artistsViewModel;
    private readonly AlbumsViewModel    _albumsViewModel;
    private readonly PlaylistsViewModel _playlistsViewModel;

    private ViewModelBase _currentView;
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ICommand ShowLibraryCommand   { get; }
    public ICommand ShowArtistsCommand   { get; }
    public ICommand ShowAlbumsCommand    { get; }
    public ICommand ShowPlaylistsCommand { get; }

    public MainViewModel(
        LibraryViewModel libraryViewModel,
        ArtistsViewModel artistsViewModel,
        AlbumsViewModel albumsViewModel,
        PlaylistsViewModel playlistsViewModel)
    {
        _libraryViewModel   = libraryViewModel;
        _artistsViewModel   = artistsViewModel;
        _albumsViewModel    = albumsViewModel;
        _playlistsViewModel = playlistsViewModel;

        _currentView = _libraryViewModel;

        ShowLibraryCommand   = new RelayCommand(() => CurrentView = _libraryViewModel);
        ShowArtistsCommand   = new RelayCommand(() => CurrentView = _artistsViewModel);
        ShowAlbumsCommand    = new RelayCommand(() => CurrentView = _albumsViewModel);
        ShowPlaylistsCommand = new RelayCommand(() =>
        {
            // Refresh playlist list every time user navigates to it
            _playlistsViewModel.Reload();
            CurrentView = _playlistsViewModel;
        });

        // When user selects an artist → switch to library and filter
        _artistsViewModel.PropertyChanged += OnArtistSelectionChanged;

        // When user selects an album → switch to library and filter
        _albumsViewModel.PropertyChanged += OnAlbumSelectionChanged;
    }

    private void OnArtistSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ArtistsViewModel.SelectedArtist)) return;
        if (_artistsViewModel.SelectedArtist == null) return;

        _libraryViewModel.FilterByArtist(_artistsViewModel.SelectedArtist);
        CurrentView = _libraryViewModel;
    }

    private void OnAlbumSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AlbumsViewModel.SelectedAlbum)) return;
        if (_albumsViewModel.SelectedAlbum == null) return;

        _libraryViewModel.FilterByAlbum(_albumsViewModel.SelectedAlbum);
        CurrentView = _libraryViewModel;
    }
}
```

> **Note**: `PlaylistsViewModel.Reload()` must be made `public` — see Step 7.

---

## Step 7 — Make PlaylistsViewModel.Reload() public

In `PlaylistsViewModel.cs`, change:
```csharp
// Before:
private void Reload()

// After:
public void Reload()
```

---

## Step 8 — LibraryView.xaml (filter bar + context menu)

### WPF Context Menu Gotcha
`ContextMenu` lives in a **separate visual tree** from the `DataGrid`, so normal `{Binding}` won't find the ViewModel. The fix: set `Tag="{Binding}"` on the `DataGrid` (which binds it to LibraryViewModel), then use `PlacementTarget.Tag` inside the ContextMenu to reach back to it.

```xml
<!-- src/AlconMusicPlayer.WPF/Views/LibraryView.xaml -->

<UserControl x:Class="AlconMusicPlayer.WPF.Views.LibraryView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />  <!-- Filter label bar -->
            <RowDefinition Height="Auto" />  <!-- Search bar -->
            <RowDefinition Height="Auto" />  <!-- Error bar -->
            <RowDefinition Height="*"    />  <!-- Track list -->
        </Grid.RowDefinitions>

        <!-- Active filter label (only visible when a filter is active) -->
        <Grid Grid.Row="0" Margin="0,0,0,8"
              Visibility="{Binding ActiveFilterLabel,
                           Converter={StaticResource StringToVisibilityConverter}}">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBlock Grid.Column="0"
                       Text="{Binding ActiveFilterLabel}"
                       Foreground="{StaticResource AccentBrush}"
                       FontSize="13" VerticalAlignment="Center" />
            <Button Grid.Column="1" Content="✕ Clear Filter"
                    Command="{Binding ClearFilterCommand}"
                    Background="Transparent"
                    Foreground="{StaticResource MutedBrush}"
                    BorderThickness="0" Padding="8,4" />
        </Grid>

        <!-- Search bar -->
        <Grid Grid.Row="1" Margin="0,0,0,8">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBox Grid.Column="0"
                     Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                     Padding="8,6" />
            <Button Grid.Column="1" Content="Clear"
                    Command="{Binding ClearSearchCommand}"
                    Margin="8,0,0,0" />
        </Grid>

        <!-- Error message bar -->
        <TextBlock Grid.Row="2"
                   Text="{Binding ErrorMessage}"
                   Foreground="{StaticResource AccentBrush}"
                   Margin="0,0,0,6"
                   Visibility="{Binding ErrorMessage,
                                Converter={StaticResource StringToVisibilityConverter}}" />

        <!-- Track DataGrid
             Tag="{Binding}" — exposes LibraryViewModel to the ContextMenu
             (ContextMenu lives in a separate visual tree and can't bind directly) -->
        <DataGrid Grid.Row="3"
                  ItemsSource="{Binding Tracks}"
                  SelectedItem="{Binding SelectedTrack}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  SelectionMode="Single"
                  Tag="{Binding}">

            <DataGrid.Columns>
                <DataGridTextColumn Header="Title"    Binding="{Binding Title}"            Width="*"   />
                <DataGridTextColumn Header="Artist"   Binding="{Binding Artist.Name}"      Width="150" />
                <DataGridTextColumn Header="Album"    Binding="{Binding Album.Name}"       Width="150" />
                <DataGridTextColumn Header="Genre"    Binding="{Binding Genre}"            Width="100" />
                <DataGridTextColumn Header="Duration" Binding="{Binding DurationDisplay}"  Width="80"  />
                <DataGridTextColumn Header="Plays"    Binding="{Binding PlayCount}"        Width="60"  />
            </DataGrid.Columns>

            <DataGrid.RowStyle>
                <Style TargetType="DataGridRow">
                    <Setter Property="ContextMenu">
                        <Setter.Value>
                            <!--
                                DataContext trick: PlacementTarget = the DataGridRow
                                DataGridRow.Tag is inherited from DataGrid.Tag = LibraryViewModel
                            -->
                            <ContextMenu DataContext="{Binding PlacementTarget.Tag,
                                                      RelativeSource={RelativeSource Self}}">

                                <!-- "Add to Playlist" submenu — ItemsSource = AvailablePlaylists -->
                                <MenuItem Header="Add to Playlist"
                                          ItemsSource="{Binding AvailablePlaylists}">
                                    <MenuItem.ItemContainerStyle>
                                        <Style TargetType="MenuItem">
                                            <Setter Property="Header" Value="{Binding Name}" />
                                            <!--
                                                Command and CommandParameter:
                                                - Command lives on LibraryViewModel (ContextMenu's DataContext)
                                                - CommandParameter is this Playlist item (the current {Binding})
                                                - AncestorType=ContextMenu navigates up to get LibraryViewModel
                                            -->
                                            <Setter Property="Command"
                                                    Value="{Binding DataContext.AddToPlaylistCommand,
                                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}" />
                                            <Setter Property="CommandParameter" Value="{Binding}" />
                                        </Style>
                                    </MenuItem.ItemContainerStyle>
                                </MenuItem>

                                <Separator />

                                <!-- "Add to New Playlist..." opens NewPlaylistDialog -->
                                <MenuItem Header="Add to New Playlist…"
                                          Command="{Binding AddToNewPlaylistCommand}" />

                            </ContextMenu>
                        </Setter.Value>
                    </Setter>
                </Style>
            </DataGrid.RowStyle>

        </DataGrid>
    </Grid>
</UserControl>
```

---

## Step 9 — ArtistsView.xaml (bind SelectedItem)

```xml
<!-- src/AlconMusicPlayer.WPF/Views/ArtistsView.xaml -->

<UserControl x:Class="AlconMusicPlayer.WPF.Views.ArtistsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <TextBlock Grid.Row="0" Text="Artists"
                   Foreground="{StaticResource MutedBrush}"
                   FontSize="11" Margin="0,0,0,8" />
        <!--
            SelectedItem="{Binding SelectedArtist}" — two-way binding
            When user clicks an artist, ArtistsViewModel.SelectedArtist is set,
            which fires PropertyChanged, which MainViewModel picks up
        -->
        <ListBox Grid.Row="1"
                 ItemsSource="{Binding Artists}"
                 SelectedItem="{Binding SelectedArtist}"
                 DisplayMemberPath="Name" />
    </Grid>
</UserControl>
```

---

## Step 10 — AlbumsView.xaml (bind SelectedItem)

```xml
<!-- src/AlconMusicPlayer.WPF/Views/AlbumsView.xaml -->

<UserControl x:Class="AlconMusicPlayer.WPF.Views.AlbumsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <TextBlock Grid.Row="0" Text="Albums"
                   Foreground="{StaticResource MutedBrush}"
                   FontSize="11" Margin="0,0,0,8" />
        <ListBox Grid.Row="1"
                 ItemsSource="{Binding Albums}"
                 SelectedItem="{Binding SelectedAlbum}"
                 DisplayMemberPath="Name" />
    </Grid>
</UserControl>
```

---

## Step 11 — Add StringToVisibilityConverter

The filter label and error bar use a converter to hide when empty.

Create `src/AlconMusicPlayer.WPF/Converters/StringToVisibilityConverter.cs`:

```csharp
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlconMusicPlayer.WPF.Converters;

/// <summary>
/// Returns Visible when the string is non-empty, Collapsed when null or empty.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
```

Register it in `Resources/Styles.xaml` (or `App.xaml` resources):

```xml
<!-- Add inside <ResourceDictionary> in App.xaml or Styles.xaml -->
<converters:StringToVisibilityConverter x:Key="StringToVisibilityConverter" />
```

And add the namespace to whichever file declares it:
```xml
xmlns:converters="clr-namespace:AlconMusicPlayer.WPF.Converters"
```

---

## Step 12 — Fix App.xaml.cs

Two issues:
1. `SeedData.Build()` returns a tuple — must destructure with `var (tracks, _, _) = ...`
2. `LibraryViewModel` constructor now needs `IPlaylistService`, `CreatePlaylistUseCase`, `AddTrackToPlaylistUseCase` — DI resolves these automatically since they're already registered

```csharp
// Fix only this line in RegisterService():

// BEFORE (wrong — treats tuple as a List):
var tracks = SeedData.Build();
serviceProvider.AddSingleton<ITrackRepository>(new InMemoryTrackRepository(tracks.ToList()));

// AFTER (correct — destructure the tuple):
var (tracks, _, _) = SeedData.Build();
serviceProvider.AddSingleton<ITrackRepository>(new InMemoryTrackRepository(tracks.ToList()));
```

No other changes needed — all new dependencies (`IPlaylistService`, `CreatePlaylistUseCase`, `AddTrackToPlaylistUseCase`) are already registered in DI and will be injected into `LibraryViewModel` automatically.

---

## Summary of Data Flow

### Add track to existing playlist
```
User right-clicks row in LibraryView
  → DataGrid.SelectedItem = SelectedTrack (set by binding)
  → ContextMenu opens — submenu shows AvailablePlaylists
  → User clicks a playlist
  → AddToPlaylistCommand (RelayCommand<Playlist>) fires with that Playlist as parameter
  → AddTrackToPlaylistUseCase.Execute(playlist.Id, selectedTrack.Id)
  → AvailablePlaylists refreshed
```

### Add track to new playlist
```
User right-clicks row → "Add to New Playlist…"
  → AddToNewPlaylistCommand fires
  → NewPlaylistDialog shown
  → User types name + clicks Create
  → CreatePlaylistUseCase.Execute(name) — validates no duplicate
  → AddTrackToPlaylistUseCase.Execute(newPlaylist.Id, selectedTrack.Id)
```

### Artist/Album filtering
```
User navigates to Artists view → clicks "Pink Floyd"
  → ArtistsViewModel.SelectedArtist = Pink Floyd (via ListBox SelectedItem binding)
  → PropertyChanged fires with "SelectedArtist"
  → MainViewModel.OnArtistSelectionChanged() called
  → LibraryViewModel.FilterByArtist(Pink Floyd)
  → _activeArtistFilter set, ActiveFilterLabel = "Artist: Pink Floyd"
  → ApplyFilter() runs → Tracks updated
  → CurrentView switched to LibraryViewModel
  → LibraryView shows filtered tracks + "Artist: Pink Floyd" label with ✕ Clear Filter
```

---

## Checklist

```
Step 1   RelayCommand<T> added                          🔲
Step 2   NewPlaylistDialog.xaml + .cs created           🔲
Step 3   LibraryViewModel replaced                      🔲
Step 4   ArtistsViewModel — SelectedArtist added        🔲
Step 5   AlbumsViewModel — SelectedAlbum added          🔲
Step 6   MainViewModel — PropertyChanged wiring added   🔲
Step 7   PlaylistsViewModel.Reload() made public        🔲
Step 8   LibraryView.xaml — filter bar + context menu   🔲
Step 9   ArtistsView.xaml — SelectedItem bound          🔲
Step 10  AlbumsView.xaml — SelectedItem bound           🔲
Step 11  StringToVisibilityConverter created + registered 🔲
Step 12  App.xaml.cs SeedData tuple fix                 🔲
         dotnet build — verify 0 errors                 🔲
         Test: artist click → library filters           🔲
         Test: right-click → add to playlist submenu    🔲
         Test: right-click → add to new playlist dialog 🔲
```
