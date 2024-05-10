using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GGDeals.AddFailures;
using GGDeals.AddFailures.MVVM;
using GGDeals.Services;
using GGDeals.Settings;
using GGDeals.Settings.MVVM;
using GGDeals.Website;
using GGDeals.Website.Url;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace GGDeals
{
    public class GGDeals : GenericPlugin
    {
        private const string FailuresFileName = "failures.json";
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly IPlayniteAPI _api;
        private readonly StartupSettingsValidator _startupSettingsValidator;
        private readonly AddFailuresManager _addFailuresManager;

        private GGDealsSettingsViewModel _settings;

        public override Guid Id { get; } = Guid.Parse("2af05ded-085c-426b-a10e-8e03185092bf");

        public GGDeals(IPlayniteAPI api) : base(api)
        {
            _api = api;
            var failuresFilePath = Path.Combine(GetPluginUserDataPath(), FailuresFileName);
            var addFailuresFileService = new AddFailuresFileService(failuresFilePath);
            _addFailuresManager = new AddFailuresManager(addFailuresFileService);

            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            PlayniteApi.Database.Games.ItemCollectionChanged += (_, gamesAddedArgs) =>
            {
                AddGamesToGGCollection(gamesAddedArgs.AddedItems);
            };

            var pluginSettingsPersistence = new PluginSettingsPersistence(this);
            _startupSettingsValidator = new StartupSettingsValidator(pluginSettingsPersistence, new SettingsMigrator(pluginSettingsPersistence));
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            _startupSettingsValidator.EnsureCorrectVersionSettingsExist();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = ResourceProvider.GetString("LOC_GGDeals_GameMenuItemAddToGGDealsCollection"),
                Action = actionArgs => { AddGamesToGGCollection(actionArgs.Games); }
            };
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOC_GGDeals_MainMenuItemAddToGGDealsCollection"),
                MenuSection = "@GG.deals",
                Action = actionArgs => { AddGamesToGGCollection(_api.Database.Games.ToList()); }
            };

            yield return new MainMenuItem
            {
                Description = ResourceProvider.GetString("LOC_GGDeals_MainMenuItemShowFailures"),
                MenuSection = "@GG.deals",
                Action = actionArgs =>
                {
                    var window = _api.Dialogs.CreateWindow(new WindowCreationOptions()
                    {
                        ShowCloseButton = true,
                        ShowMaximizeButton = true,
                        ShowMinimizeButton = false,
                    });

                    window.Height = 768;
                    window.Width = 768;
                    window.Title = ResourceProvider.GetString("LOC_GGDeals_ShowAddFailuresTitle");

                    var showAddFailuresViewModel = new ShowAddFailuresViewModel(this, _addFailuresManager);
                    window.Content = new ShowAddFailuresView(showAddFailuresViewModel);

                    window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    window.ShowDialog();
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings ?? (_settings = new GGDealsSettingsViewModel(this));
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GGDealsSettingsView(PlayniteApi);
        }

        public void AddGamesToGGCollection(IReadOnlyCollection<Game> games)
        {
            Task.Run(async () =>
            {
                try
                {
                    using (var awaitableWebView = new AwaitableWebView(PlayniteApi.WebViews.CreateOffscreenView()))
                    {
                        var settings = LoadPluginSettings<GGDealsSettings>();
                        var homePageResolver = new HomePageResolver();
                        var gamePageUrlGuesser = new GamePageUrlGuesser(homePageResolver);
                        var libraryNameMap = new LibraryNameMap(PlayniteApi);
                        var ggWebsite = new GGWebsite(awaitableWebView, homePageResolver, gamePageUrlGuesser);
                        var homePage = new HomePage(awaitableWebView);
                        var gamePage = new GamePage(awaitableWebView, libraryNameMap);
                        var addAGameService = new AddAGameService(settings, ggWebsite, gamePage);

                        var ggDealsService = new GGDealsService(PlayniteApi, ggWebsite, homePage, addAGameService, _addFailuresManager);
                        await ggDealsService.AddGamesToLibrary(games);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to add games to GG.deals collection.");
                }
            });
        }
    }
}