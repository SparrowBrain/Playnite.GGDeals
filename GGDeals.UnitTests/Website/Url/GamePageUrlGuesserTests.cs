using AutoFixture.Xunit2;
using GGDeals.Website.Url;
using Moq;
using Playnite.SDK.Models;
using TestTools.Shared;
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
        [InlineAutoMoqData("Assassin's Creed Valhalla", "https://abc.xyz/game/assassins-creed-valhalla/")]
        [InlineAutoMoqData("Superbrothers: Sword & Sworcery EP", "https://abc.xyz/game/superbrothers-sword-sworcery-ep/")]
        public void Resolve_GivenGameName_ReturnsFullUrl(
            string gameName,
            string expectedUrl,
            [Frozen] Mock<IHomePageResolver> homePageResolverMock,
            Game game,
            GamePageUrlGuesser sut)
        {
            // Arrange
            game.Name = gameName;
            homePageResolverMock.Setup(x => x.Resolve()).Returns("https://abc.xyz");

            // Act
            var actualUrl = sut.Resolve(game);

            // Assert
            Assert.Equal(expectedUrl, actualUrl);
        }

        [Theory]
        [InlineAutoMoqData("2064: Read Only Memories", "https://abc.xyz/game/read-only-memories/")]
        public void Resolve_GivenAnomalyGameName_ReturnsAntiPatternUrl(
            string gameName,
            string expectedUrl,
            [Frozen] Mock<IHomePageResolver> homePageResolverMock,
            Game game,
            GamePageUrlGuesser sut)
        {
            // Arrange
            game.Name = gameName;
            homePageResolverMock.Setup(x => x.Resolve()).Returns("https://abc.xyz");

            // Act
            var actualUrl = sut.Resolve(game);

            // Assert
            Assert.Equal(expectedUrl, actualUrl);
        }
    }
}