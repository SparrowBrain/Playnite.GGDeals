using System;
using AutoFixture.Xunit2;
using Moq;
using ReleaseTools.Changelog;
using ReleaseTools.InstallerManifestYaml;
using ReleaseTools.Package;
using TestTools.Shared;
using Xunit;

namespace ReleaseTools.UnitTests.InstallerManifestYaml
{
    public class InstallerManifestEntryGeneratorTests
    {
        [Theory, AutoMoqData]
        public void Generate_CreatesEntry(
            [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
            [Frozen] Mock<IPlayniteSdkVersionParser> playniteSdkVersionParserMock,
            [Frozen] Mock<IExtensionPackageNameGuesser> extensionPackageNameGuesserMock,
            InstallerManifestEntryGenerator sut)
        {
            // Arrange
            dateTimeProviderMock.Setup(x => x.Now).Returns(DateTime.Parse("2020-03-24"));
            playniteSdkVersionParserMock.Setup(x => x.GetVersion()).Returns("9.8.7");
            extensionPackageNameGuesserMock
                .Setup(x => x.GetName(It.Is<string>(v => v == "2.3.4")))
                .Returns("SparrowBrain_GGDeals_2_3_4.pext");
            var changeEntry = new ChangelogEntry("2.3.4", new[] { "- Change 1", "- Change 22", "- Fix important!" });
            var expected = @"  - Version: 2.3.4
    RequiredApiVersion: 9.8.7
    ReleaseDate: 2020-03-24
    PackageUrl: https://github.com/SparrowBrain/Playnite.GGDeals/releases/download/v2.3.4/SparrowBrain_GGDeals_2_3_4.pext
    Changelog:
      - Change 1
      - Change 22
      - Fix important!
";

            // Act
            var result = sut.Generate(changeEntry);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}