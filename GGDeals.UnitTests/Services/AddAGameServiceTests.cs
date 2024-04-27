using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.Services;
using GGDeals.Website;
using Moq;
using Playnite.SDK.Models;
using Xunit;

namespace GGDeals.UnitTests.Services
{
    public class AddAGameServiceTests
    {
        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_NavigateToGamePage_ReturnsFalse_WhenNoSuchPageExists(
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            IGamePage gamePage = null;
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game.Name, out gamePage)).ReturnsAsync(false);

            // Act
            var success = await sut.TryAddToCollection(game);

            // Assert
            Assert.False(success);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ThrowsException_WhenFailedToClickOwnItButton(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            var gamePage = gamePageMock.Object;
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game.Name, out gamePage)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.ClickOwnItButton()).Throws(() => new Exception("Clicking own it button failed."));

            // Act
            var exception = await Record.ExceptionAsync(() => sut.TryAddToCollection(game));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Clicking own it button failed.", exception.Message);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ThrowsException_WhenExpandingDrmDropDownFails(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            var gamePage = gamePageMock.Object;
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game.Name, out gamePage)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.ExpandDrmDropDown()).Throws(() => new Exception("Expanding DRM dropdown failed."));

            // Act
            var exception = await Record.ExceptionAsync(() => sut.TryAddToCollection(game));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Expanding DRM dropdown failed.", exception.Message);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ThrowsException_ClickingDrmPlatformCheckBoxFailed(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            var gamePage = gamePageMock.Object;
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game.Name, out gamePage)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.ClickDrmPlatformCheckBox(game)).Throws(() => new Exception("Clicking DRM platform checkbox failed."));

            // Act
            var exception = await Record.ExceptionAsync(() => sut.TryAddToCollection(game));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Clicking DRM platform checkbox failed.", exception.Message);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ThrowsException_ClickingSubmitOwnItFormFails(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            var gamePage = gamePageMock.Object;
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game.Name, out gamePage)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.ClickSubmitOwnItForm()).Throws(() => new Exception("Clicking Submit \"Own It\" form failed."));

            // Act
            var exception = await Record.ExceptionAsync(() => sut.TryAddToCollection(game));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Clicking Submit \"Own It\" form failed.", exception.Message);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ReturnsTrue_WhenGameIsAdded(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            var gamePage = gamePageMock.Object;
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game.Name, out gamePage)).ReturnsAsync(true);

            // Act
            var actual = await sut.TryAddToCollection(game);

            // Assert
            Assert.True(actual);
        }
    }
}