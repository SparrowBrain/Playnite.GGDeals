using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.Website;
using Moq;
using Playnite.SDK;
using Xunit;

namespace GGDeals.UnitTests.Website
{
    public class HomePageTests
    {
        [Theory]
        [AutoMoqData]
        public async Task IsUserLoggedIn_ThrowsException_WhenRunningScriptFails(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            HomePage sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".login"").length")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = false });

            // Act
            var actual = await Record.ExceptionAsync(sut.IsUserLoggedIn);

            // Assert
            Assert.NotNull(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task IsUserLoggedIn_ReturnsFalse_WhenLoginButtonFound(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            HomePage sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".login"").length")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = true, Result = 1 });

            // Act
            var result = await sut.IsUserLoggedIn();

            // Assert
            Assert.False(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task IsUserLoggedIn_ReturnsTrue_WhenLoginButtonNotFound(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            HomePage sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".login"").length")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = true, Result = 0 });

            // Act
            var result = await sut.IsUserLoggedIn();

            // Assert
            Assert.True(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetUserName_ThrowsException_WhenRunningScriptFails(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            HomePage sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".menu-profile-label"").contents().not($("".menu-profile-label"").children()).text()")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = false });

            // Act
            var actual = await Record.ExceptionAsync(sut.GetUserName);

            // Assert
            Assert.NotNull(actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetUserName_ReturnsUserName_WhenUserNameFound(
            [Frozen] Mock<IAwaitableWebView> awaitableWebViewMock,
            string expected,
            HomePage sut)
        {
            // Arrange
            awaitableWebViewMock
                .Setup(x => x.EvaluateScriptAsync(It.Is<string>(s => s == @"$("".menu-profile-label"").contents().not($("".menu-profile-label"").children()).text()")))
                .ReturnsAsync(new JavaScriptEvaluationResult() { Success = true, Result = expected });

            // Act
            var actual = await sut.GetUserName();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}