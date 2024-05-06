using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.Website;
using GGDeals.Website.Url;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Website
{
    public class GGWebsiteTests
    {
        [Theory]
        [AutoMoqData]
        public async Task NavigateToHomePage_ThrowsException_WhenNavigatingThrowsException(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Exception expected,
            GGWebsite sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Navigate(It.IsAny<string>())).Throws(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.NavigateToHomePage);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task NavigateToHomePage_Returns_WhenNavigatingSucceeds(
            GGWebsite sut)
        {
            // Act
            var actual = await Record.ExceptionAsync(sut.NavigateToHomePage);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryNavigateToGamePage_ThrowsException_WhenGamePageUrlGuesserThrowsException(
            [Frozen] Mock<IGamePageUrlGuesser> gamePageUrlGuesserMock,
            Game game,
            Exception expected,
            GGWebsite sut)
        {
            // Arrange
            gamePageUrlGuesserMock.Setup(x => x.Resolve(It.IsAny<Game>())).Throws(expected);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.TryNavigateToGamePage(game));

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryNavigateToGamePage_ThrowsException_WhenNavigatingToPageThrowsException(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Game game,
            Exception expected,
            GGWebsite sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.Navigate(It.IsAny<string>())).Throws(expected);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.TryNavigateToGamePage(game));

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryNavigateToGamePage_ThrowsException_WhenRunningScriptOnGamePageThrowsException(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Game game,
            Exception expected,
            GGWebsite sut)
        {
            // Arrange
            awaitableWebViewMock.Setup(x => x.EvaluateScriptAsync(It.IsAny<string>())).Throws(expected);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.TryNavigateToGamePage(game));

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryNavigateToGamePage_ThrowsException_WhenRunningScriptFails(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Game game,
            GGWebsite sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".error-404"").length")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = false });

            // Act
            var actual = await Record.ExceptionAsync(() => sut.TryNavigateToGamePage(game));

            // Assert
            Assert.NotNull(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryNavigateToGamePage_ReturnsFalse_WhenGamePageIs404ErrorPage(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Game game,
            GGWebsite sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".error-404"").length")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = true, Result = 1 });

            // Act
            var actual = await sut.TryNavigateToGamePage(game);

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task TryNavigateToGamePage_ReturnsTrue_WhenGamePageIsNot404ErrorPage(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            Game game,
            GGWebsite sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".error-404"").length")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = true, Result = 0 });

            // Act
            var actual = await sut.TryNavigateToGamePage(game);

            // Assert
            Assert.True(actual);
        }
    }
}