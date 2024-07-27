using AutoFixture.Xunit2;
using GGDeals.Api.Models;
using GGDeals.Api.Services;
using GGDeals.Services;
using Moq;
using Newtonsoft.Json;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
	public class AddGamesServiceTests
	{
		[Theory]
		[AutoMoqData]
		public async Task TryAddToCollection_ReturnsAddToCollectionResult_WhenGamesAreFilteredOut(
			[Frozen] Mock<IGameToAddFilter> gameToAddFilterMock,
			List<Game> games,
			CancellationToken ct,
			AddResult expectedAddResult,
			AddGamesService sut)
		{
			// Arrange
			gameToAddFilterMock.Setup(x => x.ShouldTryAddGame(It.IsAny<Game>(), out expectedAddResult)).Returns(false);

			// Act
			var result = await sut.TryAddToCollection(games, ct);

			// Assert
			Assert.All(result.Values, x => Assert.Equal(expectedAddResult, x));
			Assert.All(games.Select(g => g.Id), x => Assert.Contains(x, result.Keys));
		}

		[Theory]
		[AutoMoqData]
		public async Task TryAddToCollection_DoesNotCallTheApi_WhenGamesAllAreFilteredOut(
			[Frozen] Mock<IGameToAddFilter> gameToAddFilterMock,
			[Frozen] Mock<IRequestDataBatcher> requestDataBatcherMock,
			List<Game> games,
			CancellationToken ct,
			AddResult expectedAddResult,
			AddGamesService sut)
		{
			// Arrange
			gameToAddFilterMock.Setup(x => x.ShouldTryAddGame(It.IsAny<Game>(), out expectedAddResult)).Returns(false);

			// Act
			await sut.TryAddToCollection(games, ct);

			// Assert
			requestDataBatcherMock.Verify(x => x.CreateDataJsons(It.IsAny<IReadOnlyCollection<GameWithLauncher>>()), Times.Never);
		}

		[Theory]
		[InlineAutoMoqData(ImportResultStatus.Error, AddToCollectionResult.Error)]
		[InlineAutoMoqData(ImportResultStatus.Added, AddToCollectionResult.Added)]
		[InlineAutoMoqData(ImportResultStatus.Skipped, AddToCollectionResult.Synced)]
		[InlineAutoMoqData(ImportResultStatus.Miss, AddToCollectionResult.NotFound)]
		[InlineAutoMoqData(ImportResultStatus.Ignored, AddToCollectionResult.Ignored)]
		public async Task TryAddToCollection_ReturnsSuccess_WhenGamesResponseSuccess(
			ImportResultStatus importResultStatus,
			AddToCollectionResult expectedAddToCollectionResult,
			[Frozen] Mock<IGameToAddFilter> gameToAddFilterMock,
			[Frozen] Mock<IGameToGameWithLauncherConverter> gameToGameWithLauncherConverterMock,
			[Frozen] Mock<IRequestDataBatcher> requestDataBatcherMock,
			[Frozen] Mock<IGGDealsApiClient> ggDealsApiClientMock,
			List<Game> games,
			CancellationToken ct,
			GGLauncher ggLauncher,
			AddResult expectedAddResult,
			List<string> dataJsons,
			List<ImportResponse> importResponses,
			AddGamesService sut)
		{
			// Arrange
			foreach (var importResponse in importResponses)
			{
				importResponse.Success = true;
				foreach (var item in importResponse.Data.Result)
				{
					item.Status = importResultStatus;
				}
			}

			gameToAddFilterMock.Setup(x => x.ShouldTryAddGame(It.IsAny<Game>(), out expectedAddResult)).Returns(true);
			gameToGameWithLauncherConverterMock.Setup(x => x.GetGameWithLauncher(It.IsAny<Game>())).Returns<Game>(g =>
			{
				var gameWithLauncher = JsonConvert.DeserializeObject<GameWithLauncher>(JsonConvert.SerializeObject(g));
				gameWithLauncher.GGLauncher = ggLauncher;
				return gameWithLauncher;
			});
			requestDataBatcherMock.Setup(x =>
					x.CreateDataJsons(It.Is<IReadOnlyCollection<GameWithLauncher>>(c =>
						c.Select(g => g.Id).SequenceEqual(games.Select(h => h.Id)))))
				.Returns(dataJsons);
			ggDealsApiClientMock
				.SetupSequence(x => x.ImportGames(It.IsAny<ImportRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(importResponses[0])
				.ReturnsAsync(importResponses[1])
				.ReturnsAsync(importResponses[2]);

			// Act
			var result = await sut.TryAddToCollection(games, ct);

			// Assert
			ggDealsApiClientMock.Verify(x => x.ImportGames(It.IsAny<ImportRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(dataJsons.Count));
			Assert.All(result.Values, x => Assert.Equal(expectedAddToCollectionResult, x.Result));
			Assert.All(result.Values,
				x => Assert.Contains(x.Message, importResponses.SelectMany(r => r.Data.Result.Select(d => d.Message))));
		}
	}
}