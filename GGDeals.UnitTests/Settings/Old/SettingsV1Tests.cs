using GGDeals.Settings;
using GGDeals.Settings.Old;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Settings.Old
{
	public class SettingsV1Tests
	{
		[Theory]
		[AutoMoqData]
		public void Migrate_RetainsOldValues(
			SettingsV1 sut)
		{
			// Arrange
			// TODO Should be skipped in equivalent, but alas
			sut.Version = 2;

			// Act
			var result = sut.Migrate() as GGDealsSettings;

			// Assert
			Assert.Equivalent(sut, result);
		}

		[Theory]
		[AutoMoqData]
		public void Migrate_FillsNewValuesWithDefault(
			SettingsV1 sut)
		{
			// Act
			var result = sut.Migrate() as GGDealsSettings;

			// Assert
			Assert.NotNull(result);
			Assert.Empty(result.LibraryMapOverride);
		}
	}
}