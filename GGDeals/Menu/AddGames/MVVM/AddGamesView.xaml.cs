using System.Windows.Controls;

namespace GGDeals.Menu.AddGames.MVVM
{
    /// <summary>
    /// Interaction logic for AddGames.xaml
    /// </summary>
    public partial class AddGamesView : UserControl
    {
        public AddGamesView(AddGamesViewModel addGamesViewModel)
        {
            InitializeComponent();
            DataContext = addGamesViewModel;
        }
    }
}