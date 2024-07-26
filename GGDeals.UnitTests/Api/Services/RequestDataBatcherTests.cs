using AutoFixture.Xunit2;
using GGDeals.Api.Models;
using GGDeals.Api.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GGDeals.UnitTests.Api.Services
{
	public class RequestDataBatcherTests
	{
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly RequestDataBatcher _sut;

		public RequestDataBatcherTests()
		{
			_jsonSerializerSettings = new JsonSerializerSettings
			{
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new DefaultNamingStrategy()
				},
			};
			_sut = CreateSut();
		}

		[Fact]
		public void CreateDataJsons_ThrowsException_WhenInputIsEmpty()
		{
			// Arrange
			var games = new List<GameWithLauncher>();

			// Act
			var exception = Record.Exception(() => _sut.CreateDataJsons(games).ToList());

			// Assert
			Assert.NotNull(exception);
			Assert.IsType<ArgumentException>(exception);
		}

		[Theory]
		[AutoData]
		public void CreateDataJsons_CreatesSmallJson_WhenInputIsSmall(
			List<GameWithLauncher> games)
		{
			// Arrange

			// Act
			var result = _sut.CreateDataJsons(games);

			// Assert
			var json = Assert.Single(result);
			var deserialized = JsonConvert.DeserializeObject<List<GameWithLauncher>>(json, _jsonSerializerSettings);
			Assert.Equivalent(games, deserialized);
		}

		[Theory]
		[AutoData]
		public void CreateDataJsons_CreatesTwoBatches_WhenInput1001(
			List<GameWithLauncher> games)
		{
			// Arrange
			var game = games.Last();
			for (; games.Count < 1001;)
			{
				var newGame = JsonConvert.DeserializeObject<GameWithLauncher>(JsonConvert.SerializeObject(game), _jsonSerializerSettings);
				newGame.Id = Guid.NewGuid();
				games.Add(newGame);
			}

			var firstBatchGames = games.Take(1000).ToList();
			var secondBatchGames = games.Skip(1000).ToList();

			// Act
			var result = _sut.CreateDataJsons(games);

			// Assert
			Assert.Equal(2, result.Count());
			var firstBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.First(), _jsonSerializerSettings);
			var secondBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.Last(), _jsonSerializerSettings);
			Assert.Equivalent(firstBatchGames, firstBatch);
			Assert.Equivalent(secondBatchGames, secondBatch);
		}

		[Theory]
		[AutoData]
		public void CreateDataJsons_CreatesMultipleBatches_WhenJsonIsMoreThan10MB(
			List<GameWithLauncher> games)
		{
			// Arrange
			var game = games.Last();
			var json = JsonConvert.SerializeObject(games);
			while (Encoding.UTF8.GetBytes(json).Length < 10_000_000)
			{
				var newGame = JsonConvert.DeserializeObject<GameWithLauncher>(JsonConvert.SerializeObject(game));
				newGame.Id = Guid.NewGuid();
				newGame.Name = new string('A', 5_000_000);
				games.Add(newGame);
				json = JsonConvert.SerializeObject(games);
			}

			var firstBatchGames = games.Take(games.Count - 1).ToList();
			var secondBatchGames = games.Skip(games.Count - 1).ToList();

			// Act
			var result = _sut.CreateDataJsons(games);

			// Assert
			Assert.Equal(2, result.Count());
			var firstBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.First(), _jsonSerializerSettings);
			var secondBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.Last(), _jsonSerializerSettings);
			Assert.Equivalent(firstBatchGames, firstBatch);
			Assert.Equivalent(secondBatchGames, secondBatch);
		}

		private RequestDataBatcher CreateSut()
		{
			return new RequestDataBatcher(_jsonSerializerSettings);
		}
	}
}