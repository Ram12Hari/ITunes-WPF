
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