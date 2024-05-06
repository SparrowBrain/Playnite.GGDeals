using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.Services;
using GGDeals.Settings;
using GGDeals.Website;
using Moq;
using Playnite.SDK.Models;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
    public class AddAGameServiceTests
    {
        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ReturnsSkippedDueToLibrary_WhenLibraryIsSkippedInSettings(
            [Frozen] GGDealsSettings settings,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            game.PluginId = settings.LibrariesToSkip.Last();

            // Act
            var result = await sut.TryAddToCollection(game);

            // Assert
            Assert.Equal(AddToCollectionResult.SkippedDueToLibrary, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ReturnsNoPageFound_WhenNoSuchPageExists(
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(false);

            // Act
            var result = await sut.TryAddToCollection(game);

            // Assert
            Assert.Equal(AddToCollectionResult.PageNotFound, result);
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
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(true);
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
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.ExpandDrmDropDown()).Throws(() => new Exception("Expanding DRM dropdown failed."));

            // Act
            var exception = await Record.ExceptionAsync(() => sut.TryAddToCollection(game));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Expanding DRM dropdown failed.", exception.Message);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ThrowsException_CheckingForActiveDrmCheckboxFailed(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            Exception expected,
            AddAGameService sut)
        {
            // Arrange
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.IsDrmPlatformCheckboxActive(game)).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.TryAddToCollection(game));

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ReturnsGameAlreadyOwned_WhenCheckingDrmCheckboxIsActiveReturnsTrue(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            Exception expected,
            AddAGameService sut)
        {
            // Arrange
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.IsDrmPlatformCheckboxActive(game)).ReturnsAsync(true);

            // Act
            var result = await sut.TryAddToCollection(game);

            // Assert
            Assert.Equal(AddToCollectionResult.AlreadyOwned, result);
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
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.IsDrmPlatformCheckboxActive(game)).ReturnsAsync(false);
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
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.IsDrmPlatformCheckboxActive(game)).ReturnsAsync(false);
            gamePageMock.Setup(x => x.ClickSubmitOwnItForm()).Throws(() => new Exception("Clicking Submit \"Own It\" form failed."));

            // Act
            var exception = await Record.ExceptionAsync(() => sut.TryAddToCollection(game));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Clicking Submit \"Own It\" form failed.", exception.Message);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryAddToCollection_ReturnsAdded_WhenGameIsAdded(
            [Frozen] Mock<IGamePage> gamePageMock,
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Game game,
            AddAGameService sut)
        {
            // Arrange
            ggWebsiteMock.Setup(x => x.TryNavigateToGamePage(game)).ReturnsAsync(true);
            gamePageMock.Setup(x => x.IsDrmPlatformCheckboxActive(game)).ReturnsAsync(false);

            // Act
            var actual = await sut.TryAddToCollection(game);

            // Assert
            Assert.Equal(AddToCollectionResult.Added, actual);
        }
    }
}