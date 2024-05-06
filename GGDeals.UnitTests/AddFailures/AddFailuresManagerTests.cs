using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GGDeals.AddFailures;
using GGDeals.Services;
using Moq;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.AddFailures
{
    public class AddFailuresManagerTests
    {
        [Theory]
        [AutoMoqData]
        public async Task AddFailures_ThrowsException_WhenLoadingFails(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> failures,
            Exception expected,
            AddFailuresManager sut)
        {
            // Arrange
            addFailuresFileServiceMock.Setup(x => x.Load()).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.AddFailures(failures));

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddFailures_LoadsFromFile_WhenNotInitialized(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> failuresOnFile,
            Dictionary<Guid, AddToCollectionResult> failures,
            AddFailuresManager sut)
        {
            // Arrange
            addFailuresFileServiceMock.Setup(x => x.Load()).ReturnsAsync(failuresOnFile);

            // Act
            await sut.AddFailures(failures);

            // Assert
            addFailuresFileServiceMock.Verify(x => x.Load(), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddFailures_SavesToFile(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> failuresOnFile,
            Dictionary<Guid, AddToCollectionResult> failures,
            AddFailuresManager sut)
        {
            // Arrange
            addFailuresFileServiceMock.Setup(x => x.Load()).ReturnsAsync(failuresOnFile);

            // Act
            await sut.AddFailures(failures);

            // Assert
            addFailuresFileServiceMock.Verify(
                x => x.Save(It.Is<Dictionary<Guid, AddToCollectionResult>>(d =>
                    failures.All(f => d.ContainsKey(f.Key)) && failuresOnFile.All(f => d.ContainsKey(f.Key)))), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddFailures_DoesNotSaveToFile_IfFailureAlreadyAdded(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> failuresOnFile,
            AddFailuresManager sut)
        {
            // Arrange
            var failures = new Dictionary<Guid, AddToCollectionResult>
                {
                    { failuresOnFile.Last().Key, failuresOnFile.Last().Value}
                };
            addFailuresFileServiceMock.Setup(x => x.Load()).ReturnsAsync(failuresOnFile);

            // Act
            await sut.AddFailures(failures);

            // Assert
            addFailuresFileServiceMock.Verify(
                x => x.Save(It.IsAny<Dictionary<Guid, AddToCollectionResult>>()), Times.Never);
        }

        [Theory]
        [AutoMoqData]
        public async Task RemoveFailures_ThrowsException_WhenSavingFails(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> failuresOnFile,
            Exception expected,
            AddFailuresManager sut)
        {
            // Arrange
            var gameId = failuresOnFile.Keys.Last();
            addFailuresFileServiceMock.Setup(x => x.Load()).ReturnsAsync(failuresOnFile);
            addFailuresFileServiceMock.Setup(x => x.Save(It.IsAny<Dictionary<Guid, AddToCollectionResult>>())).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(() => sut.RemoveFailures(new[] { gameId }));

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task RemoveFailures_DoesNotSaveToFile_WhenIdIsNotAFailure(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> failuresOnFile,
            Guid gameId,
            AddFailuresManager sut)
        {
            // Arrange
            addFailuresFileServiceMock.Setup(x => x.Load()).ReturnsAsync(failuresOnFile);

            // Act
            await sut.RemoveFailures(new[] { gameId });

            // Assert
            addFailuresFileServiceMock.Verify(
                x => x.Save(It.Is<Dictionary<Guid, AddToCollectionResult>>(d =>
                    !d.ContainsKey(gameId) && d.ContainsKey(failuresOnFile.First().Key))), Times.Never);
        }

        [Theory]
        [AutoMoqData]
        public async Task RemoveFailures_SavesToFile(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> failuresOnFile,
            AddFailuresManager sut)
        {
            // Arrange
            var gameId = failuresOnFile.Keys.Last();
            addFailuresFileServiceMock.Setup(x => x.Load()).ReturnsAsync(failuresOnFile);

            // Act
            await sut.RemoveFailures(new[] { gameId });

            // Assert
            addFailuresFileServiceMock.Verify(
                x => x.Save(It.Is<Dictionary<Guid, AddToCollectionResult>>(d =>
                    !d.ContainsKey(gameId) && d.ContainsKey(failuresOnFile.First().Key))), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetFailures_ThrowsException_WhenLoadingFromFileThrowsException(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Exception expected,
            AddFailuresManager sut)
        {
            // Arrange
            addFailuresFileServiceMock.Setup(x => x.Load()).ThrowsAsync(expected);

            // Act
            var actual = await Record.ExceptionAsync(sut.GetFailures);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetFailures_ReturnsFailures(
            [Frozen] Mock<IAddFailuresFileService> addFailuresFileServiceMock,
            Dictionary<Guid, AddToCollectionResult> expected,
            AddFailuresManager sut)
        {
            // Arrange
            addFailuresFileServiceMock.Setup(x => x.Load()).ReturnsAsync(expected);

            // Act
            var actual = await sut.GetFailures();

            // Assert
            Assert.NotSame(expected, actual);
            Assert.Equal(expected, actual);
        }
    }
}