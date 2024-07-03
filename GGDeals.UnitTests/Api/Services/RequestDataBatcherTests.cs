using AutoFixture;
using AutoFixture.Xunit2;
using GGDeals.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGDeals.Api.Services;
using Xunit;

namespace GGDeals.UnitTests.Api.Services
{
	public class RequestDataBatcherTests
	{
		private Fixture _fixture = new Fixture();

		[Theory]
		[AutoData]
		public void CreateDataJsons_ThrowsException_WhenInputIsEmpty(
			RequestDataBatcher sut)
		{
			// Arrange
			var games = new List<GameWithLauncher>();

			// Act
			var exception = Record.Exception(() => sut.CreateDataJsons(games).ToList());

			// Assert
			Assert.NotNull(exception);
			Assert.IsType<ArgumentException>(exception);
		}

		[Theory]
		[AutoData]
		public void CreateDataJsons_CreatesSmallJson_WhenInputIsSmall(
			List<GameWithLauncher> games,
			RequestDataBatcher sut)
		{
			// Act
			var result = sut.CreateDataJsons(games);

			// Assert
			var json = Assert.Single(result);
			var deserialized = JsonConvert.DeserializeObject<List<GameWithLauncher>>(json);
			Assert.Equivalent(games, deserialized);
		}

		[Theory]
		[AutoData]
		public void CreateDataJsons_CreatesTwoBatches_WhenInput1001(
			List<GameWithLauncher> games,
			RequestDataBatcher sut)
		{
			// Arrange
			var game = games.Last();
			for (; games.Count < 1001;)
			{
				var newGame = JsonConvert.DeserializeObject<GameWithLauncher>(JsonConvert.SerializeObject(game));
				newGame.Id = Guid.NewGuid();
				games.Add(newGame);
			}

			var firstBatchGames = games.Take(1000).ToList();
			var secondBatchGames = games.Skip(1000).ToList();

			// Act
			var result = sut.CreateDataJsons(games);

			// Assert
			Assert.Equal(2, result.Count());
			var firstBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.First());
			var secondBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.Last());
			Assert.Equivalent(firstBatchGames, firstBatch);
			Assert.Equivalent(secondBatchGames, secondBatch);
		}

		[Theory]
		[AutoData]
		public void CreateDataJsons_CreatesMultipleBatches_WhenJsonIsMoreThan10MB(
			List<GameWithLauncher> games,
			RequestDataBatcher sut)
		{
			// Arrange
			var game = games.Last();
			var json = JsonConvert.SerializeObject(games);
			while (Encoding.UTF8.GetBytes(json).Length < 10_000_000)
			{
				var newGame = JsonConvert.DeserializeObject<GameWithLauncher>(JsonConvert.SerializeObject(game));
				newGame.Id = Guid.NewGuid();
				newGame.Description = new string('A', 5_000_000);
				games.Add(newGame);
				json = JsonConvert.SerializeObject(games);
			}

			var firstBatchGames = games.Take(games.Count - 1).ToList();
			var secondBatchGames = games.Skip(games.Count - 1).ToList();

			// Act
			var result = sut.CreateDataJsons(games);

			// Assert
			Assert.Equal(2, result.Count());
			var firstBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.First());
			var secondBatch = JsonConvert.DeserializeObject<List<GameWithLauncher>>(result.Last());
			Assert.Equivalent(firstBatchGames, firstBatch);
			Assert.Equivalent(secondBatchGames, secondBatch);
		}
	}
}