using GGDeals.Settings;
using GGDeals.Website;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Threading.Tasks;

namespace GGDeals.Services
{
	public class AddAGameService : IAddAGameService
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

		private readonly GGDealsSettings _settings;
		private readonly IGGWebsite _ggWebsite;
		private readonly IGamePage _gamePage;

		public AddAGameService(GGDealsSettings settings, IGGWebsite ggWebsite, IGamePage gamePage)
		{
			_settings = settings;
			_ggWebsite = ggWebsite;
			_gamePage = gamePage;
		}

		public async Task<AddToCollectionResult> TryAddToCollection(Game game)
		{
			if (game.PluginId == Guid.Empty && !_settings.SyncPlayniteLibrary)
			{
				Logger.Debug($"Skipped due to Playnite library: {{ Id: {game.Id}, Name: {game.Name} }}.");
				return AddToCollectionResult.SkippedDueToLibrary;
			}

			if (_settings.LibrariesToSkip.Contains(game.PluginId))
			{
				Logger.Debug($"Skipped due to library: {{ Id: {game.Id}, Name: {game.Name} }}.");
				return AddToCollectionResult.SkippedDueToLibrary;
			}

			if (!await _ggWebsite.TryNavigateToGamePage(game))
			{
				Logger.Info($"Could not add game {{ Id: {game.Id}, Name: {game.Name} }}. Most likely failed to guess the game page url.");
				return AddToCollectionResult.PageNotFound;
			}

			await _gamePage.ClickOwnItButton();
			await _gamePage.ExpandDrmDropDown();
			var isActive = await _gamePage.IsDrmPlatformCheckboxActive(game);
			if (isActive)
			{
				Logger.Debug($"Already owned: {{ Id: {game.Id}, Name: {game.Name} }}.");
				return AddToCollectionResult.AlreadyOwned;
			}

			await _gamePage.ClickDrmPlatformCheckBox(game);
			await _gamePage.ClickSubmitOwnItForm();

			Logger.Info($"Added to GG.deals collection: {{ Id: {game.Id}, Name: {game.Name} }}.");
			return AddToCollectionResult.Added;
		}
	}
}