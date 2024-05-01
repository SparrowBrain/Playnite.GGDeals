using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using GGDeals.Services;
using GGDeals.Settings;
using GGDeals.Website;
using GGDeals.Website.Url;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace GGDeals
{
    public class GgDeals : GenericPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private GGDealsSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("2af05ded-085c-426b-a10e-8e03185092bf");

        public GgDeals(IPlayniteAPI api) : base(api)
        {
            Settings = new GGDealsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            PlayniteApi.Database.Games.ItemCollectionChanged += (_, gamesAddedArgs) =>
            {
                AddGamesToGGCollection(gamesAddedArgs.AddedItems);
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOC_GGDeals_GameMenuItemAddToGGDealsCollection"),
                    Action = actionArgs => { AddGamesToGGCollection(actionArgs.Games); }
                }
            };
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GGDealsSettingsView(PlayniteApi);
        }

        private void AddGamesToGGCollection(IReadOnlyCollection<Game> games)
        {
            Task.Run(async () =>
            {
                using (var awaitableWebView = new AwaitableWebView(PlayniteApi.WebViews.CreateOffscreenView()))
                {
                    var homePageResolver = new HomePageResolver();
                    var gamePageUrlGuesser = new GamePageUrlGuesser(homePageResolver);
                    var libraryNameMap = new LibraryNameMap(PlayniteApi);
                    var ggWebsite = new GGWebsite(homePageResolver, gamePageUrlGuesser, awaitableWebView);
                    var gamePage = new GamePage(awaitableWebView, libraryNameMap);
                    var addAGameService = new AddAGameService(ggWebsite, gamePage);
                    var ggDealsService = new GGDealsService(PlayniteApi, ggWebsite, addAGameService);
                    await ggDealsService.AddGamesToLibrary(games);
                }
            });
        }
    }
}