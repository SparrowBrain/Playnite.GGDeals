﻿using AutoFixture.Xunit2;
using GGDeals.Api.Models;
using GGDeals.Api.Services;
using Moq;
using Playnite.SDK.Models;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Api.Services
{
	public class GameToGameWithLauncherConverterTests
	{
		[Theory]
		[AutoMoqData]
		public void GetGameWithLauncher(
			[Frozen] Mock<ILibraryToGGLauncherMap> libraryToGGLauncherMapMock,
			Game game,
			GGLauncher ggLauncher,
			GameToGameWithLauncherConverter sut)
		{
			// Arrange
			libraryToGGLauncherMapMock.Setup(x => x.GetGGLauncher(game)).Returns(ggLauncher);

			// Act
			var result = sut.GetGameWithLauncher(game);

			// Assert
			Assert.Equal(ggLauncher, result.GGLauncher);
			Assert.Equal(game.Id, result.Id);
			Assert.Equal(game.GameId, result.GameId);
			Assert.Equal(game.Links, result.Links);
			Assert.Equal(game.Source, result.Source);
			Assert.Equal(game.ReleaseDate, result.ReleaseDate);
			Assert.Equal(game.ReleaseYear, result.ReleaseYear);
			Assert.Equal(game.Name, result.Name);
		}

		[Theory]
		[AutoMoqData]
		public void GetGameWithLauncher_WhenLinksAreNull(
			[Frozen] Mock<ILibraryToGGLauncherMap> libraryToGGLauncherMapMock,
			Game game,
			GGLauncher ggLauncher,
			GameToGameWithLauncherConverter sut)
		{
			// Arrange
			game.Links = null;
			libraryToGGLauncherMapMock.Setup(x => x.GetGGLauncher(game)).Returns(ggLauncher);

			// Act
			var result = sut.GetGameWithLauncher(game);

			// Assert
			Assert.Equal(ggLauncher, result.GGLauncher);
			Assert.Equal(game.Id, result.Id);
			Assert.Equal(game.GameId, result.GameId);
			Assert.Equal(game.Links, result.Links);
			Assert.Equal(game.Source, result.Source);
			Assert.Equal(game.ReleaseDate, result.ReleaseDate);
			Assert.Equal(game.ReleaseYear, result.ReleaseYear);
			Assert.Equal(game.Name, result.Name);
		}
	}
}