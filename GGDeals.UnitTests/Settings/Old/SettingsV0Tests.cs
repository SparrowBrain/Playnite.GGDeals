using GGDeals.Settings.Old;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Settings.Old
{
    public class SettingsV0Tests
    {
        [Theory]
        [AutoMoqData]
        public void Migrate_RetainsOldValues(
            SettingsV0 sut)
        {
            // Arrange
            // TODO Should be skipped in equivalent, but alas
            sut.Version = 1;

            // Act
            var result = sut.Migrate() as SettingsV1;

            // Assert
            Assert.Equivalent(sut, result);
        }

        [Theory]
        [AutoMoqData]
        public void Migrate_FillsNewValuesWithDefault(
            SettingsV0 sut)
        {
            // Act
            var result = sut.Migrate() as SettingsV1;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(true, result.AddTagsToGames);
            Assert.Equal(false, result.SyncNewlyAddedGames);
            Assert.Equal(true, result.ShowProgressBar);
        }
    }
}