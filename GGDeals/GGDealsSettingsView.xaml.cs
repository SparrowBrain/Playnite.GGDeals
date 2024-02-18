using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;

namespace GGDeals
{
    public partial class GGDealsSettingsView : UserControl
    {
        public GGDealsSettingsView(IPlayniteAPI api)
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}