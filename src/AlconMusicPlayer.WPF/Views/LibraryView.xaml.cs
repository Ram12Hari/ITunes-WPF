using System.Windows.Controls;
using System.Windows.Input;
using AlconMusicPlayer.WPF.ViewModels;

namespace AlconMusicPlayer.WPF.Views
{
    public partial class LibraryView : UserControl
    {
        public LibraryView()
        {
            InitializeComponent();
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
                row.Focus();
            }
        }

    }
}
