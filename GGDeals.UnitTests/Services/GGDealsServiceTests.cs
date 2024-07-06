using AutoFixture.Xunit2;
using GGDeals.Menu.Failures;
using GGDeals.Services;
using GGDeals.Website;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
	public class GGDealsServiceTests
	{
		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsErrorNotification_WhenNavigateToHomePageThrowsException(
			[Frozen] Mock<IGGWebsite> ggWebsiteMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Exception exception,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			ggWebsiteMock.Setup(x => x.NavigateToHomePage()).ThrowsAsync(exception);
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			notificationsApiMock.Verify(
				x => x.Add("gg-deals-generic-error", It.IsAny<string>(),
					It.Is<NotificationType>(n => n == NotificationType.Error)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsErrorNotification_WhenCheckingIsUserLoggedInThrowsException(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Exception exception,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ThrowsAsync(exception);
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			notificationsApiMock.Verify(
				x => x.Add("gg-deals-generic-error", It.IsAny<string>(),
					It.Is<NotificationType>(n => n == NotificationType.Error)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsInfoNotification_WhenIsUserLoggedInReturnsFalse(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(false);
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			notificationsApiMock.Verify(
				x => x.Add("gg-deals-auth-error", It.IsAny<string>(),
					It.Is<NotificationType>(n => n == NotificationType.Info)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_AddsToFailuresAllUnprocessedGames_WhenIsUserLoggedInReturnsFalse(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			Exception exception,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(false);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.NotProcessed) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
				Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsErrorNotification_WhenTryAddToCollectionThrowsException(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Exception exception,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>())).ThrowsAsync(exception);
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			notificationsApiMock.Verify(
				x => x.Add("gg-deals-generic-error", It.IsAny<string>(),
					It.Is<NotificationType>(n => n == NotificationType.Error)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_AddsToFailuresAllUnprocessedGames_WhenTryAddToCollectionThrowsException(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			Exception exception,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>())).ThrowsAsync(exception);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f => f.Values.All(v => v.Result == AddToCollectionResult.NotProcessed) && f.Keys.SequenceEqual(games.Select(g => g.Id)))),
				Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsInfoNotification_WhenGamePageCouldNotBeFound(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.PageNotFound));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			notificationsApiMock.Verify(
				x => x.Add("gg-deals-gamepagenotfound", It.IsAny<string>(),
					It.Is<NotificationType>(n => n == NotificationType.Info)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_AddToFailures_WhenGamePageCouldNotBeFound(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.PageNotFound));

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.AddFailures(It.Is<IDictionary<Guid, AddResult>>(f =>  f.Values.All(v=> v.Result == AddToCollectionResult.PageNotFound) && f.Keys.SequenceEqual(games.Select(g=>g.Id)))),
					 Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsNoNotification_WhenGameIsAdded(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.Added));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			notificationsApiMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()), Times.Never);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_RemovesFromFailures_WhenGameIsAdded(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.Added));

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.RemoveFailures(It.Is<IReadOnlyCollection<Guid>>(f => games.Select(g => g.Id).Contains(f))),
				Times.Once());
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_ShowsNoNotification_WhenLibraryIsIgnored(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.SkippedDueToLibrary));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			notificationsApiMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()), Times.Never);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_DoesNotAddAFailure_WhenLibraryIsIgnored(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.SkippedDueToLibrary));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			addFailuresManagerMock.Verify(x => x.AddFailures(It.IsAny<Dictionary<Guid, AddResult>>()), Times.Never);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_RemovesFailures_WhenLibraryIsIgnored(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.SkippedDueToLibrary));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.RemoveFailures(
					It.Is<IReadOnlyCollection<Guid>>(f => games.Select(g => g.Id).ToList().SequenceEqual(f))),
				Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public async Task AddGamesToLibrary_RemovesFailures_WhenGameIsAlreadyOwned(
			[Frozen] Mock<IHomePage> homePageMock,
			[Frozen] Mock<IAddGamesService> addGamesServiceMock,
			[Frozen] Mock<INotificationsAPI> notificationsApiMock,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IAddFailuresManager> addFailuresManagerMock,
			List<Game> games,
			GGDealsService sut)
		{
			// Arrange
			homePageMock.Setup(x => x.IsUserLoggedIn()).ReturnsAsync(true);
			addGamesServiceMock
				.Setup(x => x.TryAddToCollection(It.IsAny<IReadOnlyCollection<Game>>()))
				.ReturnsAsync(games.ToDictionary(x => x.Id, x => AddToCollectionResult.AlreadyOwned));
			playniteApiMock.Setup(x => x.Notifications).Returns(notificationsApiMock.Object);

			// Act
			await sut.AddGamesToLibrary(games);

			// Assert
			addFailuresManagerMock.Verify(
				x => x.RemoveFailures(
					It.Is<IReadOnlyCollection<Guid>>(f => games.Select(g => g.Id).ToList().SequenceEqual(f))),
				Times.Once);
		}
	}
}