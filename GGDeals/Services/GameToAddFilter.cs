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

		public GameToAddFilter(GGDealsSettings settings)
		{
			_settings = settings;
		}

		public bool ShouldTryAddGame(Game game, out AddToCollectionResult? status)
		{
			if (game.PluginId == Guid.Empty && !_settings.SyncPlayniteLibrary)
			{
				Logger.Debug($"Skipped due to Playnite library: {{ Id: {game.Id}, Name: {game.Name} }}.");
				status = AddToCollectionResult.SkippedDueToLibrary;
				return false;
			}

			if (_settings.LibrariesToSkip.Contains(game.PluginId))
			{
				Logger.Debug($"Skipped due to library: {{ Id: {game.Id}, Name: {game.Name} }}.");
				status = AddToCollectionResult.SkippedDueToLibrary;
				return false;
			}

			status = null;
			return true;

		}
	}
}