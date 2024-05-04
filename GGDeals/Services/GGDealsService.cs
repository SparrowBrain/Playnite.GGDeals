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
        private readonly IHomePage _homePage;
        private readonly IAddAGameService _addAGameService;

        public GGDealsService(
            IPlayniteAPI playniteApi,
            IGGWebsite ggWebsite, 
            IHomePage homePage,
            IAddAGameService addAGameService)
        {
            _playniteApi = playniteApi;
            _ggWebsite = ggWebsite;
            _homePage = homePage;
            _addAGameService = addAGameService;
        }

        public async Task AddGamesToLibrary(IReadOnlyCollection<Game> games)
        {
            var gamesWithoutPage = new List<Game>();
            var alreadyOwnedGames = new List<Game>();

            try
            {
                await _ggWebsite.NavigateToHomePage();
                if (!await _homePage.IsUserLoggedIn())
                {
                    throw new AuthenticationException("User is not logged in!");
                }

                foreach (var game in games)
                {
                    var addedResult = await _addAGameService.TryAddToCollection(game);
                    if (addedResult == AddToCollectionResult.PageNotFound)
                    {
                        gamesWithoutPage.Add(game);
                    }
                    if (addedResult == AddToCollectionResult.AlreadyOwned)
                    {
                        alreadyOwnedGames.Add(game);
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
                Logger.Info(authEx, "User is not authenticated.");
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

            Logger.Info($"Finished adding games to GG.deals collection: Total: {games.Count}, PageNotFound: {gamesWithoutPage.Count}, AlreadyOwned: {alreadyOwnedGames.Count}, Added: {games.Count - gamesWithoutPage.Count - alreadyOwnedGames.Count}");
        }
    }
}