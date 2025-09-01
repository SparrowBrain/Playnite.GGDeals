using GGDeals.Api.Services;
using GGDeals.Menu.AddGames.MVVM;
using GGDeals.Menu.Failures;
using GGDeals.Menu.Failures.File;
using GGDeals.Menu.Failures.MVVM;
using GGDeals.Models;
using GGDeals.Progress.MVVM;
using GGDeals.Queue;
using GGDeals.Services;
using GGDeals.Settings;
using GGDeals.Settings.MVVM;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GGDeals
{
    public class GGDeals : GenericPlugin
    {
        public const string PluginId = "2af05ded-085c-426b-a10e-8e03185092bf";
        private const string FailuresFileName = "failures.json";
        private const string QueueFileName = "queue.json";
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly JsonSerializerSettings _apiJsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
        };

        private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);

        private readonly IPlayniteAPI _api;
        private readonly StartupSettingsValidator _startupSettingsValidator;
        private readonly AddFailuresManager _addFailuresManager;
        private readonly GameStatusService _gameStatusService;
        private readonly PersistentProcessingQueue _persistentProcessingQueue;
        private readonly Action _openFailuresView;

        private GGDealsSettingsViewModel _settings;
        private LibraryToGGLauncherMap _libraryToGGLauncherMap = new LibraryToGGLauncherMap();

        public override Guid Id { get; } = Guid.Parse(PluginId);

        public GGDeals(IPlayniteAPI api) : base(api)
        {
            _api = api;
            var failuresFilePath = Path.Combine(GetPluginUserDataPath(), FailuresFileName);
            var addFailuresFileService = new AddFailuresFileService(failuresFilePath);
            _addFailuresManager = new AddFailuresManager(addFailuresFileService);
            _persistentProcessingQueue = new PersistentProcessingQueue(
                new QueuePersistence(Path.Combine(GetPluginUserDataPath(), QueueFileName)),
                gameIds => AddGamesToGGCollectionAsync(gameIds, SyncRunSettings.Default));

            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            PlayniteApi.Database.Games.ItemCollectionChanged += async (_, gamesAddedArgs) =>
            {
                var settings = LoadPluginSettings<GGDealsSettings>();
                if (settings.SyncNewlyAddedGames)
                {
                    await _persistentProcessingQueue.Enqueue(gamesAddedArgs.AddedItems.Select(x => x.Id).ToList());
                }
            };

            var pluginSettingsPersistence = new PluginSettingsPersistence(this);
            _startupSettingsValidator = new StartupSettingsValidator(pluginSettingsPersistence, new SettingsMigrator(pluginSettingsPersistence));
            _openFailuresView = () =>
            {
                var showAddFailuresViewModel = new ShowAddFailuresViewModel(this, _addFailuresManager);
                ShowDialog(
                    new ShowAddFailuresView(showAddFailuresViewModel),
                    768,
                    768,
                    ResourceProvider.GetString("LOC_GGDeals_ShowAddFailuresTitle"),
                    true,
                    true);
            };
            _gameStatusService = new GameStatusService(_api);
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            _startupSettingsValidator.EnsureCorrectVersionSettingsExist();
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            _persistentProcessingQueue.ProcessInBackground();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOC_GGDeals_GameMenuItemAddToGGDealsCollection"),
                Action = actionArgs => { AddGamesToGGCollection(actionArgs.Games); },
                MenuSection = "GG.deals"
            };

            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOC_GGDeals_GameMenuItemAddToGGDealsCollectionCustom"),
                Action = actionArgs => { ShowAddGamesDialog(actionArgs.Games); },
                MenuSection = "GG.deals"
            };
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOC_GGDeals_MainMenuItemAddToGGDealsCollection"),
                MenuSection = "@GG.deals",
                Action = actionArgs => { ShowAddGamesDialog(_api.Database.Games.ToList()); }
            };

            yield return new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOC_GGDeals_MainMenuItemShowFailures"),
                MenuSection = "@GG.deals",
                Action = actionArgs => _openFailuresView.Invoke()
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings ?? (_settings = new GGDealsSettingsViewModel(this, _libraryToGGLauncherMap));
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GGDealsSettingsView(PlayniteApi);
        }

        public void AddGamesToGGCollection(List<Game> games)
        {
            AddGamesToGGCollection(games, SyncRunSettings.Default);
        }

        public void AddGamesToGGCollection(List<Game> games, SyncRunSettings syncRunSettings)
        {
            Task.Run(async () =>
            {
                await AddGamesToGGCollectionAsync(games.Select(x => x.Id).ToList(), syncRunSettings);
            });
        }

        public async Task AddGamesToGGCollectionAsync(IReadOnlyCollection<Guid> gameIds, SyncRunSettings syncRunSettings)
        {
            if (!gameIds.Any())
            {
                return;
            }

            try
            {
                await _syncSemaphore.WaitAsync();
                var games = _api.Database.Games.Where(x => gameIds.Contains(x.Id)).ToList();
                var settings = LoadPluginSettings<GGDealsSettings>();
                using (var ggDealsApiClient = new GGDealsApiClient(settings, _apiJsonSerializerSettings))
                {
                    var gameToAddFilter = new GameToAddFilter(settings, _gameStatusService, syncRunSettings);
                    var libraryToGGLauncherMap = _libraryToGGLauncherMap;
                    var gameToGameWithLauncherConverter = new GameToGameWithLauncherConverter(libraryToGGLauncherMap, settings);
                    var requestDataBatcher = new RequestDataBatcher(_apiJsonSerializerSettings);
                    var addGamesService = new AddGamesService(settings, gameToAddFilter, gameToGameWithLauncherConverter, requestDataBatcher, ggDealsApiClient);
                    var addLinkService = new AddLinkService(_api);
                    var addResultProcessor = new AddResultProcessor(settings, _gameStatusService, addLinkService);

                    var ggDealsService = new GGDealsService(settings, _openFailuresView, PlayniteApi, addGamesService, _addFailuresManager, addResultProcessor);

                    using (var cancellationTokenSource = new CancellationTokenSource())
                    using (var progressViewModel = ShowProgressDialog(cancellationTokenSource, settings.ShowProgressBar))
                    {
                        Action<float> reportProgress = progress =>
                        {
                            _api.MainView.UIDispatcher.Invoke(() => progressViewModel.ProgressValue = progress);
                        };
                        await ggDealsService.AddGamesToLibrary(games, reportProgress, cancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to add games to GG.deals collection.");
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        private ProgressViewModel ShowProgressDialog(CancellationTokenSource cts, bool showProgressBar)
        {
            var progressViewModel = new ProgressViewModel(_api, cts);
            if (showProgressBar)
            {
                _api.MainView.UIDispatcher.Invoke(() =>
                    {
                        var window = ShowDialog(
                            new ProgressView(progressViewModel),
                            100,
                            250,
                            ResourceProvider.GetString("LOC_GGDeals_ProgressTitle"),
                            false,
                            false);

                        progressViewModel.SetWindow(window);
                    }
                );
            }

            return progressViewModel;
        }

        private void ShowAddGamesDialog(List<Game> games)
        {
            var addGamesViewModel = new AddGamesViewModel(this, games);
            ShowDialog(
                new AddGamesView(addGamesViewModel),
                250,
                500,
                ResourceProvider.GetString("LOC_GGDeals_AddGamesTitle"),
                false,
                false);
        }

        private Window ShowDialog(UserControl view, double height, double width, string title, bool showMaximizeButton, bool waitToClose)
        {
            var window = _api.Dialogs.CreateWindow(new WindowCreationOptions()
            {
                ShowCloseButton = true,
                ShowMaximizeButton = showMaximizeButton,
                ShowMinimizeButton = false,
            });

            window.Height = height;
            window.Width = width;
            window.Title = title;

            window.Content = view;
            if (view.DataContext is IViewModelForWindow viewModelForWindow)
            {
                viewModelForWindow.AssociateWindow(window);
            }

            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (waitToClose)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
            }

            return window;
        }
    }
}