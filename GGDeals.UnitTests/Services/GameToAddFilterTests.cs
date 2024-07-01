using System;
using System.Linq;
using AutoFixture.Xunit2;
using GGDeals.Services;
using GGDeals.Settings;
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
			Assert.Equal(AddToCollectionResult.SkippedDueToLibrary, status);
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
			Assert.Equal(AddToCollectionResult.SkippedDueToLibrary, status);
		}
	}
}