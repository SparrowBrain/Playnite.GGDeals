using System;
using System.Collections.Generic;
using System.Linq;
using GGDeals.Models;
using GGDeals.Settings;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public class AddResultProcessor : IAddResultProcessor
	{
		private readonly GGDealsSettings _settings;
		private readonly IGameStatusService _gameStatusService;
		private readonly IAddLinkService _addLinkService;

		public AddResultProcessor(
			GGDealsSettings settings,
			IGameStatusService gameStatusService,
			IAddLinkService addLinkService)
		{
			_settings = settings;
			_gameStatusService = gameStatusService;
			_addLinkService = addLinkService;
		}

		public void Process(IReadOnlyCollection<Game> games, IDictionary<Guid, AddResult> results)
		{
			foreach (var addResult in results)
			{
				var game = games.Single(g => g.Id == addResult.Key);

				UpdateStatus(game, addResult.Value.Result);
				AddLink(game, addResult.Value.Url);
			}
		}

		private void UpdateStatus(Game game, AddToCollectionResult addToCollectionResult)
		{
            if (!_settings.AddTagsToGames)
            {
                return;
            }

			switch (addToCollectionResult)
			{
				case AddToCollectionResult.Error:
				case AddToCollectionResult.SkippedDueToLibrary:
					return;

				case AddToCollectionResult.Added:
				case AddToCollectionResult.Synced:
				case AddToCollectionResult.NotFound:
				case AddToCollectionResult.Ignored:
					_gameStatusService.UpdateStatus(game, addToCollectionResult);
					break;

				case AddToCollectionResult.New:
				default:
					throw new Exception($"Not configured AddToCollectionResult {addToCollectionResult} while processing status.");
			}
		}

		private void AddLink(Game game, string url)
		{
			if (_settings.AddLinksToGames && !string.IsNullOrEmpty(url))
			{
				_addLinkService.AddLink(game, url);
			}
		}
	}
}