using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.Website;
using Moq;
using Playnite.SDK.Models;
using Xunit;

namespace GGDeals.UnitTests.Website
{
    public class GamePageTests
    {
        [Theory]
        [AutoMoqData]
        public async Task ClickOwnItButton_ThrowsException_IfClickingElementThrowsException(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Exception expected,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.IsAny<string>())).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.ClickOwnItButton);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task ClickOwnItButton_Returns_IfClickingElementWorked(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Exception exception,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.Is<string>(s => s != @"$("".owned-game.game-action-wrap "").first().find("".activate"")"))).ThrowsAsync(exception);

            // Act
            var actual = await Record.ExceptionAsync(sut.ClickOwnItButton);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExpandDrmDropDown_ThrowsException_IfClickingElementThrowsException(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Exception expected,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.IsAny<string>())).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.ExpandDrmDropDown);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExpandDrmDropDown_Returns_IfClickingElementWorked(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Exception exception,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.Is<string>(s => s != @"$(""#drm-collapse"").find(""a"").first()"))).ThrowsAsync(exception);

            // Act
            var actual = await Record.ExceptionAsync(sut.ExpandDrmDropDown);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task ClickDrmPlatformCheckBox_ThrowsException_IfClickingElementThrowsException(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Game game,
            Exception expected,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.IsAny<string>())).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.ClickDrmPlatformCheckBox(game));

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task ClickDrmPlatformCheckBox_Returns_IfClickingElementWorked(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            [Frozen] Mock<ILibraryNameMap> libraryNameMapMock,
            Game game,
            string ggLibraryName,
            Exception exception,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.Is<string>(s => s != $@"$(""#drm-collapse"").find("".filter-switch"").filter(""[data-name='{ggLibraryName}']"")"))).ThrowsAsync(exception);
            libraryNameMapMock.Setup(x => x.GetGGLibraryName(game)).Returns(ggLibraryName);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.ClickDrmPlatformCheckBox(game));

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task ClickSubmitOwnItForm_ThrowsException_IfClickingElementThrowsException(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Exception expected,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.IsAny<string>())).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.ClickSubmitOwnItForm);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task ClickSubmitOwnItForm_Returns_IfClickingElementWorked(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Exception exception,
            GamePage sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Click(It.Is<string>(s => s != @"$(""button[type='submit']"").filter("".btn"")"))).ThrowsAsync(exception);

            // Act
            var actual = await Record.ExceptionAsync(sut.ClickSubmitOwnItForm);

            // Assert
            Assert.Null(actual);
        }
    }
}