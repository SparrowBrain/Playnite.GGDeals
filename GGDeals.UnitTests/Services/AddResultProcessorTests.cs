using AutoFixture.Xunit2;
using GGDeals.Models;
using GGDeals.Services;
using GGDeals.Settings;
using Moq;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
	public class AddResultProcessorTests
	{
		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Added)]
		[InlineAutoMoqData(AddToCollectionResult.Synced)]
		[InlineAutoMoqData(AddToCollectionResult.NotFound)]
		[InlineAutoMoqData(AddToCollectionResult.Ignored)]
		public void Process_ChangesGameStatus_WhenResultWarrantsStatusChange(
			AddToCollectionResult addToCollectionResult,
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			[Frozen] GGDealsSettings settings,
			List<Game> games,
			AddResultProcessor sut)
		{
			// Arrange
			settings.AddTagsToGames = true;
			var addResults = games.ToDictionary(g => g.Id, g => new AddResult { Result = addToCollectionResult });

			// Act
			sut.Process(games, addResults);

			// Assert
			foreach (var game in addResults.Select(addResult => games.Single(g => g.Id == addResult.Key)))
			{
				gameStatusServiceMock.Verify(s => s.UpdateStatus(game, addToCollectionResult), Times.Once);
			}
		}

		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Error)]
		[InlineAutoMoqData(AddToCollectionResult.SkippedDueToLibrary)]
		public void Process_DoNotChangeGameStatus_WhenStatusShouldNotChange(
			AddToCollectionResult addToCollectionResult,
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			List<Game> games,
			AddResultProcessor sut)
		{
			// Arrange
			var addResults = games.ToDictionary(g => g.Id, g => new AddResult { Result = addToCollectionResult });

			// Act
			sut.Process(games, addResults);

			// Assert
			gameStatusServiceMock.Verify(s => s.UpdateStatus(It.IsAny<Game>(), It.IsAny<AddToCollectionResult>()), Times.Never);
		}

		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Added)]
		[InlineAutoMoqData(AddToCollectionResult.Synced)]
		[InlineAutoMoqData(AddToCollectionResult.NotFound)]
		[InlineAutoMoqData(AddToCollectionResult.Ignored)]
		public void Process_DoesNotChangeGameStatus_WhenSettingsAreSetNotToAddTags(
			AddToCollectionResult addToCollectionResult,
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			[Frozen] GGDealsSettings settings,
			List<Game> games,
			AddResultProcessor sut)
		{
			// Arrange
			settings.AddTagsToGames = false;
			var addResults = games.ToDictionary(g => g.Id, g => new AddResult { Result = addToCollectionResult });

			// Act
			sut.Process(games, addResults);

			// Assert
			foreach (var game in addResults.Select(addResult => games.Single(g => g.Id == addResult.Key)))
			{
				gameStatusServiceMock.Verify(s => s.UpdateStatus(game, addToCollectionResult), Times.Never);
			}
		}

		[Theory]
		[AutoMoqData()]
		public void Process_UsesBufferedUpdate_WhenChangingStatus(
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			[Frozen] GGDealsSettings settings,
			Mock<IDisposable> bufferedUpdateMock,
			AddToCollectionResult addToCollectionResult,
			List<Game> games,
			AddResultProcessor sut)
		{
			// Arrange
			settings.AddTagsToGames = true;
			var addResults = games.ToDictionary(g => g.Id, g => new AddResult { Result = addToCollectionResult });
			gameStatusServiceMock.Setup(s => s.BufferedUpdate()).Returns(bufferedUpdateMock.Object);

			// Act
			sut.Process(games, addResults);

			// Assert
			bufferedUpdateMock.Verify(b => b.Dispose(), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public void Process_AddsLink_WhenResultContainsUrlAndSettingsAllowAddingLink(
			[Frozen] Mock<IAddLinkService> addLinkServiceMock,
			[Frozen] GGDealsSettings settings,
			AddToCollectionResult addToCollectionResult,
			string url,
			List<Game> games,
			AddResultProcessor sut)
		{
			// Arrange
			var addResults = games.ToDictionary(g => g.Id, g => new AddResult { Result = addToCollectionResult, Url = url });
			settings.AddLinksToGames = true;

			// Act
			sut.Process(games, addResults);

			// Assert
			foreach (var game in addResults.Select(addResult => games.Single(g => g.Id == addResult.Key)))
			{
				addLinkServiceMock.Verify(s => s.AddLink(game, url), Times.Once);
			}
		}

		[Theory]
		[InlineAutoMoqData(false, "https://gg.deals/game/chrono-trigger/")]
		[InlineAutoMoqData(true, null)]
		[InlineAutoMoqData(true, "")]
		public void Process_DoesNotAddLink_WhenResultHasNoUrlOrSettingsDoNotAllowAddingLink(
			bool addLinksToGames,
			string url,
			[Frozen] Mock<IAddLinkService> addLinkServiceMock,
			[Frozen] GGDealsSettings settings,
			AddToCollectionResult addToCollectionResult,
			List<Game> games,
			AddResultProcessor sut)
		{
			// Arrange
			var addResults = games.ToDictionary(g => g.Id, g => new AddResult { Result = addToCollectionResult, Url = url });
			settings.AddLinksToGames = addLinksToGames;

			// Act
			sut.Process(games, addResults);

			// Assert
			addLinkServiceMock.Verify(s => s.AddLink(It.IsAny<Game>(), It.IsAny<string>()), Times.Never);
		}
	}
}