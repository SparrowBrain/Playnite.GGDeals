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
	public class GameToAddFilterTests
	{
		[Theory]
		[AutoMoqData]
		public void ShouldTryAddGame_ReturnsFalse_WhenLibraryIsSkippedInSettings(
			[Frozen] GGDealsSettings settings,
			Game game,
			GameToAddFilter sut)
		{
			// Arrange
			game.PluginId = settings.LibrariesToSkip.Last();

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.False(result);
			Assert.Equal(AddToCollectionResult.SkippedDueToLibrary, status.Result);
		}

		[Theory]
		[AutoMoqData]
		public void ShouldTryAddGame_ReturnsFalse_WhenLibraryIsPlayniteAndSkippedInSettings(
			[Frozen] GGDealsSettings settings,
			Game game,
			GameToAddFilter sut)
		{
			// Arrange
			game.PluginId = Guid.Empty;
			settings.SyncPlayniteLibrary = false;

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.False(result);
			Assert.Equal(AddToCollectionResult.SkippedDueToLibrary, status.Result);
		}

		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Added, AddToCollectionResult.Ignored)]
		public void ShouldTryAddGame_False_WhenStatusIsNotInSyncRunSettings(
			AddToCollectionResult allowedStatus,
			AddToCollectionResult actualStatus,
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			GGDealsSettings settings,
			Game game)
		{
			// Arrange
			var syncRunSettings = new SyncRunSettings { StatusesToSync = new List<AddToCollectionResult> { allowedStatus } };
			var sut = new GameToAddFilter(settings, gameStatusServiceMock.Object, syncRunSettings);
			gameStatusServiceMock.Setup(s => s.GetStatus(game)).Returns(actualStatus);

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.False(result);
			Assert.Equal(actualStatus, status.Result);
		}

		[Theory]
		[AutoMoqData]
		public void ShouldTryAddGame_True_WhenStatusIsInSyncRunSettings(
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			AddToCollectionResult allowedStatus,
			GGDealsSettings settings,
			Game game)
		{
			// Arrange
			var syncRunSettings = new SyncRunSettings { StatusesToSync = new List<AddToCollectionResult> { allowedStatus } };
			var sut = new GameToAddFilter(settings, gameStatusServiceMock.Object, syncRunSettings);
			gameStatusServiceMock.Setup(s => s.GetStatus(game)).Returns(allowedStatus);

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.True(result);
			Assert.Null(status);
		}
	}
}