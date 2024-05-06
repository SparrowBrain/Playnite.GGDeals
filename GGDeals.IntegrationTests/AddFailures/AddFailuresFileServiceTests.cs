using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.AddFailures;
using GGDeals.Services;
using Newtonsoft.Json;
using Xunit;

namespace GGDeals.IntegrationTests.AddFailures
{
    public class AddFailuresFileServiceTests
    {
        private const string FailuresFilePath = "failures.json";

        [Fact]
        public async Task Load_ReturnsEmptyDictionary_WhenFileDoesNotExist()
        {
            // Arrange
            EnsureFileDoesNotExist();
            var sut = CreateSut();

            // Act
            var result = await sut.Load();

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [AutoData]
        public async Task Load_ReturnsContentsOfFile_WhenFileExists(Dictionary<Guid, AddToCollectionResult> contents)
        {
            // Arrange
            File.WriteAllText(FailuresFilePath, JsonConvert.SerializeObject(contents));
            var sut = CreateSut();

            // Act
            var result = await sut.Load();

            // Assert
            Assert.Equal(contents, result);
        }

        [Theory]
        [AutoData]
        public async Task Save_CreatesNewFile_WhenFileDoesNotExist(
            Dictionary<Guid, AddToCollectionResult> newContents)
        {
            // Arrange
            EnsureFileDoesNotExist();
            var sut = CreateSut();

            // Act
            await sut.Save(newContents);

            // Assert
            var result = ReadFile();
            Assert.Equal(newContents, result);
        }

        [Theory]
        [AutoData]
        public async Task Save_OverwritesFile_WhenFileExists(
            Dictionary<Guid, AddToCollectionResult> originalContents,
            Dictionary<Guid, AddToCollectionResult> newContents)
        {
            // Arrange
            File.WriteAllText(FailuresFilePath, JsonConvert.SerializeObject(originalContents));
            var sut = CreateSut();

            // Act
            await sut.Save(newContents);

            // Assert
            var result = ReadFile();
            Assert.Equal(newContents, result);
        }

        private AddFailuresFileService CreateSut()
        {
            return new AddFailuresFileService(FailuresFilePath);
        }

        private void EnsureFileDoesNotExist()
        {
            if (File.Exists(FailuresFilePath))
            {
                File.Delete(FailuresFilePath);
            }
        }

        private static Dictionary<Guid, AddToCollectionResult> ReadFile()
        {
            return JsonConvert.DeserializeObject<Dictionary<Guid, AddToCollectionResult>>(File.ReadAllText(FailuresFilePath));
        }
    }
}