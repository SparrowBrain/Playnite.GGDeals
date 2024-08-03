using AutoFixture.Xunit2;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Net.Mime;
using System.Windows.Threading;
using GGDeals.Services;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
	public class AddLinkServiceTests
	{

		[Theory]
		[InlineAutoMoqData(null)]
		[InlineAutoMoqData("")]
		public void AddLink_ThrowsException_WhenUrlIsEmpty(
			string url,
			Game game,
			AddLinkService sut)
		{
			// Act
			var exception = Record.Exception(() => sut.AddLink(game, url));

			// Assert
			Assert.NotNull(exception);
			Assert.IsType<ArgumentException>(exception);
		}

		[Theory]
		[AutoMoqData]
		public void AddLink_AddsLink_ToExistingList(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IMainViewAPI> mainViewApiMock,
			string url,
			Game game,
			AddLinkService sut)
		{
			// Arrange
			playniteApiMock.SetupGet(x => x.MainView).Returns(mainViewApiMock.Object);
			mainViewApiMock.SetupGet(x => x.UIDispatcher).Returns(Dispatcher.CurrentDispatcher);

			// Act
			sut.AddLink(game, url);

			// Assert
			var link = Assert.Single(game.Links, l => l.Url == url);
			Assert.Equal(url, link.Url);
			Assert.Equal("GG.deals", link.Name);
			playniteApiMock.Verify(a => a.Database.Games.Update(It.Is<Game>(g => g == game)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public void AddLink_AddsLink_ToEmptyList(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IMainViewAPI> mainViewApiMock,
			string url,
			Game game,
			AddLinkService sut)
		{
			// Arrange
			game.Links = null;
			playniteApiMock.SetupGet(x => x.MainView).Returns(mainViewApiMock.Object);
			mainViewApiMock.SetupGet(x => x.UIDispatcher).Returns(Dispatcher.CurrentDispatcher);

			// Act
			sut.AddLink(game, url);

			// Assert
			var link = Assert.Single(game.Links, l => l.Url == url);
			Assert.Equal(url, link.Url);
			Assert.Equal("GG.deals", link.Name);
			playniteApiMock.Verify(a => a.Database.Games.Update(It.Is<Game>(g => g == game)), Times.Once);
		}

		[Theory]
		[AutoMoqData]
		public void AddLink_DoesNotAddLink_WhenLinkIsAlreadyInList(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			string url,
			Game game,
			AddLinkService sut)
		{
			// Arrange
			game.Links.Add(new Link() { Url = url });

			// Act
			sut.AddLink(game, url);

			// Assert
			var link = Assert.Single(game.Links, l => l.Url == url);
			Assert.Equal(url, link.Url);
			playniteApiMock.Verify(a => a.Database.Games.Update(It.IsAny<Game>()), Times.Never);
		}
	}
}