using AutoFixture.Xunit2;
using GGDeals.Menu.Failures;
using GGDeals.Models;
using GGDeals.Services;
using GGDeals.Settings;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GGDeals.Progress.MVVM;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
    public class GGDealsServiceTests
    {
        [Theory]
        [InlineAutoMoqData(null)]
        [InlineAutoMoqData("")]
        [InlineAutoMoqData("   ")]
        public async Task AddGamesToLibrary_ShowsInfoNotification_WhenAuthenticationTokenIsEmpty(
            string authenticationToken,
            [Frozen] Mock<INotificationsAPI> notificationsApiMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            [Frozen] GGDealsSettings settings,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            settings.AuthenticationToken = authenticationToken;
            playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            notificationsApiMock.Verify(
                x => x.Add(It.Is<NotificationMessage>(n =>
                    n.Id == "gg-deals-auth-error" && n.Type == NotificationType.Info)), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_AddsToFailuresAllUnprocessedGames_WhenAuthenticationTokenIsEmpty(
            [Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
            [Frozen] GGDealsSettings settings,
            Exception exception,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            settings.AuthenticationToken = string.Empty;

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            addFailuresManagerMock.Verify(
                x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.New) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
                Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_ShowsErrorNotification_WhenTryAddToCollectionThrowsException(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<INotificationsAPI> notificationsApiMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            Exception exception,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            notificationsApiMock.Verify(
                x => x.Add("gg-deals-generic-error", It.IsAny<string>(),
                    It.Is<NotificationType>(n => n == NotificationType.Error)), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_AddsToFailuresAllUnprocessedGames_WhenTryAddToCollectionThrowsException(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
            Exception exception,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            addFailuresManagerMock.Verify(
                x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.New) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
                Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_ShowsInfoNotification_WhenGameMatchIsAMiss(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<INotificationsAPI> notificationsApiMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.NotFound }));
            playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            notificationsApiMock.Verify(
                x => x.Add(It.Is<NotificationMessage>(m =>
                    m.Id == "gg-deals-gamepagenotfound" && m.Type == NotificationType.Info)), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_AddToFailures_WhenGameMatchIsAMiss(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.NotFound }));

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            addFailuresManagerMock.Verify(
                x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.NotFound) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
                Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_ShowsInfoNotification_WhenApiReturnsError(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<INotificationsAPI> notificationsApiMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Error }));
            playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            notificationsApiMock.Verify(
                x => x.Add(It.Is<NotificationMessage>(m =>
                    m.Id == "gg-deals-api-error" && m.Type == NotificationType.Info)), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_AddToFailures_WhenApiReturnsError(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
            List<Game> games,
            string message,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id,
                    x => new AddResult() { Result = AddToCollectionResult.Error, Message = message }));

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            addFailuresManagerMock.Verify(
                x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f =>
                    f.Values.All(v => v.Result == AddToCollectionResult.Error && v.Message == message) &&
                    f.Keys.SequenceEqual(games.Select(g => g.Id)))), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_ShowsInfoNotification_WhenGameMatchIsIgnored(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<INotificationsAPI> notificationsApiMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Ignored }));
            playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            notificationsApiMock.Verify(
                x => x.Add("gg-deals-ignored", It.IsAny<string>(),
                    It.Is<NotificationType>(n => n == NotificationType.Info)), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_RemovesFromFailures_WhenGameIsIgnored(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Ignored }));

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            addFailuresManagerMock.Verify(
                x => x.RemoveFailures(It.Is<IReadOnlyCollection<Guid>>(f => games.Select(g => g.Id).Contains(f))),
                Times.Once());
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_RemovesFromFailures_WhenGameIsAdded(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Added }));

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            addFailuresManagerMock.Verify(
                x => x.RemoveFailures(It.Is<IReadOnlyCollection<Guid>>(f => games.Select(g => g.Id).Contains(f))),
                Times.Once());
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_ShowsNoNotification_WhenLibraryIsIgnored(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<INotificationsAPI> notificationsApiMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.SkippedDueToLibrary }));
            playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            notificationsApiMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()), Times.Never);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_ShowsInfoNotification_WhenGameIsAdded(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<INotificationsAPI> notificationsApiMock,
            [Frozen] Mock<IPlayniteAPI> playniteApiMock,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Added }));
            playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            notificationsApiMock.Verify(
                x => x.Add("gg-deals-added", It.IsAny<string>(),
                    It.Is<NotificationType>(n => n == NotificationType.Info)), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task AddGamesToLibrary_ProcessAddResults(
            [Frozen] Mock<IAddGamesService> addGamesServiceMock,
            [Frozen] Mock<IAddResultProcessor> addResultProcessorMock,
            AddToCollectionResult addResult,
            List<Game> games,
            Action<float> reportProgress,
            CancellationToken ct,
            GGDealsService sut)
        {
            // Arrange
            var addResults = games.ToDictionary(x => x.Id, x => new AddResult() { Result = addResult });
            addGamesServiceMock
                .Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<Action<float>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(addResults);

            // Act
            await sut.AddGamesToLibrary(games, reportProgress, ct);

            // Assert
            addResultProcessorMock.Verify(
                x => x.Process(It.Is<IReadOnlyCollection<Game>>(g => g == games), It.Is<IDictionary<Guid, AddResult>>(r => r == addResults)),
                Times.Once());
        }
    }
}