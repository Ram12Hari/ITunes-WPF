# WPF Wiring Guide — Alcon Music Player

> **Purpose**: Step-by-step guide to building the WPF layer from scratch. Written for someone new to WPF.
> Assumes Domain, Application, and Infrastructure layers are complete and functional.

---

## Part 1 — Core WPF Concepts (Read First)

### 1.1 XAML
XML-based markup language for defining UI. Each tag is a C# object.
```xml
<TextBlock Text="Hello" FontSize="16" />
<!-- equivalent to: var t = new TextBlock(); t.Text = "Hello"; t.FontSize = 16; -->
```

### 1.2 DataBinding
Connects a UI control's property to a ViewModel property. The UI updates automatically when the ViewModel changes.
```xml
<TextBlock Text="{Binding Title}" />
<!-- reads ViewModel.Title and displays it -->

<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}" />
<!-- two-way: also writes back to ViewModel.SearchText on every keystroke -->
```

### 1.3 INotifyPropertyChanged
The mechanism that tells the UI "this property just changed, re-read it."
Every ViewModel inherits `ViewModelBase` which implements this.
```csharp
// In ViewModelBase:
protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
{
    field = value;
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// In a ViewModel:
private string _searchText = "";
public string SearchText
{
    get => _searchText;
    set => SetProperty(ref _searchText, value);  // fires PropertyChanged automatically
}
```

### 1.4 Commands
Replace event handlers in code-behind with bindable actions on the ViewModel.
```xml
<Button Command="{Binding ClearSearchCommand}" Content="Clear" />
```
```csharp
// In ViewModel:
public ICommand ClearSearchCommand { get; }
// Assigned in constructor:
ClearSearchCommand = new RelayCommand(() => SearchText = "");
```

### 1.5 DataTemplate
Maps a ViewModel type to a View. When WPF sees an object of type X, it knows to render it using View Y.
```xml
<!-- In App.xaml — register once, works everywhere -->
<DataTemplate DataType="{x:Type vm:LibraryViewModel}">
    <views:LibraryView />
</DataTemplate>
```

### 1.6 ContentControl + ViewModel-First Navigation
A `ContentControl` renders whatever object is in its `Content` property.
Combined with DataTemplates, switching views = just changing a property on `MainViewModel`.
```xml
<ContentControl Content="{Binding CurrentView}" />
<!-- When CurrentView = LibraryViewModel instance → shows LibraryView -->
<!-- When CurrentView = ArtistsViewModel instance → shows ArtistsView -->
```

### 1.7 DataContext
The object a UI element pulls its bindings from.
Set on the root window once, and all child elements inherit it automatically.
```csharp
// In App.xaml.cs:
MainWindow.DataContext = mainViewModel;
```

---

## Part 2 — Build Order

Build in this exact order. Each step depends on the previous one.

```
Step 1 → ViewModelBase + RelayCommand         (foundation for all ViewModels)
Step 2 → App.xaml.cs DI + composition root    (wires the whole app together)
Step 3 → App.xaml Resources + DataTemplates   (theme + ViewModel→View mapping)
Step 4 → MainWindow.xaml shell layout         (top bar + sidebar + content area)
Step 5 → LibraryViewModel + LibraryView       (main track list, proves the stack works)
Step 6 → ArtistsViewModel + ArtistsView       (artist browsing)
Step 7 → AlbumsViewModel + AlbumsView         (album browsing)
Step 8 → PlaylistsViewModel + PlaylistsView   (all playlists)
Step 9 → PlaylistViewModel + PlaylistView     (single playlist detail)
Step 10 → Sidebar navigation wiring           (clicking sidebar changes CurrentView)
Step 11 → Converters                          (duration formatting, bool→visibility)
```

---

## Part 3 — Step-by-Step Implementation

---

### Step 1: ViewModelBase + RelayCommand

Create folder: `src/AlconMusicPlayer.WPF/ViewModels/Base/`

**`ViewModelBase.cs`**
```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlconMusicPlayer.WPF.ViewModels.Base;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

**`RelayCommand.cs`**
```csharp
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels.Base;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
}
```

---

### Step 2: App.xaml.cs — DI Composition Root

Install NuGet first:
```bash
dotnet add src/AlconMusicPlayer.WPF package Microsoft.Extensions.DependencyInjection
```

**`App.xaml.cs`**
```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Infra.Data;
using AlconMusicPlayer.Infra.Repositories;
using AlconMusicPlayer.Infra.Services;
using AlconMusicPlayer.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AlconMusicPlayer.WPF;

public partial class App : Application
{
    private ServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        RegisterServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Seed data — build once, share the track list
        var (tracks, _, _) = SeedData.Build();

        // Repositories
        services.AddSingleton<ITrackRepository>(new InMemoryTrackRepository(tracks.ToList()));
        services.AddSingleton<IPlaylistRepository, InMemoryPlaylistRepository>();

        // Services
        services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();

        // ViewModels
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<ArtistsViewModel>();` 
        services.AddTransient<AlbumsViewModel>();
        services.AddTransient<PlaylistsViewModel>();
        services.AddTransient<PlaylistViewModel>();
        services.AddSingleton<MainViewModel>();

        // Window
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
```

> ⚠️ Also remove `StartupUri="MainWindow.xaml"` from `App.xaml` — we're launching the window manually now.

---

### Step 3: App.xaml — Resources + DataTemplates

**`App.xaml`** (replace entirely)
```xml
<Application x:Class="AlconMusicPlayer.WPF.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:AlconMusicPlayer.WPF.ViewModels"
             xmlns:views="clr-namespace:AlconMusicPlayer.WPF.Views">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>

            <!-- ViewModel → View mapping. WPF uses these automatically
                 when it sees a ViewModel inside a ContentControl -->
            <DataTemplate DataType="{x:Type vm:LibraryViewModel}">
                <views:LibraryView />
            </DataTemplate>
            <DataTemplate DataType="{x:Type vm:ArtistsViewModel}">
                <views:ArtistsView />
            </DataTemplate>
            <DataTemplate DataType="{x:Type vm:AlbumsViewModel}">
                <views:AlbumsView />
            </DataTemplate>
            <DataTemplate DataType="{x:Type vm:PlaylistsViewModel}">
                <views:PlaylistsView />
            </DataTemplate>
            <DataTemplate DataType="{x:Type vm:PlaylistViewModel}">
                <views:PlaylistView />
            </DataTemplate>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

---

### Step 4: MainWindow.xaml — Shell Layout

3-panel layout: top bar (60px) | sidebar (200px) | content area (fills rest).

**`MainWindow.xaml`**
```xml
<Window x:Class="AlconMusicPlayer.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Alcon Music Player" Height="700" Width="1100"
        Background="{StaticResource BackgroundBrush}">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="60" />    <!-- Top bar -->
            <RowDefinition Height="*" />     <!-- Main area -->
        </Grid.RowDefinitions>

        <!-- Top bar placeholder -->
        <Border Grid.Row="0" Background="{StaticResource SurfaceBrush}">
            <TextBlock Text="♫ Alcon Music Player"
                       Foreground="{StaticResource ForegroundBrush}"
                       VerticalAlignment="Center" Margin="16,0" FontSize="16" />
        </Border>

        <!-- Main area: sidebar + content -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="200" />   <!-- Sidebar -->
                <ColumnDefinition Width="*" />     <!-- Content -->
            </Grid.ColumnDefinitions>

            <!-- Sidebar -->
            <Border Grid.Column="0" Background="{StaticResource SidebarBrush}">
                <StackPanel Margin="0,16">
                    <TextBlock Text="LIBRARY" Foreground="{StaticResource MutedBrush}"
                               FontSize="11" Margin="16,0,0,8" />
                    <Button Content="Songs"    Command="{Binding ShowLibraryCommand}"    Style="{StaticResource SidebarButtonStyle}" />
                    <Button Content="Artists"  Command="{Binding ShowArtistsCommand}"   Style="{StaticResource SidebarButtonStyle}" />
                    <Button Content="Albums"   Command="{Binding ShowAlbumsCommand}"    Style="{StaticResource SidebarButtonStyle}" />

                    <TextBlock Text="PLAYLISTS" Foreground="{StaticResource MutedBrush}"
                               FontSize="11" Margin="16,16,0,8" />
                    <Button Content="All Playlists" Command="{Binding ShowPlaylistsCommand}" Style="{StaticResource SidebarButtonStyle}" />
                </StackPanel>
            </Border>

            <!-- Content area: renders whichever ViewModel is current -->
            <ContentControl Grid.Column="1"
                            Content="{Binding CurrentView}" />
        </Grid>
    </Grid>
</Window>
```

---

### Step 5: MainViewModel

**`ViewModels/MainViewModel.cs`**
```csharp
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly LibraryViewModel  _libraryViewModel;
    private readonly ArtistsViewModel  _artistsViewModel;
    private readonly AlbumsViewModel   _albumsViewModel;
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

        // Start on the library view
        _currentView = _libraryViewModel;

        ShowLibraryCommand   = new RelayCommand(() => CurrentView = _libraryViewModel);
        ShowArtistsCommand   = new RelayCommand(() => CurrentView = _artistsViewModel);
        ShowAlbumsCommand    = new RelayCommand(() => CurrentView = _albumsViewModel);
        ShowPlaylistsCommand = new RelayCommand(() => CurrentView = _playlistsViewModel);
    }
}
```

---

### Step 6: LibraryViewModel + LibraryView

**`ViewModels/LibraryViewModel.cs`**
```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class LibraryViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicService;
    private List<Track> _allTracks = [];

    private ObservableCollection<Track> _tracks = [];
    public ObservableCollection<Track> Tracks
    {
        get => _tracks;
        set => SetProperty(ref _tracks, value);
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            ApplyFilter();
        }
    }

    public ICommand ClearSearchCommand { get; }

    public LibraryViewModel(IMusicLibraryService musicService)
    {
        _musicService = musicService;
        ClearSearchCommand = new RelayCommand(() => SearchText = "");
        LoadTracks();
    }

    private void LoadTracks()
    {
        _allTracks = _musicService.GetAllTracks().ToList();
        Tracks = new ObservableCollection<Track>(_allTracks);
    }

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allTracks
            : _allTracks.Where(t =>
                t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Artist.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Album.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Tracks = new ObservableCollection<Track>(filtered);
    }
}
```

Create folder: `src/AlconMusicPlayer.WPF/Views/`

**`Views/LibraryView.xaml`**
```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.LibraryView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <!-- Search bar -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBox Grid.Column="0"
                     Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                     Background="{StaticResource SurfaceBrush}"
                     Foreground="{StaticResource ForegroundBrush}"
                     Padding="8,6" />
            <Button  Grid.Column="1" Content="Clear"
                     Command="{Binding ClearSearchCommand}"
                     Margin="8,0,0,0" />
        </Grid>

        <!-- Track list -->
        <DataGrid Grid.Row="1"
                  ItemsSource="{Binding Tracks}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  SelectionMode="Single">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Title"    Binding="{Binding Title}"       Width="*" />
                <DataGridTextColumn Header="Artist"   Binding="{Binding Artist.Name}" Width="150" />
                <DataGridTextColumn Header="Album"    Binding="{Binding Album.Name}"  Width="150" />
                <DataGridTextColumn Header="Genre"    Binding="{Binding Genre}"       Width="100" />
                <DataGridTextColumn Header="Duration" Binding="{Binding DurationDisplay}" Width="80" />
                <DataGridTextColumn Header="Plays"    Binding="{Binding PlayCount}"   Width="60" />
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```

**`Views/LibraryView.xaml.cs`** (minimal code-behind — always needed with UserControl)
```csharp
namespace AlconMusicPlayer.WPF.Views;

public partial class LibraryView : System.Windows.Controls.UserControl
{
    public LibraryView() => InitializeComponent();
}
```

---

### Step 7: Remaining ViewModels (Stubs to get it compiling)

Create these as stubs first — proves navigation works, fill in logic after.

**`ViewModels/ArtistsViewModel.cs`**
```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;

namespace AlconMusicPlayer.WPF.ViewModels;

public class ArtistsViewModel : ViewModelBase
{
    public ObservableCollection<Artist> Artists { get; } = [];

    public ArtistsViewModel(IMusicLibraryService musicService)
    {
        foreach (var artist in musicService.GetAllArtists())
            Artists.Add(artist);
    }
}
```

**`ViewModels/AlbumsViewModel.cs`**
```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;

namespace AlconMusicPlayer.WPF.ViewModels;

public class AlbumsViewModel : ViewModelBase
{
    public ObservableCollection<Album> Albums { get; } = [];

    public AlbumsViewModel(IMusicLibraryService musicService)
    {
        foreach (var album in musicService.GetAllAlbums())
            Albums.Add(album);
    }
}
```

**`ViewModels/PlaylistsViewModel.cs`**
```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class PlaylistsViewModel : ViewModelBase
{
    private readonly IPlaylistService _playlistService;
    public ObservableCollection<Playlist> Playlists { get; } = [];

    private string _newPlaylistName = "";
    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set => SetProperty(ref _newPlaylistName, value);
    }

    public ICommand CreatePlaylistCommand { get; }

    public PlaylistsViewModel(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
        CreatePlaylistCommand = new RelayCommand(CreatePlaylist,
            () => !string.IsNullOrWhiteSpace(NewPlaylistName));
        Reload();
    }

    private void CreatePlaylist()
    {
        _playlistService.AddPlaylist(NewPlaylistName);
        NewPlaylistName = "";
        Reload();
    }

    private void Reload()
    {
        Playlists.Clear();
        foreach (var p in _playlistService.GetAllPlaylists())
            Playlists.Add(p);
    }
}
```

**`ViewModels/PlaylistViewModel.cs`**
```csharp
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;

namespace AlconMusicPlayer.WPF.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    private Playlist? _playlist;

    public string PlaylistName => _playlist?.Name ?? "";
    public ObservableCollection<Track> Tracks { get; } = [];

    public void Load(Playlist playlist)
    {
        _playlist = playlist;
        Tracks.Clear();
        foreach (var t in playlist.Tracks)
            Tracks.Add(t);
        OnPropertyChanged(nameof(PlaylistName));
    }
}
```

---

### Step 8: Stub Views

Create these as minimal stubs so the DataTemplates compile:

**`Views/ArtistsView.xaml`**
```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.ArtistsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ListBox ItemsSource="{Binding Artists}"
             DisplayMemberPath="Name"
             Margin="16" />
</UserControl>
```

**`Views/AlbumsView.xaml`**
```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.AlbumsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ListBox ItemsSource="{Binding Albums}"
             DisplayMemberPath="Name"
             Margin="16" />
</UserControl>
```

**`Views/PlaylistsView.xaml`**
```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.PlaylistsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
            <TextBox Text="{Binding NewPlaylistName, UpdateSourceTrigger=PropertyChanged}"
                     Width="200" Padding="8,6" />
            <Button Content="New Playlist" Command="{Binding CreatePlaylistCommand}" Margin="8,0,0,0" />
        </StackPanel>
        <ListBox Grid.Row="1" ItemsSource="{Binding Playlists}" DisplayMemberPath="Name" />
    </Grid>
</UserControl>
```

**`Views/PlaylistView.xaml`**
```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.PlaylistView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        <TextBlock Grid.Row="0" Text="{Binding PlaylistName}"
                   FontSize="20" Margin="0,0,0,12"
                   Foreground="{StaticResource ForegroundBrush}" />
        <DataGrid Grid.Row="1" ItemsSource="{Binding Tracks}"
                  AutoGenerateColumns="False" IsReadOnly="True">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Title"  Binding="{Binding Title}"       Width="*" />
                <DataGridTextColumn Header="Artist" Binding="{Binding Artist.Name}" Width="150" />
                <DataGridTextColumn Header="Album"  Binding="{Binding Album.Name}"  Width="150" />
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```

Each view needs a code-behind file:
```csharp
// Same pattern for all — just change class name and base type
public partial class ArtistsView  : UserControl { public ArtistsView()  => InitializeComponent(); }
public partial class AlbumsView   : UserControl { public AlbumsView()   => InitializeComponent(); }
public partial class PlaylistsView: UserControl { public PlaylistsView()=> InitializeComponent(); }
public partial class PlaylistView : UserControl { public PlaylistView() => InitializeComponent(); }
```

---

### Step 9: Minimal Resource Dictionaries

Create folder: `src/AlconMusicPlayer.WPF/Resources/`

**`Resources/Colors.xaml`** — dark theme palette
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="BackgroundBrush" Color="#1C1C1E" />
    <SolidColorBrush x:Key="SurfaceBrush"    Color="#2C2C2E" />
    <SolidColorBrush x:Key="SidebarBrush"    Color="#242426" />
    <SolidColorBrush x:Key="ForegroundBrush" Color="#F2F2F7" />
    <SolidColorBrush x:Key="MutedBrush"      Color="#8E8E93" />
    <SolidColorBrush x:Key="AccentBrush"     Color="#FF453A" />
    <SolidColorBrush x:Key="HoverBrush"      Color="#3A3A3C" />
    <SolidColorBrush x:Key="SelectedBrush"   Color="#FF453A" />
</ResourceDictionary>
```

**`Resources/Styles.xaml`** — minimal control styles
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Sidebar button -->
    <Style x:Key="SidebarButtonStyle" TargetType="Button">
        <Setter Property="Background"   Value="Transparent" />
        <Setter Property="Foreground"   Value="{StaticResource ForegroundBrush}" />
        <Setter Property="BorderThickness" Value="0" />
        <Setter Property="HorizontalContentAlignment" Value="Left" />
        <Setter Property="Padding"      Value="16,10" />
        <Setter Property="Cursor"       Value="Hand" />
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="{StaticResource HoverBrush}" />
            </Trigger>
        </Style.Triggers>
    </Style>

    <!-- Default button -->
    <Style TargetType="Button">
        <Setter Property="Background"      Value="{StaticResource AccentBrush}" />
        <Setter Property="Foreground"      Value="{StaticResource ForegroundBrush}" />
        <Setter Property="BorderThickness" Value="0" />
        <Setter Property="Padding"         Value="12,6" />
        <Setter Property="Cursor"          Value="Hand" />
    </Style>

    <!-- TextBox -->
    <Style TargetType="TextBox">
        <Setter Property="Background"      Value="{StaticResource SurfaceBrush}" />
        <Setter Property="Foreground"      Value="{StaticResource ForegroundBrush}" />
        <Setter Property="BorderBrush"     Value="{StaticResource HoverBrush}" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="Padding"         Value="8,6" />
    </Style>

    <!-- DataGrid -->
    <Style TargetType="DataGrid">
        <Setter Property="Background"            Value="{StaticResource BackgroundBrush}" />
        <Setter Property="Foreground"            Value="{StaticResource ForegroundBrush}" />
        <Setter Property="BorderThickness"       Value="0" />
        <Setter Property="RowBackground"         Value="{StaticResource BackgroundBrush}" />
        <Setter Property="AlternatingRowBackground" Value="{StaticResource SurfaceBrush}" />
        <Setter Property="GridLinesVisibility"   Value="None" />
        <Setter Property="HeadersVisibility"     Value="Column" />
    </Style>

    <!-- DataGrid column header -->
    <Style TargetType="DataGridColumnHeader">
        <Setter Property="Background"  Value="{StaticResource SurfaceBrush}" />
        <Setter Property="Foreground"  Value="{StaticResource MutedBrush}" />
        <Setter Property="Padding"     Value="8,6" />
        <Setter Property="BorderThickness" Value="0" />
    </Style>

    <!-- ListBox -->
    <Style TargetType="ListBox">
        <Setter Property="Background"      Value="{StaticResource BackgroundBrush}" />
        <Setter Property="Foreground"      Value="{StaticResource ForegroundBrush}" />
        <Setter Property="BorderThickness" Value="0" />
    </Style>
</ResourceDictionary>
```

---

## Part 4 — Checklist

```
Step 1  ViewModelBase + RelayCommand          🔲
Step 2  App.xaml.cs DI composition root       🔲
Step 3  App.xaml — remove StartupUri          🔲
Step 3  App.xaml — Resources + DataTemplates  🔲
Step 4  MainWindow.xaml shell layout          🔲
Step 5  MainViewModel                         🔲
Step 6  LibraryViewModel                      🔲
Step 6  LibraryView.xaml + .cs                🔲
Step 7  ArtistsViewModel                      🔲
Step 7  AlbumsViewModel                       🔲
Step 7  PlaylistsViewModel                    🔲
Step 7  PlaylistViewModel                     🔲
Step 8  ArtistsView.xaml + .cs                🔲
Step 8  AlbumsView.xaml + .cs                 🔲
Step 8  PlaylistsView.xaml + .cs              🔲
Step 8  PlaylistView.xaml + .cs               🔲
Step 9  Resources/Colors.xaml                 🔲
Step 9  Resources/Styles.xaml                 🔲
        dotnet build — verify 0 errors        🔲
        dotnet run — verify app opens         🔲
```

---

## Part 5 — Common WPF Gotchas

| Gotcha | What happens | Fix |
|---|---|---|
| `StartupUri` still in App.xaml | Window opens twice or without DataContext | Remove `StartupUri`, use `OnStartup` |
| Missing code-behind `.cs` for a View | Build error: partial class not found | Every `.xaml` needs a `.xaml.cs` |
| Binding path typo | Silently shows nothing, no exception | Check Output window for binding errors |
| Property doesn't call `SetProperty` | UI never updates | Ensure setter calls `SetProperty` not just `= value` |
| `ObservableCollection` replaced, not mutated | UI updates work | Replacing the collection fires `PropertyChanged` — ✅ this approach is fine |
| DataTemplate DataType wrong namespace | View never renders in ContentControl | Check `xmlns:vm` points to the correct namespace |
