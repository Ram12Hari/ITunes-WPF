using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AlconMusicPlayer.Domain.Entities;
using AlconMusicPlayer.WPF.ViewModels;

namespace AlconMusicPlayer.WPF.Views
{
    /// <summary>
    /// Interaction logic for PlaylistView.xaml
    /// </summary>
    public partial class PlaylistView : UserControl
    {
        public PlaylistView()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
                row.Focus();
            }
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
