using GGDeals.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGDeals.Api.Services
{
	public class RequestDataBatcher : IRequestDataBatcher
	{
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private const int BatchGameCount = 1000;
		private const int MaxJsonLength = 10_000_000;

		public RequestDataBatcher(JsonSerializerSettings jsonSerializerSettings)
		{
			_jsonSerializerSettings = jsonSerializerSettings;
		}

		public IEnumerable<string> CreateDataJsons(IReadOnlyCollection<GameWithLauncher> games)
		{
			if (games.Count == 0)
			{
				throw new ArgumentException("Cannot batch an empty games list", nameof(games));
			}

			var remainingGames = new List<GameWithLauncher>(games);
			while (true)
			{
				var batch = remainingGames.Take(BatchGameCount).ToList();
				var gamesTaken = batch.Count;
				var json = JsonConvert.SerializeObject(batch, _jsonSerializerSettings);
				for (var i = 1; Encoding.UTF8.GetBytes(json).Length > MaxJsonLength; i++)
				{
					batch = remainingGames.Take(gamesTaken - i).ToList();
					gamesTaken = batch.Count;
					json = JsonConvert.SerializeObject(batch, _jsonSerializerSettings);
				}

				yield return json;

				remainingGames = remainingGames.Skip(gamesTaken).ToList();
				if (!remainingGames.Any())
				{
					yield break;
				}
			}
		}
	}
}