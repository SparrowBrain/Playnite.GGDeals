using System;
using System.Collections.Generic;
using AutoFixture.Xunit2;
using GGDeals.Website;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Website
{
    public class LibraryNameMapTests
    {
        [Theory]
        [InlineAutoMoqData("CB91DFC9-B977-43BF-8E70-55F46E410FAB", "Steam")]
        [InlineAutoMoqData("85DD7072-2F20-4E76-A007-41035E390724", "EA App")]
        [InlineAutoMoqData("C2F038E5-8B92-4877-91F1-DA9094155FC5", "Ubisoft Connect")]
        [InlineAutoMoqData("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E", "GOG")]
        [InlineAutoMoqData("00000002-DBD1-46C6-B5D0-B1BA559D10E4", "Epic Games Launcher")]
        [InlineAutoMoqData("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287", "Microsoft Store")]
        [InlineAutoMoqData("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD", "Battle.net")]
        [InlineAutoMoqData("88409022-088a-4de8-805a-fdbac291f00a", "Rockstar Games Launcher")]
        [InlineAutoMoqData("402674cd-4af6-4886-b6ec-0e695bfa0688", "Prime Gaming")]
        [InlineAutoMoqData("e4ac81cb-1b1a-4ec9-8639-9a9633989a71", "PlayStation Network")]
        [InlineAutoMoqData("e4ac81cb-1b1a-4ec9-8639-9a9633989a72", "Nintendo eShop")]
        [InlineAutoMoqData("00000001-EBB2-4EEC-ABCB-7C89937A42BB", "Itch.io")]
        [InlineAutoMoqData("6defe124-7936-4a1d-848b-0618e2122d19", "Other")]
        [InlineAutoMoqData("415373e6-afb4-4be0-8f31-82d9a0b54086", "Other")]
        public void GetLibraryName_ReturnsGGLibraryNameFromLibraryId_WhenPluginIdMatchesTheMap(
            string playniteLibraryId,
            string ggLibraryName,
            [Frozen] Mock<IAddons> addonsMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            Game game,
            LibraryNameMap sut)
        {
            // Arrange
            game.PluginId = Guid.Parse(playniteLibraryId);
            playniteApiMock.Setup(x => x.Addons).Returns(addonsMock.Object);
            addonsMock.Setup(x => x.Plugins).Returns(new List<Plugin>());

            // Act
            var actual = sut.GetGGLibraryName(game);

            // Assert
            Assert.Equal(ggLibraryName, actual);
        }

        [Theory]
        [InlineAutoMoqData("Steam", "Steam")]
        [InlineAutoMoqData("EA app", "EA App")]
        [InlineAutoMoqData("Ubisoft Connect", "Ubisoft Connect")]
        [InlineAutoMoqData("GOG", "GOG")]
        [InlineAutoMoqData("Epic", "Epic Games Launcher")]
        [InlineAutoMoqData("Xbox", "Microsoft Store")]
        [InlineAutoMoqData("Battle.net", "Battle.net")]
        [InlineAutoMoqData("Rockstar Games", "Rockstar Games Launcher")]
        [InlineAutoMoqData("Amazon Games", "Prime Gaming")]
        [InlineAutoMoqData("PlayStation", "PlayStation Network")]
        [InlineAutoMoqData("Nintendo", "Nintendo eShop")]
        [InlineAutoMoqData("itch.io", "Itch.io")]
        [InlineAutoMoqData("ABC", "Other")]
        [InlineAutoMoqData("***", "Other")]
        public void GetLibraryName_ReturnsGGLibraryNameMatchByPluginName_WhenPluginIdDoesNotMatch(
            string playniteLibraryName,
            string ggLibraryName,
            [Frozen] Mock<IAddons> addonsMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            Game game,
            LibraryNameMap sut)
        {
            // Arrange
            var libraryPlugin = new TestableLibraryPlugin(playniteApiMock.Object, game.PluginId, playniteLibraryName);
            playniteApiMock.Setup(x => x.Addons).Returns(addonsMock.Object);
            addonsMock.Setup(x => x.Plugins).Returns(new List<Plugin> { libraryPlugin });

            // Act
            var actual = sut.GetGGLibraryName(game);

            // Assert
            Assert.Equal(ggLibraryName, actual);
        }

        private class TestableLibraryPlugin : LibraryPlugin
        {
            public TestableLibraryPlugin(IPlayniteAPI api, Guid id, string name) : base(api)
            {
                Id = id;
                Name = name;
            }

            public override Guid Id { get; }
            public override string Name { get; }
        }
    }
}