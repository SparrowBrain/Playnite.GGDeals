using GGDeals.Menu.Failures;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using GGDeals.Settings;

namespace GGDeals.Services
{
	public class GGDealsService
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

		private readonly GGDealsSettings _settings;
		private readonly IPlayniteAPI _playniteApi;
		private readonly IAddGamesService _addGamesService;
		private readonly IAddFailuresManager _addFailuresManager;

		public GGDealsService(
			GGDealsSettings settings,
			IPlayniteAPI playniteApi,
			IAddGamesService addGamesService,
			IAddFailuresManager addFailuresManager)
		{
			_settings = settings;
			_playniteApi = playniteApi;
			_addGamesService = addGamesService;
			_addFailuresManager = addFailuresManager;
		}

		public async Task AddGamesToLibrary(IReadOnlyCollection<Game> games, CancellationToken ct)
		{
			var addedGames = new List<Guid>();
			var gamesWithoutPage = new List<Guid>();
			var alreadyOwnedGames = new List<Guid>();
			var skippedDueToLibrary = new List<Guid>();

			try
			{
				if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
				{
					throw new AuthenticationException("Authentication token is empty!");
				}

				var addResults = await _addGamesService.TryAddToCollection(games, ct);
				addedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Added).Select(r => r.Key).ToList();
				gamesWithoutPage = addResults.Where(r => r.Value.Result == AddToCollectionResult.PageNotFound).Select(r => r.Key).ToList();
				alreadyOwnedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.AlreadyOwned).Select(r => r.Key).ToList();
				skippedDueToLibrary = addResults.Where(r => r.Value.Result == AddToCollectionResult.SkippedDueToLibrary).Select(r => r.Key).ToList();

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
				await _addFailuresManager.AddFailures(gamesWithoutPage.ToDictionary(g => g,
					_ => new AddResult() { Result = AddToCollectionResult.PageNotFound }));
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
			await _addFailuresManager.AddFailures(unprocessedGames.ToDictionary(g => g.Id,
				_ => new AddResult() { Result = AddToCollectionResult.NotProcessed }));
		}
	}
}