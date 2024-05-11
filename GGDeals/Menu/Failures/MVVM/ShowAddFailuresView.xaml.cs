using System.Windows.Controls;

namespace GGDeals.Menu.Failures.MVVM
{
    /// <summary>
    /// Interaction logic for ShowAddFailuresView.xaml
    /// </summary>
    public partial class ShowAddFailuresView : UserControl
    {
        public ShowAddFailuresView(ShowAddFailuresViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}