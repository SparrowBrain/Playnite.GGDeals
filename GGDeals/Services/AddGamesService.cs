using GGDeals.Api.Models;
using GGDeals.Api.Services;
using GGDeals.Settings;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GGDeals.Services
{
	public class AddGamesService : IAddGamesService
	{
		private readonly GGDealsSettings _settings;
		private readonly IGameToAddFilter _gameToAddFilter;
		private readonly IGameToGameWithLauncherConverter _gameToGameWithLauncherConverter;
		private readonly IRequestDataBatcher _requestDataBatcher;
		private readonly IGGDealsApiClient _ggDealsApiClient;

		public AddGamesService(
			GGDealsSettings settings,
			IGameToAddFilter gameToAddFilter,
			IGameToGameWithLauncherConverter gameToGameWithLauncherConverter,
			IRequestDataBatcher requestDataBatcher,
			IGGDealsApiClient ggDealsApiClient)
		{
			_settings = settings;
			_gameToAddFilter = gameToAddFilter;
			_gameToGameWithLauncherConverter = gameToGameWithLauncherConverter;
			_requestDataBatcher = requestDataBatcher;
			_ggDealsApiClient = ggDealsApiClient;
		}

		public async Task<IDictionary<Guid, AddResult>> TryAddToCollection(IReadOnlyCollection<Game> games, CancellationToken ct)
		{
			var result = new Dictionary<Guid, AddResult>();
			var gamesToProcess = new List<GameWithLauncher>();
			foreach (var game in games)
			{
				if (!_gameToAddFilter.ShouldTryAddGame(game, out var addResult))
				{
					result.Add(game.Id, addResult);
				}
				else
				{
					var gameWithLauncher = _gameToGameWithLauncherConverter.GetGameWithLauncher(game);
					gamesToProcess.Add(gameWithLauncher);
				}
			}

			var requests = _requestDataBatcher.CreateDataJsons(gamesToProcess)
				.Select(data => new ImportRequest() { Data = data, Token = _settings.AuthenticationToken });

			foreach (var request in requests)
			{
				var response = await _ggDealsApiClient.ImportGames(request, ct);
				foreach (var item in response.Data.Result)
				{
					var addToCollectionResult = MapToAddToCollectionResult(item);
					result.Add(item.Id, new AddResult() { Result = addToCollectionResult, Message = item.Message });
				}
			}

			return result;
		}

		private static AddToCollectionResult MapToAddToCollectionResult(ImportResult item)
		{
			AddToCollectionResult addToCollectionResult;
			switch (item.Status)
			{
				case ImportResultStatus.Error:
					addToCollectionResult = AddToCollectionResult.Error;
					break;

				case ImportResultStatus.Added:
					addToCollectionResult = AddToCollectionResult.Added;
					break;

				case ImportResultStatus.Skipped:
					addToCollectionResult = AddToCollectionResult.AlreadyOwned;
					break;

				case ImportResultStatus.Miss:
					addToCollectionResult = AddToCollectionResult.Missed;
					break;

				default:
					throw new Exception("No mapping between ImportResultStatus and AddToCollectionResult");
			}

			return addToCollectionResult;
		}
	}
}