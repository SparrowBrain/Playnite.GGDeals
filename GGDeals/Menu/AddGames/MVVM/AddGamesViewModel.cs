using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Playnite.SDK;

namespace GGDeals.Menu.AddGames.MVVM
{
    public class AddGamesViewModel : ObservableObject
    {
        private readonly GGDeals _plugin;
        private bool _includeHidden;

        public AddGamesViewModel(GGDeals plugin)
        {
            _plugin = plugin;
        }

        public bool IncludeHidden
        {
            get => _includeHidden;
            set => SetValue(ref _includeHidden, value);
        }

        // ReSharper disable once UnusedMember.Global
        public ICommand AddAllGames => new RelayCommand(() =>
        {
            var games = IncludeHidden
                ? _plugin.PlayniteApi.Database.Games.ToList()
                : _plugin.PlayniteApi.Database.Games.Where(game => !game.Hidden).ToList();

            _plugin.AddGamesToGGCollection(games);
        });
    }
}