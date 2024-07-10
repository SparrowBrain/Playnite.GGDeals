using GGDeals.Menu.Failures;
using GGDeals.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

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
			var addedGames = new Dictionary<Guid, AddResult>();
			var missedGames = new Dictionary<Guid, AddResult>();
			var alreadyOwnedGames = new Dictionary<Guid, AddResult>();
			var skippedDueToLibrary = new Dictionary<Guid, AddResult>();
			var errorGames = new Dictionary<Guid, AddResult>();

			try
			{
				if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
				{
					throw new AuthenticationException("Authentication token is empty!");
				}

				var addResults = await _addGamesService.TryAddToCollection(games, ct);
				addedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Added).ToDictionary(x => x.Key, x => x.Value);
				missedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Missed).ToDictionary(x => x.Key, x => x.Value);
				alreadyOwnedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.AlreadyOwned).ToDictionary(x => x.Key, x => x.Value);
				skippedDueToLibrary = addResults.Where(r => r.Value.Result == AddToCollectionResult.SkippedDueToLibrary).ToDictionary(x => x.Key, x => x.Value);
				errorGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Error).ToDictionary(x => x.Key, x => x.Value);

				if (missedGames.Count > 0)
				{
					_playniteApi.Notifications.Add(
						"gg-deals-gamepagenotfound",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGameMiss_Format"), missedGames.Count),
						NotificationType.Info);
				}

				if (errorGames.Count > 0)
				{
					_playniteApi.Notifications.Add(
						"gg-deals-api-error",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationApiError_Format"), errorGames.Count),
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

				await AddUnprocessedGameFailures(games, addedGames, missedGames, errorGames);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Error while trying to add games to library.");
				_playniteApi.Notifications.Add(
					"gg-deals-generic-error",
					ResourceProvider.GetString("LOC_GGDeals_NotificationFailedAddingGamesToLibrary"),
					NotificationType.Error);

				await AddUnprocessedGameFailures(games, addedGames, missedGames, errorGames);
			}

			Logger.Info($"Finished adding games to GG.deals collection: Total: {games.Count}, PageNotFound: {missedGames.Count}, AlreadyOwned: {alreadyOwnedGames.Count}, SkippedDueToLibrary: {skippedDueToLibrary.Count}, Added: {addedGames.Count}");

			if (missedGames.Count > 0)
			{
				await _addFailuresManager.AddFailures(missedGames);
			}

			if (errorGames.Count > 0)
			{
				await _addFailuresManager.AddFailures(errorGames);
			}

			if (addedGames.Count > 0)
			{
				await _addFailuresManager.RemoveFailures(addedGames.Keys);
			}

			if (skippedDueToLibrary.Count > 0)
			{
				await _addFailuresManager.RemoveFailures(skippedDueToLibrary.Keys);
			}

			if (alreadyOwnedGames.Count > 0)
			{
				await _addFailuresManager.RemoveFailures(alreadyOwnedGames.Keys);
			}
		}

		private async Task AddUnprocessedGameFailures(IReadOnlyCollection<Game> games, IDictionary<Guid, AddResult> addedGames, IDictionary<Guid, AddResult> missedGames, IDictionary<Guid, AddResult> errorGames)
		{
			var unprocessedGames = games
				.Where(g => !addedGames.Keys.Contains(g.Id))
				.Where(g => !missedGames.Keys.Contains(g.Id))
				.Where(g => !errorGames.Keys.Contains(g.Id))
				.ToList();
			await _addFailuresManager.AddFailures(unprocessedGames.ToDictionary(g => g.Id,
				_ => new AddResult() { Result = AddToCollectionResult.NotProcessed }));
		}
	}
}