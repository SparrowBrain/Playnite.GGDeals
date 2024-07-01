using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public class AddGamesService : IAddGamesService
	{
		private readonly IGameToAddFilter _gameToAddFilter;

		public AddGamesService(IGameToAddFilter gameToAddFilter)
		{
			_gameToAddFilter = gameToAddFilter;
		}

		public async Task<IDictionary<Guid, AddToCollectionResult>> TryAddToCollection(IReadOnlyCollection<Game> games)
		{
			var result = new Dictionary<Guid, AddToCollectionResult>();
			foreach (var game in games)
			{
				if (!_gameToAddFilter.ShouldTryAddGame(game, out var addResult))
				{
					result.Add(game.Id, (AddToCollectionResult)addResult);
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			return result;
		}

	}
}