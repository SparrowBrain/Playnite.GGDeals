using System;
using System.Collections.Generic;
using System.Reflection;
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
        public void GetLibraryName_ReturnsGGLibraryName_WhenGamePluginNameMatchesTheMap(
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

        [Theory(Skip = "Cannot set game Source. It is being read from DB in a static internal.")]
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
        public void GetLibraryName_ReturnsGGLibraryNameFromSourceName_WhenGamePluginNameDoesNotExist(
            string playniteLibraryName,
            string ggLibraryName,
            [Frozen] GameSource source,
            Game game,
            LibraryNameMap sut)
        {
            // Arrange
            source.Name = playniteLibraryName;
            SetGameSource(game, source);

            // Act
            var actual = sut.GetGGLibraryName(game);

            // Assert
            Assert.Equal(ggLibraryName, actual);
        }

        private static void SetGameSource(Game game, GameSource source)
        {
            var prop = game.GetType().GetProperty(nameof(game.Source), BindingFlags.Public | BindingFlags.Instance);
            if (null != prop && prop.CanWrite)
            {
                prop.SetValue(game, source, null);
            }
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