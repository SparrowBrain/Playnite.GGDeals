using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace GGDeals.Menu.Failures.MVVM
{
    public class ShowAddFailuresViewModel : ObservableObject
    {
        private readonly ILogger _logger = LogManager.GetLogger();
        private readonly GGDeals _plugin;
        private readonly AddFailuresManager _addFailuresManager;
        private ObservableCollection<FailureItem> _failures;
        private bool? _isAllChecked;
        private bool _isAllCheckedThreeState;

        public ShowAddFailuresViewModel(GGDeals plugin, AddFailuresManager addFailuresManager)
        {
            _plugin = plugin;
            _addFailuresManager = addFailuresManager;
            Load();
        }

        public ObservableCollection<FailureItem> Failures
        {
            get => _failures;
            set => SetValue(ref _failures, value);
        }

        // ReSharper disable once UnusedMember.Global
        public bool? IsAllChecked
        {
            get => _isAllChecked;
            set
            {
                IsAllCheckedThreeState = value.HasValue;
                if (!value.HasValue)
                {
                    SetValue(ref _isAllChecked, value);
                    return;
                }

                foreach (var failure in Failures)
                {
                    failure.IsChecked = value.Value;
                }

                SetValue(ref _isAllChecked, value);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public bool IsAllCheckedThreeState
        {
            get => _isAllCheckedThreeState;
            set => SetValue(ref _isAllCheckedThreeState, value);
        }

        public void Load()
        {
            Task.Run(async () =>
            {
                try
                {
                    var failures = await _addFailuresManager.GetFailures();
                    var games = _plugin.PlayniteApi.Database.Games.Where(x => failures.ContainsKey(x.Id));
                    var libraries = _plugin.PlayniteApi.Addons.Plugins
                        .Where(x => games.Select(g => g.PluginId).Contains(x.Id))
                        .Select(x => x as LibraryPlugin).ToList();

                    var failureItems = games.Select(x => new FailureItem(
                        this,
                        x,
                        libraries.FirstOrDefault(l => l.Id == x.PluginId)?.Name,
                        failures[x.Id]));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Failures = new ObservableCollection<FailureItem>(failureItems);
                    });
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to load failures.");
                }
            });
        }

        // ReSharper disable once UnusedMember.Global
        public ICommand RetryChecked => new RelayCommand(() =>
        {
            var games = Failures.Where(x => x.IsChecked).Select(x => x.Game).ToList();
            _plugin.AddGamesToGGCollection(games);
        });

        // ReSharper disable once UnusedMember.Global
        public ICommand RemoveChecked => new RelayCommand(() =>
        {
            var games = Failures.Where(x => x.IsChecked).Select(x => x.Game.Id).ToList();
            Task.Run(async () =>
            {
                try
                {
                    await _addFailuresManager.RemoveFailures(games);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to remove failures.");
                }
            });
        });
    }
}