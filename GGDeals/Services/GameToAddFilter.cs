using GGDeals.Models;
using GGDeals.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;

namespace GGDeals.Services
{
	public class GameToAddFilter : IGameToAddFilter
	{
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly GGDealsSettings _settings;
		private readonly IGameStatusService _gameStatusService;
		private readonly SyncRunSettings _syncRunSettings;

		public GameToAddFilter(GGDealsSettings settings, IGameStatusService gameStatusService, SyncRunSettings syncRunSettings)
		{
			_settings = settings;
			_gameStatusService = gameStatusService;
			_syncRunSettings = syncRunSettings;
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
			if (!_syncRunSettings.StatusesToSync.Contains(gameStatus))
			{
				Logger.Debug($"Skipped due to status: {{ Id: {game.Id}, Name: {game.Name}, Status: {gameStatus} }}.");
				status = new AddResult() { Result = gameStatus };
				return false;
			}

			status = null;
			return true;
		}
	}
}