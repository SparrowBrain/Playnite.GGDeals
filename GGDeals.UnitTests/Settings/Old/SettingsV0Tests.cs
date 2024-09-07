using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGDeals.Settings;
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
            var result = sut.Migrate() as GGDealsSettings;

            // Assert
            Assert.Equivalent(sut, result);
        }

        [Theory]
        [AutoMoqData]
        public void Migrate_FillsNewValuesWithDefault(
            SettingsV0 sut)
        {
            // Act
            var result = sut.Migrate() as GGDealsSettings;

            // Assert
            Assert.NotNull(result);
            result.AddTagsToGames = true;
            result.SyncNewlyAddedGames = false;
            result.ShowProgressBar = true;
        }
    }
}