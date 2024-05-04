using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.Services;
using GGDeals.Website;
using Moq;
using Playnite.SDK;
using Xunit;

namespace GGDeals.UnitTests.Services
{
    public class AuthenticationStatusServiceTests
    {
        [Theory]
        [AutoMoqData]
        public async Task GetAuthenticationStatus_ThrowsException_WhenNavigatingToHomePageThrowsException(
            [Frozen] Mock<IGGWebsite> ggWebsiteMock,
            Exception expected,
            AuthenticationStatusService sut)
        {
            // Arrange
            ggWebsiteMock.Setup(x => x.NavigateToHomePage()).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.GetAuthenticationStatus);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetAuthenticationStatus_ThrowsException_WhenCheckingIsUserLoggedInThrowsException(
            [Frozen] Mock<IHomePage> homePageMock,
            Exception expected,
            AuthenticationStatusService sut)
        {
            // Arrange
            homePageMock.Setup(x => x.IsUserLoggedIn()).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.GetAuthenticationStatus);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetAuthenticationStatus_ReturnsNotAuthenticated_WhenCheckingIsUserLoggedInReturnsFalse(
            [Frozen] Mock<IHomePage> homePageMock,
            AuthenticationStatusService sut)
        {
            // Arrange
            homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(false);

            // Act
            var actual = await sut.GetAuthenticationStatus();

            // Assert
            Assert.Equal(ResourceProvider.GetString("LOC_GGDeals_SettingsNotAuthenticated"), actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetAuthenticationStatus_ThrowsException_WhenGettingUserNameThrowsException(
            [Frozen] Mock<IHomePage> homePageMock,
            Exception expected,
            AuthenticationStatusService sut)
        {
            // Arrange
            homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
            homePageMock.Setup(x => x.GetUserName()).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.GetAuthenticationStatus);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetAuthenticationStatus_ReturnsUserName_WhenGettingUserNameReturnsUserName(
            [Frozen] Mock<IHomePage> homePageMock,
            string userName,
            AuthenticationStatusService sut)
        {
            // Arrange
            homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
            homePageMock.Setup(x => x.GetUserName()).ReturnsAsync(userName);

            // Act
            var result = await sut.GetAuthenticationStatus();

            // Assert
            Assert.Equal(string.Format(ResourceProvider.GetString("LOC_GGDeals_SettingsAuthenticatedAs_Format"), userName), result);
        }
    }
}