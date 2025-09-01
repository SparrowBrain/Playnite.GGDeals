using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using GGDeals.Settings;
using GGDeals.Settings.Old;
using Moq;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Settings
{
    public class SettingsMigratorTests
    {
        [Theory, AutoMoqData]
        public void LoadAndMigrateToNewest_ThrowsException_WhenCalledWithNonConfiguredVersion(
            SettingsMigrator sut)
        {
            // Act
            var act = new Action(() => sut.LoadAndMigrateToNewest(int.MaxValue));

            // Assert
            Assert.ThrowsAny<ArgumentException>(act);
        }

        [Theory, MemberAutoMoqData(nameof(GetAllOldSettingsVersions))]
        public void LoadAndMigrateToNewest_MigratesAllExistingNonCurrentSettingsVersionsToNewest(
            int version,
            SettingsMigrator sut)
        {
            // Act
            var result = sut.LoadAndMigrateToNewest(version);

            // Assert
            Assert.Equal(GGDealsSettings.CurrentVersion, result.Version);
        }

        [Theory]
        [InlineAutoMoqData(0)]
        [InlineAutoMoqData(+2)]
        [InlineAutoMoqData(-1)]
        public void LoadAndMigrateToNewest_ThrowsException_WhenSettingsMigratesToNonIncrementedVersion(
            int versionIncrement,
            [Frozen] Mock<IPluginSettingsPersistence> pluginSettingsPersistenceMock,
            SettingsV0Fake settingsV0Fake,
            SettingsMigrator sut)
        {
            // Arrange
            pluginSettingsPersistenceMock.Setup(x => x.LoadPluginSettings<SettingsV0>()).Returns(settingsV0Fake);
            settingsV0Fake.SetupVersionItMigratesTo(settingsV0Fake.Version + versionIncrement);

            // Act
            var act = new Action(() => sut.LoadAndMigrateToNewest(settingsV0Fake.Version));

            // Assert
            Assert.ThrowsAny<Exception>(act);
        }

        public static IEnumerable<object[]> GetAllOldSettingsVersions()
        {
            var type = typeof(IVersionedSettings);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("GGDeals"))
                .SelectMany(s => s.GetTypes())
                .Where(x => x.IsClass)
                .Where(p => type.IsAssignableFrom(p))
                .Where(x => x != typeof(VersionedSettings))
                .Where(x => x != typeof(SettingsV0Fake));

            var allOldSettingsVersions = types.Select(x =>
            {
                var ctor = x.GetConstructor(new Type[] { });
                object instance = ctor.Invoke(new object[] { });
                return new object[] { (instance as IVersionedSettings).Version };
            }).Where(x => (int)x[0] != GGDealsSettings.CurrentVersion);

            return allOldSettingsVersions;
        }

        public class SettingsV0Fake : SettingsV0
        {
            private int _newVersion;

            public void SetupVersionItMigratesTo(int newVersion)
            {
                _newVersion = newVersion;
            }

            public override IVersionedSettings Migrate()
            {
                return new SettingsV0()
                {
                    Version = _newVersion
                };
            }
        }
    }
}