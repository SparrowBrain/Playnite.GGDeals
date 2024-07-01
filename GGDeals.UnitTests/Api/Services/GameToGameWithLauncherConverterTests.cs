using AutoFixture.Xunit2;
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
			Assert.Equivalent(game, result);
		}
	}
}