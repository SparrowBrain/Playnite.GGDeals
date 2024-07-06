using AutoFixture.Xunit2;
using GGDeals.Menu.Failures;
using GGDeals.Services;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GGDeals.Settings;
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
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			settings.AuthenticationToken = authenticationToken;
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games, ct);

			// Assert
			notificationsApiMock.Verify(
				x => x.Add("gg-deals-auth-error", It.IsAny<string>(),
					It.Is<NotificationType>(n => n == NotificationType.Info)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_AddsToFailuresAllUnprocessedGames_WhenAuthenticationTokenIsEmpty(
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			[Frozen] GGDealsSettings settings,
			Exception exception,
			List<Game> games,
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			settings.AuthenticationToken = string.Empty;

			// Act
			await sut.AddGamesToLibrary(games, ct);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.NotProcessed) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
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
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			addGamesServiceMock.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games, ct);

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
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			addGamesServiceMock.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

			// Act
			await sut.AddGamesToLibrary(games, ct);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.NotProcessed) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
				Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsInfoNotification_WhenGamePageCouldNotBeFound(
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Game> games,
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Missed }));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games, ct);

			// Assert
			notificationsApiMock.Verify(
				x => x.Add("gg-deals-gamepagenotfound", It.IsAny<string>(),
					It.Is<NotificationType>(n => n == NotificationType.Info)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_AddToFailures_WhenGamePageCouldNotBeFound(
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			List<Game> games,
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Missed }));

			// Act
			await sut.AddGamesToLibrary(games, ct);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.Missed) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
				Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsNoNotification_WhenGameIsAdded(
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Game> games,
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Added }));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games, ct);

			// Assert
			notificationsApiMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()), Times.Never);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_RemovesFromFailures_WhenGameIsAdded(
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			List<Game> games,
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.Added }));

			// Act
			await sut.AddGamesToLibrary(games, ct);

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
			CancellationToken ct,
			GGDealsService sut)
		{
			// Arrange
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => new AddResult() { Result = AddToCollectionResult.SkippedDueToLibrary }));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games, ct);

			// Assert
			notificationsApiMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()), Times.Never);
		}
	}
}