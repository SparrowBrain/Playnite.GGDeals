using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using GGDeals.AddFailures;
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
        private readonly IAddFailuresManager _addFailuresManager;

        public GGDealsService(
            IPlayniteAPI playniteApi,
            IGGWebsite ggWebsite,
            IHomePage homePage,
            IAddAGameService addAGameService,
            IAddFailuresManager addFailuresManager)
        {
            _playniteApi = playniteApi;
            _ggWebsite = ggWebsite;
            _homePage = homePage;
            _addAGameService = addAGameService;
            _addFailuresManager = addFailuresManager;
        }

        public async Task AddGamesToLibrary(IReadOnlyCollection<Game> games)
        {
            var addedGames = new List<Guid>();
            var gamesWithoutPage = new List<Guid>();
            var alreadyOwnedGames = new List<Guid>();
            var skippedDueToLibrary = new List<Guid>();

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
                    switch (addedResult)
                    {
                        case AddToCollectionResult.Added:
                            addedGames.Add(game.Id);
                            break;

                        case AddToCollectionResult.PageNotFound:
                            gamesWithoutPage.Add(game.Id);
                            break;

                        case AddToCollectionResult.AlreadyOwned:
                            alreadyOwnedGames.Add(game.Id);
                            break;

                        case AddToCollectionResult.SkippedDueToLibrary:
                            skippedDueToLibrary.Add(game.Id);
                            break;

                        default:
                            throw new NotImplementedException("Unimplemented add to collection result processing!");
                    }
                }

                if (gamesWithoutPage.Count > 0)
                {
                    _playniteApi.Notifications.Add(
                        "gg-deals-gamepagenotfound",
                        string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGamePageNotFound_Format"), gamesWithoutPage.Count),
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

                await AddUnprocessedGameFailures(games, addedGames, gamesWithoutPage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while trying to add games to library.");
                _playniteApi.Notifications.Add(
                    "gg-deals-generic-error",
                    ResourceProvider.GetString("LOC_GGDeals_NotificationFailedAddingGamesToLibrary"),
                    NotificationType.Error);

                await AddUnprocessedGameFailures(games, addedGames, gamesWithoutPage);
            }

            Logger.Info($"Finished adding games to GG.deals collection: Total: {games.Count}, PageNotFound: {gamesWithoutPage.Count}, AlreadyOwned: {alreadyOwnedGames.Count}, SkippedDueToLibrary: {skippedDueToLibrary.Count}, Added: {addedGames.Count}");

            if (gamesWithoutPage.Count > 0)
            {
                await _addFailuresManager.AddFailures(gamesWithoutPage.ToDictionary(g => g, _ => AddToCollectionResult.PageNotFound));
            }

            if (addedGames.Count > 0)
            {
                await _addFailuresManager.RemoveFailures(addedGames);
            }

            if (skippedDueToLibrary.Count > 0)
            {
                await _addFailuresManager.RemoveFailures(skippedDueToLibrary);
            }

            if (alreadyOwnedGames.Count > 0)
            {
                await _addFailuresManager.RemoveFailures(alreadyOwnedGames);
            }
        }

        private async Task AddUnprocessedGameFailures(IReadOnlyCollection<Game> games, List<Guid> addedGames, List<Guid> gamesWithoutPage)
        {
            var unprocessedGames = games
                .Where(g => !addedGames.Contains(g.Id))
                .Where(g => !gamesWithoutPage.Contains(g.Id))
                .ToList();
            await _addFailuresManager.AddFailures(unprocessedGames.ToDictionary(g => g.Id, _ => AddToCollectionResult.NotProcessed));
        }
    }
}