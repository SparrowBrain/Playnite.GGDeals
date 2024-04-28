using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using GGDeals.Website;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
    public class GGDealsService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly IPlayniteAPI _playniteApi;
        private readonly IGGWebsite _ggWebsite;
        private readonly IAddAGameService _addAGameService;

        public GGDealsService(IPlayniteAPI playniteApi, IGGWebsite ggWebsite, IAddAGameService addAGameService)
        {
            _playniteApi = playniteApi;
            _ggWebsite = ggWebsite;
            _addAGameService = addAGameService;
        }

        public async Task AddGamesToLibrary(IReadOnlyCollection<Game> games)
        {
            try
            {
                await _ggWebsite.CheckLoggedIn();
                var gamesWithoutPage = new List<Game>();
                foreach (var game in games)
                {
                    var added = await _addAGameService.TryAddToCollection(game);
                    if (!added)
                    {
                        Logger.Info($"Could not add game {{ Id: {game.Id}, Name: {game.Name} }}. Most likely failed to guess the game page url.");
                        gamesWithoutPage.Add(game);
                    }
                }

                if (gamesWithoutPage.Count > 0)
                {
                    _playniteApi.Notifications.Add(
                        "gg-deals-gamepagenotfound",
                        string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGamePageNotFoundFormat"), gamesWithoutPage.Count),
                        NotificationType.Info);
                }
            }
            catch (AuthenticationException authEx)
            {
                Logger.Info(authEx, "User not authenticated.");
                _playniteApi.Notifications.Add(
                    "gg-deals-auth-error",
                    ResourceProvider.GetString("LOC_GGDeals_NotificationUserNotAuthenticatedLoginInAddonSettings"),
                    NotificationType.Info);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while trying to add games to library.");
                _playniteApi.Notifications.Add(
                    "gg-deals-generic-error",
                    ResourceProvider.GetString("LOC_GGDeals_NotificationFailedAddingGamesToLibrary"),
                    NotificationType.Error);
            }
        }
    }
}