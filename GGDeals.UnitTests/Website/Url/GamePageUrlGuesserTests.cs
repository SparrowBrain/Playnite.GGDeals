using AutoFixture.Xunit2;
using GGDeals.Website.Url;
using Moq;
using Xunit;

namespace GGDeals.UnitTests.Website.Url
{
    public class GamePageUrlGuesserTests
    {
        [Theory]
        [InlineAutoMoqData("The Crew", "https://abc.xyz/game/the-crew/")]
        [InlineAutoMoqData("DARK SOULS: REMASTERED", "https://abc.xyz/game/dark-souls-remastered/")]
        [InlineAutoMoqData("Terminator: Dark Fate - Defiance", "https://abc.xyz/game/terminator-dark-fate-defiance/")]
        [InlineAutoMoqData("Destroy All Humans! 2 - Reprobed", "https://abc.xyz/game/destroy-all-humans-2-reprobed/")]
        public void Resolve_GivenGameName_ReturnsFullUrl(
            string gameName,
            string expectedUrl,
            [Frozen] Mock<IHomePageResolver> homePageResolverMock,
            GamePageUrlGuesser sut)
        {
            // Arrange
            homePageResolverMock.Setup(x => x.Resolve()).Returns("https://abc.xyz");

            // Act
            var actualUrl = sut.Resolve(gameName);

            // Assert
            Assert.Equal(expectedUrl, actualUrl);
        }
    }
}