using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace GGDeals
{
    public class GgDeals : GenericPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private AwaitableWebView _webView;

        private GGDealsSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("2af05ded-085c-426b-a10e-8e03185092bf");

        public GgDeals(IPlayniteAPI api) : base(api)
        {
            Settings = new GGDealsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };

            _webView = new AwaitableWebView(PlayniteApi.WebViews.CreateOffscreenView());
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = "Add to GG.deals collection",
                    Action = games =>
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await _webView.Navigate("https://gg.deals/");

                                // Check we're logged in
                                var aaa = await RunScript<int>(@"$("".login"").children(""a"").length");
                                Logger.Info(aaa.ToString());
                                if (aaa != 0)
                                {
                                    Logger.Error("We're not logged in");
                                    return;
                                }

                                foreach (var game in games.Games)
                                {
                                    await AddToCollection(game);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Error while trying to add games to GG.deals collection");
                            }
                        });
                    }
                }
            };
        }

        private async Task AddToCollection(Game game)
        {
            var libraryPlugin = PlayniteApi.Addons.Plugins.FirstOrDefault(x => x.Id == game.PluginId) as LibraryPlugin;
            var playniteLibraryName = libraryPlugin != null
                ? libraryPlugin.Name
                : game.Source.Name;

            var libraryMapping = new Dictionary<string, string>()
            {
                { "Steam", "Steam" },
                { "EA app", "EA App" },
                { "Ubisoft Connect", "Ubisoft Connect" },
                { "GOG", "GOG" },
                { "Epic", "Epic Games Launcher" },
                { "Xbox", "Microsoft Store" },
                { "Battle.net", "Battle.net" },
                { "Rockstar Games", "Rockstar Games Launcher" },
                { "Amazon Games", "Prime Gaming"},
                { "PlayStation", "PlayStation Network"},
                { "Nintendo", "Nintendo eShop"},
                { "itch.io", "Itch.io"},
            };

            if (!libraryMapping.TryGetValue(playniteLibraryName, out var ggLibraryName))
            {
                ggLibraryName = "Other";
            }

            //////// Enter into global search
            //////await RunScript(@"$(""#global-search-input"").text(""Outer Wilds"")");

            //////// Submit search
            //////await RunScript(@"$(""#global-search-form"").find("".search-submit"").click()");

            //////// Get first result title
            //////var title = await RunScript<string>(@"$(""a.game-info-title"").first().text()");
            //////if (title != "Outer Wilds")
            //////{
            //////    logger.Error("Did not find Outer Wilds");
            //////    return;
            //////}

            //////// Navigate to game page
            //////await RunScript(@"window.location = $(""a.game-info-title"").first().attr(""href"")");

            var gameTitle = game.Name;
            var gamePage = gameTitle
                .ToLower()
                .Replace(":", "")
                .Replace(" - ", "-")
                .Replace(" ", "-");

            // Navigate to game page
            await _webView.Navigate($"https://gg.deals/game/{gamePage}/");

            // Check not 404 page
            var gameNotFound = await RunScript<int>(@"$("".error-404"").length");
            Logger.Info(gameNotFound.ToString());
            if (gameNotFound != 0)
            {
                Logger.Error($"Game page {gamePage} not found");
                return;
            }

            // Open "Own It" modal form
            await _webView.Click(@"$("".owned-game.game-action-wrap "").first().find("".activate"")");

            // Show DRM dropdown
            await _webView.Click(@"$(""#drm-collapse"").find(""a"").first()");

            // Select launchers
            await _webView.Click($@"$(""#drm-collapse"").find("".filter-switch"").filter(""[data-name='{ggLibraryName}']"")");

            await Task.Delay(1000);

            // Submit add to collection form
            await _webView.Click(@"$(""button[type='submit']"").filter("".btn"")");
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

        private async Task<T> RunScript<T>(string script)
        {
            var bbb = await _webView.EvaluateScriptAsync(script);
            Logger.Info($"{script}: {bbb.Success}");
            return (T)bbb.Result;
        }

        private async Task RunScript(string script)
        {
            var bbb = await _webView.EvaluateScriptAsync(script);
            Logger.Info($"{script}: {bbb.Success}");
        }
    }
}