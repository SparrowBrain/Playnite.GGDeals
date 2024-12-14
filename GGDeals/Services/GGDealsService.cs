using GGDeals.Api.Models;
using GGDeals.Menu.Failures;
using GGDeals.Models;
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
		private readonly IAddResultProcessor _addResultProcessor;

		public GGDealsService(
			GGDealsSettings settings,
			Action openFailureView,
			IPlayniteAPI playniteApi,
			IAddGamesService addGamesService,
			IAddFailuresManager addFailuresManager,
			IAddResultProcessor addResultProcessor)
		{
			_settings = settings;
			_openFailureView = openFailureView;
			_playniteApi = playniteApi;
			_addGamesService = addGamesService;
			_addFailuresManager = addFailuresManager;
			_addResultProcessor = addResultProcessor;
		}

		public async Task AddGamesToLibrary(IReadOnlyCollection<Game> games,
			Action<float> reportProgress,
			CancellationToken ct)
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

				var addResults = await _addGamesService.TryAddToCollection(games, reportProgress, ct);
				addedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Added)
					.ToDictionary(x => x.Key, x => x.Value);
				missedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.NotFound)
					.ToDictionary(x => x.Key, x => x.Value);
				alreadyOwnedGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Synced)
					.ToDictionary(x => x.Key, x => x.Value);
				skippedDueToLibrary = addResults.Where(r => r.Value.Result == AddToCollectionResult.SkippedDueToLibrary)
					.ToDictionary(x => x.Key, x => x.Value);
				errorGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Error)
					.ToDictionary(x => x.Key, x => x.Value);
				ignoredGames = addResults.Where(r => r.Value.Result == AddToCollectionResult.Ignored)
					.ToDictionary(x => x.Key, x => x.Value);

				if (missedGames.Count > 0)
				{
					var message = new NotificationMessage(
						"gg-deals-gamepagenotfound",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGameMiss_Format"),
							missedGames.Count),
						NotificationType.Info,
						_openFailureView);
					_playniteApi.Notifications.Add(message);
				}

				if (errorGames.Count > 0)
				{
					var message = new NotificationMessage(
						"gg-deals-api-error",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationApiError_Format"),
							errorGames.Count),
						NotificationType.Info,
						_openFailureView);
					_playniteApi.Notifications.Add(message);
				}

				if (ignoredGames.Count > 0)
				{
					_playniteApi.Notifications.Add(
						"gg-deals-ignored",
						string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGamesIgnored_Format"),
							ignoredGames.Count),
						NotificationType.Info);
				}

				if (addedGames.Count > 0
					|| alreadyOwnedGames.Count > 0)
				{
					var addedGamesMessage = string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGamesAdded_Format"),
						addedGames.Count);
					var alreadyOwnedMessage = string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationGamesAlreadyInCollection_Format"),
						alreadyOwnedGames.Count);

					_playniteApi.Notifications.Add(
						"gg-deals-added",
						alreadyOwnedGames.Count == 0
						? addedGamesMessage
						: $"{addedGamesMessage} {alreadyOwnedMessage}",
						NotificationType.Info);
				}

				_addResultProcessor.Process(games, addResults);
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

				await AddUnprocessedGameFailures(games, ResourceProvider.GetString("LOC_GGDeals_ReasonNotAuthenticated"), addedGames, missedGames, errorGames, ignoredGames,
					skippedDueToLibrary, alreadyOwnedGames);
			}
			catch (ApiException apiEx)
			{
				Logger.Error(apiEx, "Error while trying to add games to library.");
				_playniteApi.Notifications.Add(
					"gg-deals-api-error-message",
					string.Format(ResourceProvider.GetString("LOC_GGDeals_NotificationApiErrorMessage_Format"), apiEx.Message),
					NotificationType.Error
				);

				await AddUnprocessedGameFailures(games, apiEx.Message, addedGames, missedGames, errorGames, ignoredGames,
					skippedDueToLibrary, alreadyOwnedGames);
			}
			catch (OperationCanceledException)
			{
				await AddUnprocessedGameFailures(games, ResourceProvider.GetString("LOC_GGDeals_ReasonCancelled"), addedGames, missedGames, errorGames, ignoredGames, skippedDueToLibrary, alreadyOwnedGames);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Error while trying to add games to library.");
				_playniteApi.Notifications.Add(
					"gg-deals-generic-error",
					ResourceProvider.GetString("LOC_GGDeals_NotificationFailedAddingGamesToLibrary"),
					NotificationType.Error
				);

				await AddUnprocessedGameFailures(games, ex.Message, addedGames, missedGames, errorGames, ignoredGames, skippedDueToLibrary, alreadyOwnedGames);
			}

			Logger.Info($@"Finished adding games to GG.deals collection: Total: {games.Count},
{nameof(AddToCollectionResult.NotFound)}: {missedGames.Count},
AlreadyOwned: {alreadyOwnedGames.Count},
SkippedDueToLibrary: {skippedDueToLibrary.Count},
Ignored: {ignoredGames.Count},
Added: {addedGames.Count}");

			await HandleFailures(missedGames);
			await HandleFailures(errorGames);
			await HandleNonFailures(addedGames);
			await HandleNonFailures(alreadyOwnedGames);
			await HandleNonFailures(ignoredGames);
			await HandleNonFailures(skippedDueToLibrary);
		}

		private async Task AddUnprocessedGameFailures(IReadOnlyCollection<Game> games, string message, params IDictionary<Guid, AddResult>[] results)
		{
			var unprocessedGames = games
				.Where(g => !results.SelectMany(r => r.Keys).Contains(g.Id))
				.ToList();
			await _addFailuresManager.AddFailures(unprocessedGames.ToDictionary(g => g.Id,
				_ => new AddResult() { Result = AddToCollectionResult.New, Message = message }));
		}

		private async Task HandleFailures(Dictionary<Guid, AddResult> results)
		{
			if (results.Count > 0)
			{
				await _addFailuresManager.AddFailures(results);
			}
		}

		private async Task HandleNonFailures(Dictionary<Guid, AddResult> results)
		{
			if (results.Count > 0)
			{
				await _addFailuresManager.RemoveFailures(results.Keys);
			}
		}
	}
}