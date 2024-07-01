using AutoFixture.Xunit2;
using GGDeals.Services;
using Moq;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
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
			AddToCollectionResult? expectedAddResult,
			AddGamesService sut)
		{
			// Arrange
			gameToAddFilterMock.Setup(x => x.ShouldTryAddGame(It.IsAny<Game>(), out expectedAddResult)).Returns(false);

			// Act
			var result = await sut.TryAddToCollection(games);

			// Assert
			Assert.All(result.Values, x => Assert.Equal(expectedAddResult, x));
			Assert.All(games.Select(g => g.Id), x => Assert.Contains(x, result.Keys));
		}
	}
}