# WPF Project Recreation Guide — AlconMusicPlayer.WPF

This document contains **every file, every line, and every concept** needed to recreate the `AlconMusicPlayer.WPF` project from scratch. Follow the steps in order.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Folder Structure](#2-folder-structure)
3. [Project File (.csproj)](#3-project-file-csproj)
4. [Key WPF Patterns Used](#4-key-wpf-patterns-used)
5. [Step 1 — Base Infrastructure](#5-step-1--base-infrastructure)
6. [Step 2 — Converters](#6-step-2--converters)
7. [Step 3 — Resources (Themes + Styles)](#7-step-3--resources-themes--styles)
8. [Step 4 — App.xaml (Root Resources + DataTemplates)](#8-step-4--appxaml-root-resources--datatemplates)
9. [Step 5 — App.xaml.cs (Dependency Injection)](#9-step-5--appxamlcs-dependency-injection)
10. [Step 6 — MainWindow](#10-step-6--mainwindow)
11. [Step 7 — ViewModels](#11-step-7--viewmodels)
12. [Step 8 — Views (XAML + Code-Behind)](#12-step-8--views-xaml--code-behind)
13. [Step 9 — Dialogs](#13-step-9--dialogs)
14. [Step 10 — Services](#14-step-10--services)
15. [How Everything Connects](#15-how-everything-connects)

---

## 1. Project Overview

The WPF layer is the **presentation tier** of a Clean Architecture music player. It depends on:
- `AlconMusicPlayer.Domain` — entities (`Track`, `Artist`, `Album`, `Playlist`, `Genre`)
- `AlconMusicPlayer.Infra` — repositories and services
- `AlconMusicPlayer.ApplicationService` — use case classes and service interfaces

The WPF project itself contains **no business logic** — only UI, ViewModels, and DI wiring.

---

## 2. Folder Structure

```
AlconMusicPlayer.WPF/
├── AlconMusicPlayer.WPF.csproj
├── App.xaml               ← root resources, DataTemplate map, converters
├── App.xaml.cs            ← DI container setup, startup
├── AssemblyInfo.cs
├── MainWindow.xaml        ← shell: title bar, now-playing strip, sidebar, content area
├── MainWindow.xaml.cs     ← minimal code-behind (InitializeComponent only)
│
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   └── StringToVisibilityConverter.cs
│
├── Dialogs/
│   ├── NewPlaylistDialog.xaml
│   └── NewPlaylistDialog.xaml.cs
│
├── Resources/
│   ├── Colors.xaml        ← legacy (superseded by Themes/)
│   ├── Styles.xaml        ← global control styles
│   └── Themes/
│       ├── LightTheme.xaml
│       └── DarkTheme.xaml
│
├── Services/
│   └── ThemeService.cs
│
├── ViewModels/
│   ├── Base/
│   │   ├── ViewModelBase.cs
│   │   └── RelayCommand.cs
│   ├── MainViewModel.cs
│   ├── NowPlayingViewModel.cs
│   ├── LibraryViewModel.cs
│   ├── ArtistsViewModel.cs
│   ├── AlbumsViewModel.cs
│   ├── PlaylistsViewModel.cs
│   ├── PlaylistViewModel.cs
│   └── SettingsViewModel.cs
│
└── Views/
    ├── NowPlayingView.xaml / .cs
    ├── LibraryView.xaml / .cs
    ├── ArtistsView.xaml / .cs
    ├── AlbumsView.xaml / .cs
    ├── PlaylistsView.xaml / .cs
    ├── PlaylistView.xaml / .cs
    └── SettingsView.xaml / .cs
```

---

## 3. Project File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.5" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\AlconMusicPlayer.Domain\AlconMusicPlayer.Domain.csproj" />
    <ProjectReference Include="..\AlconMusicPlayer.Infra\AlconMusicPlayer.Infra.csproj" />
  </ItemGroup>

</Project>
```

**Key settings:**
- `OutputType=WinExe` — desktop executable (no console window)
- `UseWPF=true` — enables XAML compilation and WPF assemblies
- Only one NuGet package: `Microsoft.Extensions.DependencyInjection` for DI

---

## 4. Key WPF Patterns Used

| Pattern | Where used | Why |
|---|---|---|
| **MVVM** | All ViewModels + Views | Separates UI logic from UI markup |
| **INotifyPropertyChanged** | `ViewModelBase` | Notifies WPF bindings when values change |
| **ICommand / RelayCommand** | All ViewModels | Binds button clicks to ViewModel methods |
| **DataTemplate auto-wiring** | `App.xaml` | Maps ViewModel type → View automatically |
| **ContentControl + CurrentView** | `MainWindow.xaml` | Single-page navigation via ViewModel swap |
| **DynamicResource** | All XAML | Allows runtime theme switching |
| **ObservableCollection\<T\>** | List ViewModels | Auto-notifies ListBox/DataGrid of changes |
| **Microsoft.Extensions.DI** | `App.xaml.cs` | Constructor injection for all ViewModels |
| **PropertyChanged event listener** | `MainViewModel` | Cross-ViewModel communication |
| **ContextMenu + Tag trick** | `LibraryView`, `PlaylistView` | ContextMenu is outside the visual tree |

---

## 5. Step 1 — Base Infrastructure

### `ViewModels/Base/ViewModelBase.cs`

Every ViewModel inherits from this. It provides `INotifyPropertyChanged` and two helpers.

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlconMusicPlayer.WPF.ViewModels.Base
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Called from a property setter.
        // Sets the backing field and fires PropertyChanged only when value actually changed.
        // [CallerMemberName] fills in the property name automatically.
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Use when you need to notify without a backing field (e.g., computed properties).
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

**Why `protected`?** Only subclasses should call these internal notification methods. The public surface is only `PropertyChanged` event + the bound properties.
What is the issue if we set it to public? It would allow external code to call `SetProperty` or `OnPropertyChanged` on any ViewModel, which could lead to bugs if misused. By keeping them protected, we enforce that only the ViewModel itself can trigger property change notifications, maintaining better encapsulation and control over the ViewModel's state.
Give an example of how it could be misused if it were public? If `SetProperty` were public, external code could do something like this:

```csharp
var mainViewModel = new MainViewModel();
// External code directly modifies the ViewModel's internal state, which is not intended
mainViewModel.SetProperty(ref mainViewModel.SomeInternalField, newValue);
```
This would bypass any validation or logic that should be in the property setter, and it could lead to inconsistent state or unexpected behavior in the UI. By keeping `SetProperty` protected, we prevent this kind of misuse and ensure that all state changes go through the proper channels defined by the ViewModel's properties.

---

### `ViewModels/Base/RelayCommand.cs`

Two classes:
- `RelayCommand` — for commands with no parameter
- `RelayCommand<T>` — for commands that receive a typed parameter (e.g., `RelayCommand<Track>`)

```csharp
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels.Base
{
    // Parameterless command
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Hook into WPF's CommandManager so CanExecute re-evaluates automatically
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }

    // Typed parameter command
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
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
    }
}
```

**Key insight:** `CommandManager.RequerySuggested` is a WPF global event that fires whenever WPF thinks UI state may have changed (mouse click, focus change, etc.). By hooking `CanExecuteChanged` into it, buttons automatically enable/disable without manual calls. When you *do* need to force a recheck, call `CommandManager.InvalidateRequerySuggested()`.

---

## 6. Step 2 — Converters

### `Converters/BoolToVisibilityConverter.cs`

Converts `bool` → `Visibility`. Supports an `"invert"` parameter to flip the logic (show when false).

```csharp
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlconMusicPlayer.WPF.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = (bool?)value ?? false;
        bool invert = parameter?.ToString() == "invert";
        bool shouldShow = invert ? !boolValue : boolValue;
        return shouldShow ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
```

**Usage in XAML:**
```xml
<!-- Show ▶ Play button only when NOT playing -->
<Button Content="▶"
        Visibility="{Binding IsPlaying,
                     Converter={StaticResource BoolToVisibilityConverter},
                     ConverterParameter=invert}" />

<!-- Show ⏸ Pause button only when playing -->
<Button Content="⏸"
        Visibility="{Binding IsPlaying,
                     Converter={StaticResource BoolToVisibilityConverter}}" />
```

What is a static resource? A static resource is evaluated once at load time. If the underlying value changes later, the UI does NOT update. Use a static resource for things that won't change (e.g., a converter instance).
Is static resource can be called as singleton? Yes, using a static resource for converters is a common pattern because converters are typically stateless and can be shared across the entire application. By defining the converter as a static resource in `App.xaml`, you ensure that only one instance is created and reused wherever it's referenced in XAML, which is memory efficient.
What is a dynamic resource? A dynamic resource is evaluated every time the UI needs to update. If the underlying value changes, the UI updates automatically. Use a dynamic resource for things that can change at runtime (e.g., theme brushes).


---

### `Converters/StringToVisibilityConverter.cs`

Converts `string` → `Visibility`. Collapsed when null or empty; Visible when the string has content.

```csharp
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlconMusicPlayer.WPF.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
```

**Usage in XAML:**
```xml
<!-- Error bar only appears when ErrorMessage is non-empty -->
<TextBlock Text="{Binding ErrorMessage}"
           Visibility="{Binding ErrorMessage,
                        Converter={StaticResource StringToVisibilityConverter}}" />
```

---

## 7. Step 3 — Resources (Themes + Styles)

### `Resources/Themes/LightTheme.xaml`

All brushes use `po:Freeze="False"` so they can be swapped at runtime for theme switching.

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:po="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options">
    <!-- Background colors -->
    <SolidColorBrush po:Freeze="False" x:Key="BackgroundBrush"    Color="#FFFFFF" />
    <SolidColorBrush po:Freeze="False" x:Key="RowAlternateBrush"  Color="#F5F5F5" />
    <SolidColorBrush po:Freeze="False" x:Key="SidebarBrush"       Color="#E6E1DC" />
    <SolidColorBrush po:Freeze="False" x:Key="ToolbarBrush"       Color="#F2F2F7" />

    <!-- Text colors -->
    <SolidColorBrush po:Freeze="False" x:Key="ForegroundBrush"    Color="#1C1C1E" />
    <SolidColorBrush po:Freeze="False" x:Key="SecondaryTextBrush" Color="#6E6E73" />
    <SolidColorBrush po:Freeze="False" x:Key="MutedBrush"         Color="#A1A1A6" />

    <!-- Icon colors -->
    <SolidColorBrush po:Freeze="False" x:Key="IconInactiveBrush"  Color="#8E8E93" />
    <SolidColorBrush po:Freeze="False" x:Key="IconActiveBrush"    Color="#1C1C1E" />

    <!-- Dividers and borders -->
    <SolidColorBrush po:Freeze="False" x:Key="DividerBrush"       Color="#E5E5EA" />
    <SolidColorBrush po:Freeze="False" x:Key="HeaderBorderBrush"  Color="#D1D1D6" />

    <!-- Selection and interaction -->
    <SolidColorBrush po:Freeze="False" x:Key="SelectionBrush"     Color="#D61F26" />
    <SolidColorBrush po:Freeze="False" x:Key="SelectionTextBrush" Color="#FFFFFF" />
    <SolidColorBrush po:Freeze="False" x:Key="SidebarSelectionBrush" Color="#D1D1D6" />

    <!-- Surface and accent -->
    <SolidColorBrush po:Freeze="False" x:Key="SurfaceBrush"       Color="#F2F2F7" />
    <SolidColorBrush po:Freeze="False" x:Key="AccentBrush"        Color="#D61F26" />
    <SolidColorBrush po:Freeze="False" x:Key="HoverBrush"         Color="#E8E8E8" />
    <SolidColorBrush po:Freeze="False" x:Key="SelectedBrush"      Color="#D61F26" />
    <SolidColorBrush po:Freeze="False" x:Key="PrimaryActionBrush" Color="#0A84FF" />
</ResourceDictionary>
```

What is po:Freeze? By default, WPF freezes (makes immutable) brushes defined in XAML for performance. Setting `po:Freeze="False"` allows us to modify or replace these brushes at runtime, which is essential for theme switching. In this app, we swap entire ResourceDictionaries to switch themes, so we need the brushes to be unfrozen. If you don't plan to support runtime theme switching, you can omit `po:Freeze` and keep the brushes frozen for better performance.

what is the full form of po? po stands for "presentation options". It's an XML namespace prefix used in WPF XAML to access features related to presentation options, such as freezing resources. The full namespace URI is "http://schemas.microsoft.com/winfx/2006/xaml/presentation/options". By declaring this namespace in your XAML, you can use the `po:Freeze` attribute to control whether a resource is frozen (immutable) or not, which is important for scenarios like dynamic theming where you need to modify resources at runtime.

---

### `Resources/Themes/DarkTheme.xaml`

Same keys, different colors — no `po:Freeze` (not used at runtime yet).

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <SolidColorBrush x:Key="BackgroundBrush"    Color="#1C1C1E" />
    <SolidColorBrush x:Key="RowAlternateBrush"  Color="#2C2C2E" />
    <SolidColorBrush x:Key="SurfaceBrush"       Color="#2C2C2E" />
    <SolidColorBrush x:Key="SidebarBrush"       Color="#242426" />
    <SolidColorBrush x:Key="ForegroundBrush"    Color="#F2F2F7" />
    <SolidColorBrush x:Key="SecondaryTextBrush" Color="#A1A1A6" />
    <SolidColorBrush x:Key="MutedBrush"         Color="#8E8E93" />
    <SolidColorBrush x:Key="AccentBrush"        Color="#FF453A" />
    <SolidColorBrush x:Key="HoverBrush"         Color="#3A3A3C" />
    <SolidColorBrush x:Key="SelectedBrush"      Color="#FF453A" />
    <SolidColorBrush x:Key="PrimaryActionBrush" Color="#0A84FF" />
    <SolidColorBrush x:Key="DividerBrush"       Color="#3A3A3C" />
    <SolidColorBrush x:Key="HeaderBorderBrush"  Color="#3A3A3C" />
    <SolidColorBrush x:Key="ToolbarBrush"       Color="#2C2C2E" />
    <SolidColorBrush x:Key="IconInactiveBrush"  Color="#8E8E93" />
    <SolidColorBrush x:Key="IconActiveBrush"    Color="#F2F2F7" />
    <SolidColorBrush x:Key="SelectionBrush"     Color="#FF453A" />
    <SolidColorBrush x:Key="SelectionTextBrush" Color="#FFFFFF" />
    <SolidColorBrush x:Key="SidebarSelectionBrush" Color="#3A3A3C" />
</ResourceDictionary>
```

---

### `Resources/Styles.xaml`

All custom styles for the reusable controls. Uses `DynamicResource` everywhere so swapping a theme ResourceDictionary updates the UI live.

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Sidebar button: left-aligned, transparent background, hover highlight -->
    <Style x:Key="SidebarButtonStyle" TargetType="Button">
        <Setter Property="Background"              Value="Transparent" />
        <Setter Property="Foreground"              Value="{DynamicResource ForegroundBrush}" />
        <Setter Property="BorderThickness"         Value="0" />
        <Setter Property="HorizontalContentAlignment" Value="Left" />
        <Setter Property="Padding"                 Value="16,10" />
        <Setter Property="Cursor"                  Value="Hand" />
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="{DynamicResource HoverBrush}" />
            </Trigger>
        </Style.Triggers>
    </Style>

    <!-- Default Button style (applies to ALL buttons unless overridden) -->
    <Style TargetType="Button">
        <Setter Property="Background"      Value="{DynamicResource AccentBrush}" />
        <Setter Property="Foreground"      Value="{DynamicResource ForegroundBrush}" />
        <Setter Property="BorderThickness" Value="0" />
        <Setter Property="Padding"         Value="12,6" />
        <Setter Property="Cursor"          Value="Hand" />
    </Style>

    <!-- Default TextBox style -->
    <Style TargetType="TextBox">
        <Setter Property="Background"      Value="{DynamicResource SurfaceBrush}" />
        <Setter Property="Foreground"      Value="{DynamicResource ForegroundBrush}" />
        <Setter Property="BorderBrush"     Value="{DynamicResource HoverBrush}" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="Padding"         Value="8,6" />
    </Style>

    <!-- Default DataGrid style -->
    <Style TargetType="DataGrid">
        <Setter Property="Background"              Value="{DynamicResource BackgroundBrush}" />
        <Setter Property="Foreground"              Value="{DynamicResource ForegroundBrush}" />
        <Setter Property="BorderThickness"         Value="0" />
        <Setter Property="RowBackground"           Value="{DynamicResource BackgroundBrush}" />
        <Setter Property="AlternatingRowBackground" Value="{DynamicResource SurfaceBrush}" />
        <Setter Property="GridLinesVisibility"     Value="None" />
        <Setter Property="HeadersVisibility"       Value="Column" />
    </Style>

    <!-- DataGrid column header -->
    <Style TargetType="DataGridColumnHeader">
        <Setter Property="Background"      Value="{DynamicResource SurfaceBrush}" />
        <Setter Property="Foreground"      Value="{DynamicResource MutedBrush}" />
        <Setter Property="Padding"         Value="8,6" />
        <Setter Property="BorderThickness" Value="0" />
    </Style>

    <!-- Default ListBox style -->
    <Style TargetType="ListBox">
        <Setter Property="Background"      Value="{DynamicResource BackgroundBrush}" />
        <Setter Property="Foreground"      Value="{DynamicResource ForegroundBrush}" />
        <Setter Property="BorderThickness" Value="0" />
    </Style>
</ResourceDictionary>
```

---

## 8. Step 4 — App.xaml (Root Resources + DataTemplates)

This is the most important file for understanding how navigation works.

```xml
<Application x:Class="AlconMusicPlayer.WPF.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:AlconMusicPlayer.WPF.ViewModels"
             xmlns:views="clr-namespace:AlconMusicPlayer.WPF.Views"
             xmlns:converters="clr-namespace:AlconMusicPlayer.WPF.Converters">

    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Theme is loaded first so Styles.xaml can reference theme brushes -->
                <ResourceDictionary Source="Resources/Themes/LightTheme.xaml" />
                <ResourceDictionary Source="Resources/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>

            <!-- Converters are registered as application-wide resources -->
            <converters:StringToVisibilityConverter x:Key="StringToVisibilityConverter" />
            <converters:BoolToVisibilityConverter   x:Key="BoolToVisibilityConverter" />

            <!--
                DataTemplate auto-wiring:
                When a ContentControl's Content is set to a ViewModel instance,
                WPF looks up the DataTemplate for that type and renders the matching View.
                No navigation service needed — just set CurrentView = somViewModel.
            -->
            <DataTemplate DataType="{x:Type vm:NowPlayingViewModel}">
                <views:NowPlayingView />
            </DataTemplate>
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
            <DataTemplate DataType="{x:Type vm:SettingsViewModel}">
                <views:SettingsView />
            </DataTemplate>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**IMPORTANT:** There is NO `StartupUri` attribute. App startup is handled entirely in `App.xaml.cs` → `OnStartup()`. Remove `StartupUri` from the opening `<Application>` tag if Visual Studio generates one.

---

## 9. Step 5 — App.xaml.cs (Dependency Injection)

```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Interfaces;
using AlconMusicPlayer.Infra.Data;
using AlconMusicPlayer.Infra.Repositories;
using AlconMusicPlayer.Infra.Services;
using AlconMusicPlayer.WPF.Services;
using AlconMusicPlayer.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AlconMusicPlayer.WPF;

public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        RegisterService(services);
        _serviceProvider = services.BuildServiceProvider();

        // Manually resolve the window and set its DataContext
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    private static void RegisterService(IServiceCollection services)
    {
        // Load seed data (returns IEnumerable<Track>)
        var tracks = SeedData.Build();

        // --- Repositories ---
        services.AddSingleton<ITrackRepository>(new InMemoryTrackRepository(tracks.ToList()));
        services.AddSingleton<IPlaylistRepository, InMemoryPlaylistRepository>();

        // --- Domain Services ---
        services.AddSingleton<IMusicLibraryService, MusicLibraryService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();

        // --- WPF Services ---
        services.AddSingleton<IThemeService, ThemeService>();

        // --- Use Cases (stateless, so Transient is correct) ---
        services.AddTransient<CreatePlaylistUseCase>();
        services.AddTransient<AddTrackToPlaylistUseCase>();
        services.AddTransient<RemoveTrackFromPlaylistUseCase>();

        // --- ViewModels ---
        // Singleton: shared state (NowPlaying, Main)
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<NowPlayingViewModel>();
        // Note: MainViewModel and NowPlayingViewModel hold shared state that should persist across the app lifetime, so we register them as singletons. This ensures that when different Views or other ViewModels depend on MainViewModel or NowPlayingViewModel, they all get the same instance with the same state.
        //give an example here: For example, if both the MainWindow and a NowPlayingView need to access the NowPlayingViewModel, registering it as a singleton ensures that they both reference the same instance. This way, if the NowPlayingViewModel updates its state (e.g., changes the currently playing track), those changes will be reflected in all Views that depend on it, since they are all referencing the same singleton instance.
        //Updating nowplaying to transient, selected track is not updated in the nowplaying view, because mainviewmodel and nowplayingviewmodel are different instances, so they don't share state. This is why NowPlayingViewModel should be a singleton — it holds the shared state of what's currently playing, and we want all Views that depend on it to see the same state.
        //updating mainviewmodel to transient causes the same issue — the sidebar navigation buttons stop working because they update MainViewModel.CurrentView, but the MainWindow is bound to a different instance of MainViewModel that never gets updated. This is another reason why MainViewModel should be a singleton — it holds the shared state of which View is currently active, and we want all parts of the app to reference the same MainViewModel instance so that navigation and state updates work correctly.


        // why they are not an issue for the other viewmodels? The other ViewModels (LibraryViewModel, ArtistsViewModel, AlbumsViewModel, PlaylistsViewModel, PlaylistViewModel, SettingsViewModel) are registered as transient because they are typically used as the Content of a single View at a time and do not need to share state across the entire application. Each time you navigate to a different section of the app (e.g., from Library to Artists), you want a fresh instance of that ViewModel to be created with its own state. Since these ViewModels don't hold shared state that needs to persist across the app, using transient is appropriate for them.

        //what happens if we make them singleton? If we made all the ViewModels singletons, then they would all share the same instance throughout the application's lifetime. This could lead to unintended consequences, such as stale data or state conflicts. For example, if you navigate to the LibraryView and it updates its state (e.g., selected track), that state would persist even when you navigate away and back to the LibraryView, which might not be the desired behavior. Additionally, if multiple Views depend on the same singleton ViewModel, they could interfere with each other's state, leading to bugs and a confusing user experience. By using transient for these ViewModels, we ensure that each time you navigate to a section of the app, you get a fresh instance with its own independent state.
        //Give more examples to reproduce and see the issue with singleton: For instance, if you have a PlaylistViewModel that is registered as a singleton, and you navigate to a specific playlist, the PlaylistViewModel would load that playlist's data. If you then navigate to a different playlist, the same PlaylistViewModel instance would be reused, and it would still hold the data of the first playlist. This means that when you switch between playlists, you would see the wrong data because the state is shared across all instances of that ViewModel. By using transient, each time you navigate to a playlist, a new instance of PlaylistViewModel is created, ensuring that it loads the correct data for the selected playlist without any interference from previous state.
        //more ways to repoduc

        
        // Transient: each resolution gets a fresh instance
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<ArtistsViewModel>();
        services.AddTransient<AlbumsViewModel>();
        services.AddTransient<PlaylistsViewModel>();
        services.AddTransient<PlaylistViewModel>();
        services.AddTransient<SettingsViewModel>();

        // --- Window ---
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
```

why do we need transient for MainWindow? We use `AddTransient<MainWindow>()` because we want a new instance of `MainWindow` to be created each time it's requested. In this application, we only create one `MainWindow` at startup, so it doesn't strictly need to be transient. However, using transient here is a common practice for WPF windows because it allows for more flexibility in case you want to create multiple windows in the future (e.g., dialogs, secondary windows). If you were to use `AddSingleton<MainWindow>()`, the same instance would be reused throughout the application's lifetime, which could lead to issues if you ever needed to create another window or if the window's state needs to be reset.

**Lifetime rules:**
| Registration | When to use |
|---|---|
| `AddSingleton` | Shared state — same instance for entire app lifetime |
| `AddTransient` | Stateless or consumed once — new instance every time |
| `AddScoped` | Not used here (scoped to a request, relevant in web) |

---

## 10. Step 6 — MainWindow

### `MainWindow.xaml`

The shell is a 3-row Grid:

```
Row 0 (48px)  → App title bar
Row 1 (60px)  → Now Playing strip  |  Search box
Row 2 (*)     → Sidebar (200px)    |  Content area (*)
```

```xml
<Window x:Class="AlconMusicPlayer.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Alcon Music Player" Height="700" Width="1100"
        Background="{DynamicResource BackgroundBrush}">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="48" />   <!-- Title -->
            <RowDefinition Height="60" />   <!-- Now Playing + Search -->
            <RowDefinition Height="*"  />   <!-- Main area -->
        </Grid.RowDefinitions>

        <!-- ── Row 0: App title ───────────────────────────────── -->
        <Border Grid.Row="0" Background="{DynamicResource BackgroundBrush}">
            <TextBlock Text="♫ Alcon Music Player"
                       Foreground="{DynamicResource ForegroundBrush}"
                       VerticalAlignment="Center" Margin="16,0" FontSize="16" />
        </Border>

        <!-- ── Row 1: Now Playing + Search ───────────────────── -->
        <Grid Grid.Row="1" Background="{DynamicResource BackgroundBrush}">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"   />  <!-- Now Playing -->
                <ColumnDefinition Width="320" />  <!-- Search -->
            </Grid.ColumnDefinitions>

            <!-- Now Playing panel (left) -->
            <Border Grid.Column="0"
                    Background="{DynamicResource SurfaceBrush}"
                    Margin="16,6,8,6" CornerRadius="8">
                <!-- ContentControl renders NowPlayingView automatically via DataTemplate -->
                <ContentControl Content="{Binding NowPlayingViewModel}" />
            </Border>

            <!-- Search panel (right) -->
            <Border Grid.Column="1"
                    Background="{DynamicResource SurfaceBrush}"
                    Margin="8,6,16,6" CornerRadius="8" Padding="12,8">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />  <!-- search icon -->
                        <ColumnDefinition Width="*"    />  <!-- text box -->
                        <ColumnDefinition Width="Auto" />  <!-- clear button -->
                    </Grid.ColumnDefinitions>
                    <!-- Auto and * sizing to make the search box expand to fill available space 
                    explain: The first column is set to "Auto" which means it will take up only as much space as needed for the search icon. The second column is set to "*" which means it will take up all the remaining space, allowing the TextBox to expand and fill the available area. The third column is also set to "Auto" for the clear button, so it will only take up as much space as needed. This layout ensures that the search box is flexible and can grow or shrink based on the window size, while the search icon and clear button remain appropriately sized.
                    
                    -->

                    <!-- Search icon (Segoe MDL2 Assets unicode) -->
                    <TextBlock Grid.Column="0"
                               Text="&#xE721;"
                               FontFamily="Segoe MDL2 Assets"
                               Foreground="{DynamicResource MutedBrush}"
                               VerticalAlignment="Center"
                               Margin="0,0,8,0" />

                    <!-- Search text — binds to MainViewModel.SearchText which delegates to LibraryViewModel -->
                    <TextBox Grid.Column="1"
                             Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                             VerticalAlignment="Center"
                             BorderThickness="0"
                             Padding="0" />
                    <Button Grid.Column="2"
                            Content="Clear"
                            Command="{Binding ClearSearchCommand}"
                            Margin="8,0,0,0"
                            VerticalAlignment="Center"
                            Background="Transparent"
                            Foreground="{DynamicResource MutedBrush}" />
                </Grid>
            </Border>
        </Grid>

        <!-- ── Row 2: Sidebar + Content ──────────────────────── -->
        <Grid Grid.Row="2">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="200" />  <!-- Sidebar -->
                <ColumnDefinition Width="*"   />  <!-- Content -->
            </Grid.ColumnDefinitions>

            <!-- Sidebar navigation -->
            <Border Grid.Column="0" Background="{DynamicResource SidebarBrush}">
                <StackPanel Margin="0,16">
                    <TextBlock Text="LIBRARY"
                               Foreground="{DynamicResource MutedBrush}"
                               FontSize="11" Margin="16,0,0,8" />
                    <Button Content="Songs"   Command="{Binding ShowLibraryCommand}"  Style="{StaticResource SidebarButtonStyle}" />
                    <Button Content="Artists" Command="{Binding ShowArtistsCommand}"  Style="{StaticResource SidebarButtonStyle}" />
                    <Button Content="Albums"  Command="{Binding ShowAlbumsCommand}"   Style="{StaticResource SidebarButtonStyle}" />

                    <TextBlock Text="PLAYLISTS"
                               Foreground="{DynamicResource MutedBrush}"
                               FontSize="11" Margin="16,16,0,8" />
                    <Button Content="All Playlists" Command="{Binding ShowPlaylistsCommand}" Style="{StaticResource SidebarButtonStyle}" />

                    <TextBlock Text="SETTINGS"
                               Foreground="{DynamicResource MutedBrush}"
                               FontSize="11" Margin="16,16,0,8" />
                    <Button Content="Theme" Command="{Binding ShowSettingsCommand}" Style="{StaticResource SidebarButtonStyle}" />
                </StackPanel>
            </Border>

            <!--
                THE NAVIGATION CENTER:
                Content is bound to MainViewModel.CurrentView.
                When CurrentView changes to a ViewModel instance,
                WPF's DataTemplate lookup renders the matching View automatically.
            -->
            <ContentControl Grid.Column="1"
                            Content="{Binding CurrentView}" />
        </Grid>
    </Grid>
</Window>
```

### `MainWindow.xaml.cs`

Minimal — DI handles everything.

```csharp
namespace AlconMusicPlayer.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

---

## 11. Step 7 — ViewModels

### `ViewModels/MainViewModel.cs`

The hub ViewModel. Owns navigation (`CurrentView`), delegates search to `LibraryViewModel`, and listens to child ViewModels via `PropertyChanged` events.

```csharp
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.ComponentModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    // All child ViewModels injected via constructor (DI resolves them)
    private readonly LibraryViewModel    _libraryViewModel;
    private readonly ArtistsViewModel    _artistsViewModel;
    private readonly AlbumsViewModel     _albumsViewModel;
    private readonly PlaylistsViewModel  _playlistsViewModel;
    private readonly PlaylistViewModel   _playlistViewModel;
    private readonly NowPlayingViewModel _nowPlayingViewModel;
    private readonly SettingsViewModel   _settingsViewModel;

    // Exposed to MainWindow for the Now Playing strip
    public NowPlayingViewModel NowPlayingViewModel => _nowPlayingViewModel;

    // Search is visually in MainWindow but logically lives in LibraryViewModel
    public string SearchText
    {
        get => _libraryViewModel.SearchText;
        set { _libraryViewModel.SearchText = value; OnPropertyChanged(); }
    }
    public ICommand ClearSearchCommand => _libraryViewModel.ClearSearchCommand;

    // Navigation: changing this property replaces the visible view
    private ViewModelBase _currentView;
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    // Sidebar commands
    public ICommand ShowLibraryCommand   { get; }
    public ICommand ShowArtistsCommand   { get; }
    public ICommand ShowAlbumsCommand    { get; }
    public ICommand ShowPlaylistsCommand { get; }
    public ICommand ShowSettingsCommand  { get; }

    public MainViewModel(
        LibraryViewModel    libraryViewModel,
        ArtistsViewModel    artistsViewModel,
        AlbumsViewModel     albumsViewModel,
        PlaylistsViewModel  playlistsViewModel,
        PlaylistViewModel   playlistViewModel,
        NowPlayingViewModel nowPlayingViewModel,
        SettingsViewModel   settingsViewModel)
    {
        _libraryViewModel   = libraryViewModel;
        _artistsViewModel   = artistsViewModel;
        _albumsViewModel    = albumsViewModel;
        _playlistsViewModel = playlistsViewModel;
        _playlistViewModel  = playlistViewModel;
        _nowPlayingViewModel = nowPlayingViewModel;
        _settingsViewModel  = settingsViewModel;

        _currentView = _libraryViewModel;   // Default landing page

        // Sidebar commands — swap CurrentView to navigate
        ShowLibraryCommand  = new RelayCommand(() => CurrentView = _libraryViewModel);
        ShowArtistsCommand  = new RelayCommand(() => CurrentView = _artistsViewModel);
        ShowAlbumsCommand   = new RelayCommand(() => CurrentView = _albumsViewModel);
        ShowSettingsCommand = new RelayCommand(() => CurrentView = _settingsViewModel);
        ShowPlaylistsCommand = new RelayCommand(() =>
        {
            _playlistsViewModel.Reload();          // refresh list before showing
            _playlistsViewModel.SelectedPlaylist = null;
            CurrentView = _playlistsViewModel;
        });

        // Cross-ViewModel communication via PropertyChanged
        _artistsViewModel.PropertyChanged   += OnArtistSelectionChanged;
        _albumsViewModel.PropertyChanged    += OnAlbumSelectionChanged;
        _playlistsViewModel.PropertyChanged += OnPlaylistSelectionChanged;
        _libraryViewModel.PropertyChanged   += OnLibraryPropertyChanged;
        _playlistViewModel.BackRequested    += OnPlaylistBackRequested;
    }

    // When user selects an artist → filter library to that artist's tracks
    private void OnArtistSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ArtistsViewModel.SelectedArtist)) return;
        if (_artistsViewModel.SelectedArtist == null) return;
        _libraryViewModel.FilterByArtist(_artistsViewModel.SelectedArtist);
        CurrentView = _libraryViewModel;
    }

    // When user selects an album → filter library to that album's tracks
    private void OnAlbumSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AlbumsViewModel.SelectedAlbum)) return;
        if (_albumsViewModel.SelectedAlbum == null) return;
        _libraryViewModel.FilterByAlbum(_albumsViewModel.SelectedAlbum);
        CurrentView = _libraryViewModel;
    }

    // When user selects a playlist from the list → navigate into its detail view
    private void OnPlaylistSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PlaylistsViewModel.SelectedPlaylist)) return;
        if (_playlistsViewModel.SelectedPlaylist == null) return;
        _playlistViewModel.Load(_playlistsViewModel.SelectedPlaylist);
        CurrentView = _playlistViewModel;
    }

    // When user clicks "← Back" in PlaylistView → return to the playlists list
    private void OnPlaylistBackRequested()
    {
        _playlistsViewModel.Reload();
        _playlistsViewModel.SelectedPlaylist = null;
        CurrentView = _playlistsViewModel;
    }

    // Keep MainWindow's SearchText in sync with LibraryViewModel's SearchText
    private void OnLibraryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LibraryViewModel.SearchText))
            OnPropertyChanged(nameof(SearchText));
    }
}
```

---

### `ViewModels/NowPlayingViewModel.cs`

Manages the currently playing track and basic playback state (play/pause, next/prev). No actual audio output — simulated.

```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class NowPlayingViewModel : ViewModelBase
{
    private readonly IMusicLibraryService _musicLibraryService;

    private Track? _currentTrack;
    public Track? CurrentTrack
    {
        get => _currentTrack;
        set
        {
            SetProperty(ref _currentTrack, value);
            OnPropertyChanged(nameof(NowPlayingDisplay)); // computed property depends on this
        }
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    // Queue for next/previous navigation
    private int _currentTrackIndex;
    private List<Track> _queue = [];

    // Display string shown in the Now Playing strip
    public string NowPlayingDisplay => CurrentTrack == null
        ? "No track playing"
        : $"{CurrentTrack.Title} • {CurrentTrack.Artist.Name}";

    public ICommand PlayCommand     { get; }
    public ICommand PauseCommand    { get; }
    public ICommand NextCommand     { get; }
    public ICommand PreviousCommand { get; }

    public NowPlayingViewModel(IMusicLibraryService musicLibraryService)
    {
        _musicLibraryService = musicLibraryService;

        PlayCommand     = new RelayCommand(Play,     () => CurrentTrack != null);
        PauseCommand    = new RelayCommand(Pause,    () => IsPlaying);
        NextCommand     = new RelayCommand(Next,     () => _queue.Count > 0 && _currentTrackIndex < _queue.Count - 1);
        PreviousCommand = new RelayCommand(Previous, () => _queue.Count > 0 && _currentTrackIndex > 0);

        InitializeFirstTrack();
    }

    private void InitializeFirstTrack()
    {
        var allTracks = _musicLibraryService.GetAllTracks();
        if (allTracks.Any())
            PlayTrack(allTracks.First(), allTracks);
    }

    /// <summary>
    /// Called by LibraryViewModel and PlaylistViewModel when user double-clicks a track.
    /// Sets up the queue from the current visible list.
    /// </summary>
    public void PlayTrack(Track track, IEnumerable<Track> allAvailableTracks)
    {
        _queue = allAvailableTracks.ToList();
        _currentTrackIndex = _queue.IndexOf(track);
        if (_currentTrackIndex < 0) { _queue.Clear(); _currentTrackIndex = 0; }

        CurrentTrack = track;
        IsPlaying = true;
        CommandManager.InvalidateRequerySuggested(); // force button state refresh
    }

    private void Play()    { if (CurrentTrack != null) IsPlaying = true;  CommandManager.InvalidateRequerySuggested(); }
    private void Pause()   { IsPlaying = false; CommandManager.InvalidateRequerySuggested(); }
    private void Next()
    {
        if (_currentTrackIndex < _queue.Count - 1)
        { _currentTrackIndex++; CurrentTrack = _queue[_currentTrackIndex]; IsPlaying = true; }
        CommandManager.InvalidateRequerySuggested();
    }
    private void Previous()
    {
        if (_currentTrackIndex > 0)
        { _currentTrackIndex--; CurrentTrack = _queue[_currentTrackIndex]; IsPlaying = true; }
        CommandManager.InvalidateRequerySuggested();
    }
}
```

---

### `ViewModels/LibraryViewModel.cs`

The most complex ViewModel. Manages the track list with search + artist/album filtering, playlist context menu, and "add to new playlist" dialog.

```csharp
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
    private readonly IMusicLibraryService       _musicService;
    private readonly IPlaylistService           _playlistService;
    private readonly CreatePlaylistUseCase      _createPlaylistUseCase;
    private readonly AddTrackToPlaylistUseCase  _addTrackUseCase;
    private readonly NowPlayingViewModel        _nowPlayingViewModel;

    private List<Track>  _allTracks = [];
    private Artist?      _activeArtistFilter;
    private Album?       _activeAlbumFilter;

    // The displayed track collection (filtered subset of _allTracks)
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
        set { SetProperty(ref _selectedTrack, value); CommandManager.InvalidateRequerySuggested(); }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); ApplyFilter(); }
    }

    // Shows "Artist: X" or "Album: Y" banner when filtered; empty string means no filter active
    private string _activeFilterLabel = "";
    public string ActiveFilterLabel
    {
        get => _activeFilterLabel;
        set => SetProperty(ref _activeFilterLabel, value);
    }

    // Drives the "Add to Playlist" submenu in the ContextMenu
    public ObservableCollection<Playlist> AvailablePlaylists { get; } = [];

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand ClearSearchCommand      { get; }
    public ICommand ClearFilterCommand      { get; }
    public ICommand AddToPlaylistCommand    { get; }   // RelayCommand<Playlist>
    public ICommand AddToNewPlaylistCommand { get; }
    public ICommand PlayTrackCommand        { get; }   // RelayCommand<Track>

    public LibraryViewModel(
        IMusicLibraryService      musicService,
        IPlaylistService          playlistService,
        CreatePlaylistUseCase     createPlaylistUseCase,
        AddTrackToPlaylistUseCase addTrackUseCase,
        NowPlayingViewModel       nowPlayingViewModel)
    {
        _musicService          = musicService;
        _playlistService       = playlistService;
        _createPlaylistUseCase = createPlaylistUseCase;
        _addTrackUseCase       = addTrackUseCase;
        _nowPlayingViewModel   = nowPlayingViewModel;

        ClearSearchCommand      = new RelayCommand(() => SearchText = "");
        ClearFilterCommand      = new RelayCommand(ClearFilter, () => !string.IsNullOrEmpty(ActiveFilterLabel));
        AddToPlaylistCommand    = new RelayCommand<Playlist>(AddSelectedTrackToPlaylist,
                                      p => SelectedTrack != null && p != null);
        AddToNewPlaylistCommand = new RelayCommand(AddSelectedTrackToNewPlaylist,
                                      () => SelectedTrack != null);
        PlayTrackCommand        = new RelayCommand<Track>(PlayTrack, t => t != null);

        LoadTracks();
        RefreshPlaylists();
    }

    public void FilterByArtist(Artist artist)
    {
        _activeArtistFilter = artist;
        _activeAlbumFilter  = null;
        ActiveFilterLabel   = $"Artist: {artist.Name}";
        SearchText          = "";
        ApplyFilter();
    }

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

        if (_activeArtistFilter != null)
            result = result.Where(t => t.Artist.Id == _activeArtistFilter.Id);
        else if (_activeAlbumFilter != null)
            result = result.Where(t => t.Album.Id == _activeAlbumFilter.Id);

        if (!string.IsNullOrWhiteSpace(SearchText))
            result = result.Where(t =>
                t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Artist.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Album.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Tracks = new ObservableCollection<Track>(result);
    }

    private void AddSelectedTrackToPlaylist(Playlist? playlist)
    {
        if (SelectedTrack == null || playlist == null) return;
        try   { _addTrackUseCase.Execute(playlist.Id, SelectedTrack.Id); ErrorMessage = ""; RefreshPlaylists(); }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    private void AddSelectedTrackToNewPlaylist()
    {
        if (SelectedTrack == null) return;
        var dialog = new NewPlaylistDialog { Owner = System.Windows.Application.Current.MainWindow };
        if (dialog.ShowDialog() != true || dialog.PlaylistName == null) return;

        try
        {
            _createPlaylistUseCase.Execute(dialog.PlaylistName);
            var newPlaylist = _playlistService.GetAllPlaylists()
                .FirstOrDefault(p => p.Name == dialog.PlaylistName)
                ?? throw new InvalidOperationException("Playlist was not created.");

            _addTrackUseCase.Execute(newPlaylist.Id, SelectedTrack.Id);
            ErrorMessage = "";
            RefreshPlaylists();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    private void PlayTrack(Track? track)
    {
        if (track == null) return;
        _nowPlayingViewModel.PlayTrack(track, Tracks);
    }
}
```

---

### `ViewModels/ArtistsViewModel.cs`

```csharp
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
        // MainViewModel listens to PropertyChanged for this property
    }

    public ArtistsViewModel(IMusicLibraryService musicService)
    {
        foreach (var artist in musicService.GetAllArtists())
            Artists.Add(artist);
    }
}
```

---

### `ViewModels/AlbumsViewModel.cs`

```csharp
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
    }

    public AlbumsViewModel(IMusicLibraryService musicService)
    {
        foreach (var album in musicService.GetAllAlbums())
            Albums.Add(album);
    }
}
```

---

### `ViewModels/PlaylistsViewModel.cs`

```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class PlaylistsViewModel : ViewModelBase
{
    private readonly IPlaylistService      _playlistService;
    private readonly CreatePlaylistUseCase _createPlaylistUseCase;

    public ObservableCollection<Playlist> Playlists { get; } = [];

    private Playlist? _selectedPlaylist;
    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set => SetProperty(ref _selectedPlaylist, value);
        // MainViewModel listens to PropertyChanged to navigate to PlaylistView
    }

    private string _newPlaylistName = "";
    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set => SetProperty(ref _newPlaylistName, value);
    }

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand CreatePlaylistCommand { get; }

    public PlaylistsViewModel(IPlaylistService playlistService, CreatePlaylistUseCase createPlaylistUseCase)
    {
        _playlistService       = playlistService;
        _createPlaylistUseCase = createPlaylistUseCase;
        CreatePlaylistCommand  = new RelayCommand(CreatePlaylist,
            () => !string.IsNullOrWhiteSpace(NewPlaylistName));
        Reload();
    }

    private void CreatePlaylist()
    {
        try
        {
            _createPlaylistUseCase.Execute(NewPlaylistName);
            NewPlaylistName = "";
            ErrorMessage    = "";
            Reload();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    public void Reload()
    {
        Playlists.Clear();
        foreach (var p in _playlistService.GetAllPlaylists())
            Playlists.Add(p);
    }
}
```

---

### `ViewModels/PlaylistViewModel.cs`

```csharp
using AlconMusicPlayer.ApplicationService.UseCases;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    private readonly AddTrackToPlaylistUseCase    _addTrackUseCase;
    private readonly RemoveTrackFromPlaylistUseCase _removeTrackUseCase;
    private readonly NowPlayingViewModel           _nowPlayingViewModel;

    private Playlist? _playlist;

    public string PlaylistName => _playlist?.Name ?? "";
    public ObservableCollection<Track> Tracks { get; } = [];

    // C# event — MainViewModel subscribes to navigate back to playlist list
    public event Action? BackRequested;

    private Track? _selectedTrack;
    public Track? SelectedTrack
    {
        get => _selectedTrack;
        set { SetProperty(ref _selectedTrack, value); CommandManager.InvalidateRequerySuggested(); }
    }

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RemoveTrackCommand { get; }
    public ICommand BackCommand        { get; }
    public ICommand PlayTrackCommand   { get; }

    public PlaylistViewModel(
        AddTrackToPlaylistUseCase    addTrackUseCase,
        RemoveTrackFromPlaylistUseCase removeTrackUseCase,
        NowPlayingViewModel          nowPlayingViewModel)
    {
        _addTrackUseCase    = addTrackUseCase;
        _removeTrackUseCase = removeTrackUseCase;
        _nowPlayingViewModel = nowPlayingViewModel;

        RemoveTrackCommand = new RelayCommand(RemoveSelectedTrack,
            () => _playlist != null && SelectedTrack != null);
        BackCommand      = new RelayCommand(() => BackRequested?.Invoke());
        PlayTrackCommand = new RelayCommand<Track>(PlayTrack, t => t != null);
    }

    /// <summary>Called by MainViewModel when user selects a playlist.</summary>
    public void Load(Playlist playlist)
    {
        _playlist = playlist;
        Tracks.Clear();
        foreach (var t in playlist.Tracks)
            Tracks.Add(t);
        OnPropertyChanged(nameof(PlaylistName));
    }

    private void PlayTrack(Track? track)
    {
        if (track == null || _playlist == null) return;
        _nowPlayingViewModel.PlayTrack(track, Tracks);
    }

    private void RemoveSelectedTrack()
    {
        if (_playlist == null || SelectedTrack == null) return;
        try
        {
            _removeTrackUseCase.Execute(_playlist.Id, SelectedTrack.Id);
            ErrorMessage = "";
            ReloadTracks();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    private void ReloadTracks()
    {
        if (_playlist == null) return;
        Tracks.Clear();
        foreach (var t in _playlist.Tracks)
            Tracks.Add(t);
    }
}
```

---

### `ViewModels/SettingsViewModel.cs`

```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using AlconMusicPlayer.WPF.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlconMusicPlayer.WPF.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IThemeService _themeService;
        private string _selectedTheme;

        public ObservableCollection<string> AvailableThemes { get; }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (string.Equals(_selectedTheme, value, StringComparison.Ordinal)) return;
                SetProperty(ref _selectedTheme, value);
                _themeService.SetTheme(value);  // applies the ResourceDictionary swap
            }
        }

        public ICommand ResetCommand { get; }

        public SettingsViewModel(IThemeService themeService)
        {
            _themeService  = themeService;
            _selectedTheme = _themeService.CurrentTheme;
            AvailableThemes = new(_themeService.AvailableThemes);

            ResetCommand = new RelayCommand(() => SelectedTheme = "Light");
        }
    }
}
```

---

## 12. Step 8 — Views (XAML + Code-Behind)

### `Views/NowPlayingView.xaml`

Renders inside the Now Playing strip. Its `DataContext` is `NowPlayingViewModel` (set automatically by the DataTemplate in App.xaml).

```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.NowPlayingView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Height="60">
    <Border Background="Transparent" Padding="16,8">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"    />  <!-- Track info -->
                <ColumnDefinition Width="Auto" />  <!-- Controls -->
            </Grid.ColumnDefinitions>

            <TextBlock Grid.Column="0"
                       Text="{Binding NowPlayingDisplay}"
                       Foreground="{DynamicResource ForegroundBrush}"
                       FontSize="14" FontWeight="SemiBold"
                       VerticalAlignment="Center" Margin="0,0,16,0" />

            <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
                <Button Content="⏮" Command="{Binding PreviousCommand}"
                        Background="Transparent" Foreground="{DynamicResource MutedBrush}"
                        BorderThickness="0" Padding="8,4" Margin="0,0,4,0" Width="40" />

                <!-- Play button: visible only when NOT playing (invert=true) -->
                <Button Content="▶" Command="{Binding PlayCommand}"
                        Background="Transparent" Foreground="{DynamicResource ForegroundBrush}"
                        BorderThickness="0" Padding="8,4" Width="40"
                        Visibility="{Binding IsPlaying,
                                     Converter={StaticResource BoolToVisibilityConverter},
                                     ConverterParameter=invert}" />

                <!-- Pause button: visible only when playing -->
                <Button Content="⏸" Command="{Binding PauseCommand}"
                        Background="Transparent" Foreground="{DynamicResource ForegroundBrush}"
                        BorderThickness="0" Padding="8,4" Width="40"
                        Visibility="{Binding IsPlaying,
                                     Converter={StaticResource BoolToVisibilityConverter}}" />

                <Button Content="⏭" Command="{Binding NextCommand}"
                        Background="Transparent" Foreground="{DynamicResource MutedBrush}"
                        BorderThickness="0" Padding="8,4" Margin="4,0,0,0" Width="40" />
            </StackPanel>
        </Grid>
    </Border>
</UserControl>
```

**Code-behind** (`NowPlayingView.xaml.cs`) — minimal:
```csharp
using System.Windows.Controls;

namespace AlconMusicPlayer.WPF.Views;

public partial class NowPlayingView : UserControl
{
    public NowPlayingView() { InitializeComponent(); }
}
```

---

### `Views/LibraryView.xaml`

The most complex view. Contains a DataGrid with a ContextMenu that has a submenu of available playlists.

**ContextMenu visual-tree trick:** A ContextMenu is not part of the visual tree of the control that opens it — it lives in a separate popup. So it cannot find the ViewModel through normal `DataContext` inheritance. The solution:

1. Set `DataGrid.Tag="{Binding}"` — this stores the LibraryViewModel in the DataGrid's Tag.
2. Set `DataGridRow.Tag` to `{Binding DataContext, RelativeSource={RelativeSource AncestorType=DataGrid}}` — copies LibraryViewModel into each row's Tag.
3. In the ContextMenu: `DataContext="{Binding PlacementTarget.Tag, RelativeSource={RelativeSource Self}}"` — PlacementTarget is the DataGridRow, Tag is LibraryViewModel.

```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.LibraryView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />  <!-- Filter label -->
            <RowDefinition Height="Auto" />  <!-- Error message -->
            <RowDefinition Height="*"    />  <!-- Track list -->
        </Grid.RowDefinitions>

        <!-- Active filter banner (only visible when artist/album filter is active) -->
        <Grid Grid.Row="0" Margin="0,0,0,8"
              Visibility="{Binding ActiveFilterLabel,
                           Converter={StaticResource StringToVisibilityConverter}}">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"    />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBlock Grid.Column="0" Text="{Binding ActiveFilterLabel}"
                       Foreground="{DynamicResource AccentBrush}"
                       FontSize="13" VerticalAlignment="Center" />
            <Button    Grid.Column="1" Content="✕ Clear Filter"
                       Command="{Binding ClearFilterCommand}"
                       Background="Transparent" Foreground="{DynamicResource MutedBrush}"
                       BorderThickness="0" Padding="8,4" />
        </Grid>

        <!-- Error message (only visible when ErrorMessage is non-empty) -->
        <TextBlock Grid.Row="1"
                   Text="{Binding ErrorMessage}"
                   Foreground="{DynamicResource AccentBrush}"
                   Margin="0,0,0,6"
                   Visibility="{Binding ErrorMessage, Converter={StaticResource StringToVisibilityConverter}}" />

        <!-- Track DataGrid -->
        <DataGrid Grid.Row="2"
                  ItemsSource="{Binding Tracks}"
                  SelectedItem="{Binding SelectedTrack}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  SelectionMode="Single"
                  MouseDoubleClick="TracksGrid_MouseDoubleClick"
                  Tag="{Binding}">

            <DataGrid.Columns>
                <DataGridTextColumn Header="Title"    Binding="{Binding Title}"           Width="*"   />
                <DataGridTextColumn Header="Artist"   Binding="{Binding Artist.Name}"     Width="150" />
                <DataGridTextColumn Header="Album"    Binding="{Binding Album.Name}"      Width="150" />
                <DataGridTextColumn Header="Genre"    Binding="{Binding Genre}"           Width="100" />
                <DataGridTextColumn Header="Duration" Binding="{Binding DurationDisplay}" Width="80"  />
                <DataGridTextColumn Header="Plays"    Binding="{Binding PlayCount}"       Width="60"  />
            </DataGrid.Columns>

            <DataGrid.RowStyle>
                <Style TargetType="DataGridRow">
                    <!-- Step 2: Copy LibraryViewModel into each row's Tag -->
                    <Setter Property="Tag"
                            Value="{Binding DataContext, RelativeSource={RelativeSource AncestorType=DataGrid}}" />
                    <EventSetter Event="PreviewMouseRightButtonDown"
                                 Handler="DataGridRow_PreviewMouseRightButtonDown" />
                    <Setter Property="ContextMenu">
                        <Setter.Value>
                            <!-- Step 3: ContextMenu.DataContext = PlacementTarget(row).Tag = LibraryViewModel -->
                            <ContextMenu DataContext="{Binding PlacementTarget.Tag,
                                                      RelativeSource={RelativeSource Self}}">

                                <!-- Play option: CommandParameter = the Track (row's DataContext) -->
                                <MenuItem Header="▶ Play"
                                          Command="{Binding PlayTrackCommand}"
                                          CommandParameter="{Binding PlacementTarget.DataContext,
                                                                     RelativeSource={RelativeSource AncestorType=ContextMenu}}" />

                                <Separator />

                                <!-- Add to Playlist submenu — ItemsSource = LibraryViewModel.AvailablePlaylists -->
                                <MenuItem Header="Add to Playlist"
                                          ToolTip="Create a playlist first"
                                          ItemsSource="{Binding AvailablePlaylists}">
                                    <MenuItem.ItemContainerStyle>
                                        <Style TargetType="MenuItem">
                                            <Setter Property="Header" Value="{Binding Name}" />
                                            <!-- Command is on LibraryViewModel (ContextMenu's DataContext) -->
                                            <Setter Property="Command"
                                                    Value="{Binding DataContext.AddToPlaylistCommand,
                                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}" />
                                            <!-- CommandParameter is the Playlist item itself -->
                                            <Setter Property="CommandParameter" Value="{Binding}" />
                                        </Style>
                                    </MenuItem.ItemContainerStyle>
                                </MenuItem>

                                <Separator />

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

**Code-behind** (`LibraryView.xaml.cs`):
```csharp
using System.Windows.Controls;
using System.Windows.Input;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels;

namespace AlconMusicPlayer.WPF.Views
{
    public partial class LibraryView : UserControl
    {
        public LibraryView() { InitializeComponent(); }

        // Right-click: ensure the row under the cursor is selected before the context menu opens
        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row) { row.IsSelected = true; row.Focus(); }
        }

        // Double-click: play the clicked track
        private void TracksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not LibraryViewModel viewModel) return;
            if (sender is not DataGrid dataGrid) return;
            if (dataGrid.SelectedItem is not Track track) return;

            if (viewModel.PlayTrackCommand.CanExecute(track))
                viewModel.PlayTrackCommand.Execute(track);
        }
    }
}
```

---

### `Views/ArtistsView.xaml`

```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.ArtistsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*"    />
        </Grid.RowDefinitions>
        <TextBlock Grid.Row="0" Text="Artists"
                   Foreground="{DynamicResource MutedBrush}"
                   FontSize="11" Margin="0,0,0,8" />
        <!--
            SelectedItem two-way binding:
            When user clicks → ArtistsViewModel.SelectedArtist is set
            → PropertyChanged fires → MainViewModel handles it → navigates to library filtered by artist
        -->
        <ListBox Grid.Row="1"
                 ItemsSource="{Binding Artists}"
                 SelectedItem="{Binding SelectedArtist}"
                 DisplayMemberPath="Name" />
    </Grid>
</UserControl>
```

**Code-behind**: standard `InitializeComponent()` only.

---

### `Views/AlbumsView.xaml`

```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.AlbumsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*"    />
        </Grid.RowDefinitions>
        <TextBlock Grid.Row="0" Text="Albums"
                   Foreground="{DynamicResource MutedBrush}"
                   FontSize="11" Margin="0,0,0,8" />
        <ListBox Grid.Row="1"
                 ItemsSource="{Binding Albums}"
                 SelectedItem="{Binding SelectedAlbum}"
                 DisplayMemberPath="Name" />
    </Grid>
</UserControl>
```

---

### `Views/PlaylistsView.xaml`

```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.PlaylistsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />  <!-- Create new playlist bar -->
            <RowDefinition Height="*"    />  <!-- Playlist list -->
        </Grid.RowDefinitions>

        <!-- Inline create: type name + click button -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,12">
            <TextBox Text="{Binding NewPlaylistName, UpdateSourceTrigger=PropertyChanged}"
                     Width="200" Padding="8,6" />
            <Button Content="New Playlist"
                    Command="{Binding CreatePlaylistCommand}"
                    Margin="8,0,0,0" />
        </StackPanel>

        <!-- Click a playlist → SelectedPlaylist set → MainViewModel navigates to PlaylistView -->
        <ListBox Grid.Row="1"
                 ItemsSource="{Binding Playlists}"
                 SelectedItem="{Binding SelectedPlaylist}"
                 DisplayMemberPath="Name" />
    </Grid>
</UserControl>
```

---

### `Views/PlaylistView.xaml`

Same ContextMenu pattern as LibraryView, but simpler (only a "▶ Play" option).

```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.PlaylistView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />  <!-- Back button -->
            <RowDefinition Height="Auto" />  <!-- Playlist title -->
            <RowDefinition Height="*"    />  <!-- Track list -->
        </Grid.RowDefinitions>

        <Button Grid.Row="0" Content="← Back"
                Command="{Binding BackCommand}"
                Background="Transparent" Foreground="{DynamicResource MutedBrush}"
                BorderThickness="0" Padding="8,4"
                Margin="0,0,0,8" HorizontalAlignment="Left" />

        <TextBlock Grid.Row="1"
                   Text="{Binding PlaylistName}"
                   FontSize="20" Margin="0,0,0,12"
                   Foreground="{DynamicResource ForegroundBrush}" />

        <DataGrid Grid.Row="2"
                  ItemsSource="{Binding Tracks}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  SelectedItem="{Binding SelectedTrack}"
                  MouseDoubleClick="TracksGrid_MouseDoubleClick"
                  SelectionChanged="DataGrid_SelectionChanged">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Title"  Binding="{Binding Title}"       Width="*"   />
                <DataGridTextColumn Header="Artist" Binding="{Binding Artist.Name}" Width="150" />
                <DataGridTextColumn Header="Album"  Binding="{Binding Album.Name}"  Width="150" />
            </DataGrid.Columns>

            <DataGrid.RowStyle>
                <Style TargetType="DataGridRow">
                    <Setter Property="Tag"
                            Value="{Binding DataContext, RelativeSource={RelativeSource AncestorType=DataGrid}}" />
                    <EventSetter Event="PreviewMouseRightButtonDown"
                                 Handler="DataGridRow_PreviewMouseRightButtonDown" />
                    <Setter Property="ContextMenu">
                        <Setter.Value>
                            <ContextMenu DataContext="{Binding PlacementTarget.Tag,
                                                              RelativeSource={RelativeSource Self}}">
                                <MenuItem Header="▶ Play"
                                          Command="{Binding PlayTrackCommand}"
                                          CommandParameter="{Binding PlacementTarget.DataContext,
                                                                     RelativeSource={RelativeSource AncestorType=ContextMenu}}" />
                            </ContextMenu>
                        </Setter.Value>
                    </Setter>
                </Style>
            </DataGrid.RowStyle>
        </DataGrid>
    </Grid>
</UserControl>
```

**Code-behind** (`PlaylistView.xaml.cs`):
```csharp
using System.Windows.Controls;
using System.Windows.Input;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels;

namespace AlconMusicPlayer.WPF.Views
{
    public partial class PlaylistView : UserControl
    {
        public PlaylistView() { InitializeComponent(); }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row) { row.IsSelected = true; row.Focus(); }
        }

        private void TracksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not PlaylistViewModel viewModel) return;
            if (sender is not DataGrid dataGrid) return;
            if (dataGrid.SelectedItem is not Track track) return;

            if (viewModel.PlayTrackCommand.CanExecute(track))
                viewModel.PlayTrackCommand.Execute(track);
        }
    }
}
```

---

### `Views/SettingsView.xaml`

Theme switching is intentionally disabled (`IsEnabled="False"`) — the ComboBox and Reset button are shown for future use.

```xml
<UserControl x:Class="AlconMusicPlayer.WPF.Views.SettingsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Background="{DynamicResource BackgroundBrush}"
             Foreground="{DynamicResource ForegroundBrush}">
    <Grid Margin="16">
        <StackPanel VerticalAlignment="Top">
            <TextBlock Text="Settings"
                       FontSize="18" FontWeight="SemiBold"
                       Foreground="{DynamicResource ForegroundBrush}"
                       Margin="0,0,0,20" />

            <TextBlock Text="Theme"
                       FontSize="12" FontWeight="SemiBold"
                       Foreground="{DynamicResource ForegroundBrush}"
                       Margin="0,0,0,8" />

            <TextBlock Text="Light theme is currently fixed for stability. Theme switching will be revisited later."
                       Foreground="{DynamicResource MutedBrush}"
                       TextWrapping="Wrap" Margin="0,0,0,12" />

            <ComboBox ItemsSource="{Binding AvailableThemes}"
                      SelectedItem="{Binding SelectedTheme}"
                      IsEnabled="False"
                      Background="{DynamicResource SurfaceBrush}"
                      Foreground="{DynamicResource ForegroundBrush}"
                      Padding="8,6" Margin="0,0,0,16" MinWidth="150" />

            <Button Content="Reset to Default"
                    Command="{Binding ResetCommand}"
                    IsEnabled="False"
                    Background="{DynamicResource AccentBrush}"
                    Foreground="{DynamicResource SelectionTextBrush}"
                    Padding="12,8" Margin="0,20,0,0" Cursor="Hand" />
        </StackPanel>
    </Grid>
</UserControl>
```

---

## 13. Step 9 — Dialogs

### `Dialogs/NewPlaylistDialog.xaml`

A small modal `Window` (not a `UserControl`). Owned by `MainWindow` so it centers on top of it.

```xml
<Window x:Class="AlconMusicPlayer.WPF.Dialogs.NewPlaylistDialog"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="New Playlist" Height="200" Width="360"
        WindowStartupLocation="CenterOwner"
        ResizeMode="NoResize"
        Background="{DynamicResource BackgroundBrush}">
    <StackPanel Margin="16">
        <TextBlock Text="Playlist name:"
                   Foreground="{DynamicResource ForegroundBrush}" Margin="0,0,0,8" />
        <TextBox x:Name="NameBox"
                 Background="{DynamicResource SurfaceBrush}"
                 Foreground="{DynamicResource ForegroundBrush}"
                 BorderBrush="{DynamicResource HoverBrush}"
                 Padding="8,6"
                 KeyDown="NameBox_KeyDown" />
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
            <Button Content="Cancel" Width="75" Margin="0,0,8,0"
                    Click="Cancel_Click"
                    Background="{DynamicResource HoverBrush}"
                    Foreground="{DynamicResource ForegroundBrush}"
                    BorderThickness="0" Padding="8,6" />
            <Button Content="Create" Width="75"
                    Click="Create_Click"
                    Background="{DynamicResource PrimaryActionBrush}"
                    Foreground="{DynamicResource ForegroundBrush}"
                    BorderThickness="0" Padding="8,6" />
        </StackPanel>
    </StackPanel>
</Window>
```

### `Dialogs/NewPlaylistDialog.xaml.cs`

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
        Loaded += (_, _) => NameBox.Focus();  // auto-focus input on open
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
        DialogResult = true;  // closes the dialog and ShowDialog() returns true
    }
}
```

**Usage in LibraryViewModel:**
```csharp
var dialog = new NewPlaylistDialog { Owner = Application.Current.MainWindow };
if (dialog.ShowDialog() == true && dialog.PlaylistName != null)
{
    // use dialog.PlaylistName
}
```

---

## 14. Step 10 — Services

### `Services/ThemeService.cs`

Implements `IThemeService` from `ApplicationService.Interfaces`. Swaps the theme `ResourceDictionary` at runtime.

```csharp
using AlconMusicPlayer.ApplicationService.Interfaces;
using System.Windows;

namespace AlconMusicPlayer.WPF.Services
{
    public class ThemeService : IThemeService
    {
        private string _currentTheme = "Light";

        public string CurrentTheme => _currentTheme;

        // Only "Light" is exposed; Dark is stubbed in resources but not offered yet
        public IReadOnlyList<string> AvailableThemes => new[] { "Light" };

        public event EventHandler? ThemeChanged;

        public void SetTheme(string themeName)
        {
            if (!AvailableThemes.Contains(themeName))
                throw new ArgumentException($"Theme '{themeName}' not found.");

            if (_currentTheme == themeName) return;

            _currentTheme = themeName;
            ApplyTheme(themeName);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyTheme(string themeName)
        {
            var app = Application.Current;
            if (app == null) return;

            // Build the new theme's URI
            var themePath = $"pack://application:,,,/Resources/Themes/{themeName}Theme.xaml";
            var themeDictionary = new ResourceDictionary { Source = new Uri(themePath) };

            // Remove the current theme dictionary
            for (var i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var dict   = app.Resources.MergedDictionaries[i];
                var source = dict.Source?.OriginalString;
                if (source != null && source.Contains("/Resources/Themes/", StringComparison.OrdinalIgnoreCase))
                    app.Resources.MergedDictionaries.RemoveAt(i);
            }

            // Insert the new theme at position 0 (before Styles.xaml)
            app.Resources.MergedDictionaries.Insert(0, themeDictionary);
        }
    }
}
```

**Why `DynamicResource` in XAML?** `StaticResource` is resolved once at load time. `DynamicResource` re-resolves every time the resource changes. Because `SetTheme()` replaces the ResourceDictionary at runtime, only `DynamicResource` will pick up the change.

---

## 15. How Everything Connects

### Navigation flow

```
User clicks sidebar button
       ↓
MainViewModel.ShowXxxCommand executes
       ↓
CurrentView = _xxxViewModel
       ↓
MainWindow: ContentControl.Content changes
       ↓
WPF looks up DataTemplate for the ViewModel type (App.xaml)
       ↓
Renders the matching UserControl View
       ↓
View's DataContext = the ViewModel (set automatically by ContentControl)
```

### Cross-ViewModel communication flow

```
ArtistsView: user clicks an artist
       ↓
ArtistsViewModel.SelectedArtist = artist  (SetProperty fires PropertyChanged)
       ↓
MainViewModel.OnArtistSelectionChanged() runs
       ↓
LibraryViewModel.FilterByArtist(artist)  ← updates the track list
CurrentView = _libraryViewModel          ← navigates to library
```

### PlaylistView "Back" flow

```
User clicks "← Back" in PlaylistView
       ↓
PlaylistViewModel.BackCommand executes
       ↓
BackRequested?.Invoke()  ← C# event
       ↓
MainViewModel.OnPlaylistBackRequested() runs
       ↓
PlaylistsViewModel.Reload()
CurrentView = _playlistsViewModel
```

### DI resolution chain

```
App.OnStartup()
  → serviceProvider.GetRequiredService<MainWindow>()
  → serviceProvider.GetRequiredService<MainViewModel>()
      → constructor needs: LibraryViewModel, ArtistsViewModel, AlbumsViewModel,
                           PlaylistsViewModel, PlaylistViewModel,
                           NowPlayingViewModel, SettingsViewModel
      → each of those constructor-inject services and use cases
      → services inject repositories
```

### File creation order (recommended)

1. Create project + `.csproj`
2. `ViewModels/Base/ViewModelBase.cs`
3. `ViewModels/Base/RelayCommand.cs`
4. `Converters/BoolToVisibilityConverter.cs`
5. `Converters/StringToVisibilityConverter.cs`
6. `Resources/Themes/LightTheme.xaml`
7. `Resources/Themes/DarkTheme.xaml`
8. `Resources/Styles.xaml`
9. All **ViewModels** (non-Main first, then `MainViewModel`)
10. All **Views** (XAML + code-behind)
11. `Dialogs/NewPlaylistDialog.xaml` + `.cs`
12. `Services/ThemeService.cs`
13. `App.xaml` (DataTemplates + ResourceDictionaries)
14. `App.xaml.cs` (DI container — remove `StartupUri` from App.xaml!)
15. `MainWindow.xaml` + `MainWindow.xaml.cs`
