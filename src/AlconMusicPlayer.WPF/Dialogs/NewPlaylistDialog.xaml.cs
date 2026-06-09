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
        if (e.Key == Key.Enter) TryAccept();
        if (e.Key == Key.Escape) DialogResult = false;
    }

    private void TryAccept()
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) return;
        PlaylistName = NameBox.Text.Trim();
        DialogResult = true;
    }
}