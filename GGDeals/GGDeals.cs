using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using GGDeals.Services;
using GGDeals.Website;
using GGDeals.Website.Url;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;

namespace GGDeals
{
    public class GgDeals : GenericPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly GGDealsService _ggDealsService;

        private GGDealsSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("2af05ded-085c-426b-a10e-8e03185092bf");

        public GgDeals(IPlayniteAPI api) : base(api)
        {
            Settings = new GGDealsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };

            var awaitableWebView = new AwaitableWebView(PlayniteApi.WebViews.CreateOffscreenView());
            var homePageResolver = new HomePageResolver();
            var gamePageUrlGuesser = new GamePageUrlGuesser(homePageResolver);
            var libraryNameMap = new LibraryNameMap(PlayniteApi);
            var ggWebsite = new GGWebsite(homePageResolver, gamePageUrlGuesser, awaitableWebView);
            var gamePage = new GamePage(awaitableWebView, libraryNameMap);
            var addAGameService = new AddAGameService(ggWebsite, gamePage);
            _ggDealsService = new GGDealsService(PlayniteApi, ggWebsite, addAGameService);
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOC_GGDeals_GameMenuItemAddToGGDealsCollection"),
                    Action = actionArgs =>
                    {
                        Task.Run(async () =>
                        {
                           await _ggDealsService.AddGamesToLibrary(actionArgs.Games);
                        });
                    }
                }
            };
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GGDealsSettingsView(PlayniteApi);
        }
    }
}