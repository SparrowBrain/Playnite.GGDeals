using System;
using System.Linq;
using AutoFixture.Xunit2;
using GGDeals.Services;
using GGDeals.Settings;
using Moq;
using Playnite.SDK.Models;
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
		[AutoMoqData]
		public void ShouldTryAddGame_False_WhenGameIsIgnored(
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			Game game,
			GameToAddFilter sut)
		{
			// Arrange
			gameStatusServiceMock.Setup(s => s.GetStatus(game)).Returns(AddToCollectionResult.Ignored);

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.False(result);
			Assert.Equal(AddToCollectionResult.Ignored, status.Result);
		}

		[Theory]
		[AutoMoqData]
		public void ShouldTryAddGame_False_WhenGameIsSynced(
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			Game game,
			GameToAddFilter sut)
		{
			// Arrange
			gameStatusServiceMock.Setup(s => s.GetStatus(game)).Returns(AddToCollectionResult.Synced);

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.False(result);
			Assert.Equal(AddToCollectionResult.Synced, status.Result);
		}

		[Theory]
		[AutoMoqData]
		public void ShouldTryAddGame_True_WhenGameIsNew(
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			Game game,
			GameToAddFilter sut)
		{
			// Arrange
			gameStatusServiceMock.Setup(s => s.GetStatus(game)).Returns(AddToCollectionResult.New);

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.True(result);
			Assert.Equal(null, status);
		}

		[Theory]
		[AutoMoqData]
		public void ShouldTryAddGame_True_WhenGameIsNotFound(
			[Frozen] Mock<IGameStatusService> gameStatusServiceMock,
			Game game,
			GameToAddFilter sut)
		{
			// Arrange
			gameStatusServiceMock.Setup(s => s.GetStatus(game)).Returns(AddToCollectionResult.NotFound);

			// Act
			var result = sut.ShouldTryAddGame(game, out var status);

			// Assert
			Assert.True(result);
			Assert.Equal(null, status);
		}
	}
}