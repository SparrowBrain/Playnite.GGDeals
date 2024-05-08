using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GGDeals.Services;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace GGDeals.AddFailures.MVVM
{
    public class ShowAddFailuresViewModel : ObservableObject
    {
        private readonly ILogger _logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _api;
        private readonly AddFailuresManager _addFailuresManager;
        private ObservableCollection<FailureItem> _failures;

        public ShowAddFailuresViewModel(IPlayniteAPI api, AddFailuresManager addFailuresManager)
        {
            _api = api;
            _addFailuresManager = addFailuresManager;
            Load();
        }

        public ObservableCollection<FailureItem> Failures
        {
            get => _failures;
            set => SetValue(ref _failures, value);
        }

        public void Load()
        {
            Task.Run(async () =>
            {
                try
                {
                    var failures = await _addFailuresManager.GetFailures();
                    var games = _api.Database.Games.Where(x => failures.ContainsKey(x.Id));
                    var libraries = _api.Addons.Plugins
                        .Where(x => games.Select(g => g.PluginId).Contains(x.Id))
                        .Select(x => x as LibraryPlugin).ToList();

                    var failureItems = games.Select(x => new FailureItem(
                        x.Id,
                        x.Name,
                        libraries.FirstOrDefault(l => l.Id == x.PluginId)?.Name,
                        failures[x.Id]));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Failures = new ObservableCollection<FailureItem>(failureItems);
                    });
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to load failures");
                }
            });
        }
    }

    public class FailureItem : ObservableObject
    {
        private bool _isChecked;

        public FailureItem(Guid id, string name, string libraryName, AddToCollectionResult result)
        {
            Id = id;
            Name = name;
            LibraryName = libraryName;
            Result = result;
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetValue(ref _isChecked, value);
        }

        public Guid Id { get; }
        public string Name { get; }
        public string LibraryName { get; }
        public AddToCollectionResult Result { get; }
    }
}