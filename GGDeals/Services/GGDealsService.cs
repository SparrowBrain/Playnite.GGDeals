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
		private readonly Action _openFailureView;
		private readonly IPlayniteAPI _playniteApi;
		private readonly IAddGamesService _addGamesService;
		private readonly IAddFailuresManager _addFailuresManager;
		private readonly IGameStatusService _gameStatusService;

		public GGDealsService(
			GGDealsSettings settings,
			Action openFailureView,
			IPlayniteAPI playniteApi,
			IAddGamesService addGamesService,
			IAddFailuresManager addFailuresManager,
			IGameStatusService gameStatusService)
		{
			_settings = settings;
			_openFailureView = openFailureView;
			_playniteApi = playniteApi;
			_addGamesService = addGamesService;
			_addFailuresManager = addFailuresManager;
			_gameStatusService = gameStatusService;
		}

		public async Task AddGamesToLibrary(IReadOnlyCollection<Game> games, CancellationToken ct)
		{
			var addedGames = new Dictionary<Guid, AddResult>();
			var missedGames = new Dictionary<Guid, AddResult>();
			var alreadyOwnedGames = new Dictionary<Guid, AddResult>();
			var skippedDueToLibrary = new Dictionary<Guid, AddResult>();
			var ignoredGames = new Dictionary<Guid, AddResult>();
			var errorGames = new Dictionary<Guid, AddResult>();

			try
			{
				if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
				{
					throw new AuthenticationException("Authentication token is empty!");
				}

				var addResults = await _addGamesService.TryAddToCollection(games, ct);
				addedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Added).ToDictionary(x => x.Key, x => x.Value);
				missedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.NotFound).ToDictionary(x => x.Key, x => x.Value);
				alreadyOwnedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Synced).ToDictionary(x => x.Key, x => x.Value);
				skippedDueToLibrary = addResults.Where(r => r.Value.Result == AddToCollectionResult.SkippedDueToLibrary).ToDictionary(x => x.Key, x => x.Value);
				errorGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Error).ToDictionary(x => x.Key, x => x.Value);
				ignoredGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Ignored).ToDictionary(x => x.Key, x => x.Value);

				if (missedGames.Count > 0)
				{
					var message = new NotificationMessage(
						"gg-deals-gamepagenotfound",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGameMiss_Format"), missedGames.Count),
						NotificationType.Info,
						_openFailureView);
					_playniteApi.Notifications.Add(message);
				}

				if (errorGames.Count > 0)
				{
					var message = new NotificationMessage(
						"gg-deals-api-error",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationApiError_Format"), errorGames.Count),
						NotificationType.Info,
						_openFailureView);
					_playniteApi.Notifications.Add(message);
				}

				if (ignoredGames.Count > 0)
				{
					_playniteApi.Notifications.Add(
						"gg-deals-ignored",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGamesIgnored_Format"), ignoredGames.Count),
						NotificationType.Info);
				}
			}
			catch (AuthenticationException authEx)
			{
				Logger.Info(authEx, "User is not authenticated.");
				var message = new NotificationMessage(
					"gg-deals-auth-error",
					ResourceProvider.GetString("LOC_GGDeals_NotificationUserNotAuthenticatedLoginInAddonSettings"),
					NotificationType.Info,
					() => _playniteApi.MainView.OpenPluginSettings(Guid.Parse(GGDeals.PluginId))
				);
				_playniteApi.Notifications.Add(message);

				await AddUnprocessedGameFailures(games, addedGames, missedGames, errorGames, ignoredGames, skippedDueToLibrary, alreadyOwnedGames);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Error while trying to add games to library.");
				_playniteApi.Notifications.Add(
					"gg-deals-generic-error",
					ResourceProvider.GetString("LOC_GGDeals_NotificationFailedAddingGamesToLibrary"),
					NotificationType.Error
				);

				await AddUnprocessedGameFailures(games, addedGames, missedGames, errorGames, ignoredGames, skippedDueToLibrary, alreadyOwnedGames);
			}

			Logger.Info($@"Finished adding games to GG.deals collection: Total: {games.Count},
{nameof(AddToCollectionResult.NotFound)}: {missedGames.Count},
AlreadyOwned: {alreadyOwnedGames.Count},
SkippedDueToLibrary: {skippedDueToLibrary.Count},
Ignored: {ignoredGames.Count},
Added: {addedGames.Count}");

			await HandleFailuresWithStatusChange(games, missedGames);
			await HandleFailuresWithoutStatusChange(errorGames);
			await HandleNonFailuresWithStatusChange(games, addedGames);
			await HandleNonFailuresWithStatusChange(games, alreadyOwnedGames);
			await HandleNonFailuresWithStatusChange(games, ignoredGames);
			await HandleNonFailuresWithoutStatusChange(skippedDueToLibrary);
		}

		private async Task AddUnprocessedGameFailures(IReadOnlyCollection<Game> games, params IDictionary<Guid, AddResult>[] results)
		{
			var unprocessedGames = games
				.Where(g => !results.SelectMany(r => r.Keys).Contains(g.Id))
				.ToList();
			await _addFailuresManager.AddFailures(unprocessedGames.ToDictionary(g => g.Id,
				_ => new AddResult() { Result = AddToCollectionResult.New }));
		}

		private async Task HandleFailuresWithoutStatusChange(Dictionary<Guid, AddResult> results)
		{
			if (results.Count > 0)
			{
				await _addFailuresManager.AddFailures(results);
			}
		}

		private async Task HandleNonFailuresWithoutStatusChange(Dictionary<Guid, AddResult> results)
		{
			if (results.Count > 0)
			{
				await _addFailuresManager.RemoveFailures(results.Keys);
			}
		}

		private async Task HandleFailuresWithStatusChange(IReadOnlyCollection<Game> games, Dictionary<Guid, AddResult> results)
		{
			if (results.Count > 0)
			{
				foreach (var result in results)
				{
					_gameStatusService.UpdateStatus(games.First(g => g.Id == result.Key), result.Value.Result);
				}

				await _addFailuresManager.AddFailures(results);
			}
		}

		private async Task HandleNonFailuresWithStatusChange(IReadOnlyCollection<Game> games, Dictionary<Guid, AddResult> results)
		{
			if (results.Count > 0)
			{
				foreach (var result in results)
				{
					_gameStatusService.UpdateStatus(games.First(g => g.Id == result.Key), result.Value.Result);
				}

				await _addFailuresManager.RemoveFailures(results.Keys);
			}
		}
	}
}