using System;
using GGDeals.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public class GameToAddFilter : IGameToAddFilter
	{
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly GGDealsSettings _settings;
		private readonly IGameStatusService _gameStatusService;

		public GameToAddFilter(
			GGDealsSettings settings,
			IGameStatusService gameStatusService)
		{
			_settings = settings;
			_gameStatusService = gameStatusService;
		}

		public bool ShouldTryAddGame(Game game, out AddResult status)
		{
			if (game.PluginId == Guid.Empty && !_settings.SyncPlayniteLibrary)
			{
				Logger.Debug($"Skipped due to Playnite library: {{ Id: {game.Id}, Name: {game.Name} }}.");
				status = new AddResult() { Result = AddToCollectionResult.SkippedDueToLibrary };
				return false;
			}

			if (_settings.LibrariesToSkip.Contains(game.PluginId))
			{
				Logger.Debug($"Skipped due to library: {{ Id: {game.Id}, Name: {game.Name} }}.");
				status = new AddResult() { Result = AddToCollectionResult.SkippedDueToLibrary };
				return false;
			}

			var gameStatus = _gameStatusService.GetStatus(game);
			switch (gameStatus)
			{
				case AddToCollectionResult.Ignored:
					Logger.Debug($"Skipped due to ignored status: {{ Id: {game.Id}, Name: {game.Name} }}.");
					status = new AddResult() { Result = AddToCollectionResult.Ignored };
					return false;

				case AddToCollectionResult.Synced:
					Logger.Debug($"Skipped due to already owned status: {{ Id: {game.Id}, Name: {game.Name} }}.");
					status = new AddResult() { Result = AddToCollectionResult.Synced };
					return false;

				case AddToCollectionResult.New:
				case AddToCollectionResult.NotFound:
					status = null;
					return true;

				default:
					throw new ArgumentOutOfRangeException($"Unexpected game sync status {gameStatus.ToString()} in filter.");
			}
		}
	}
}