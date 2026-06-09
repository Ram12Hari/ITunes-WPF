
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