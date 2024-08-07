using AutoFixture.Xunit2;
using GGDeals.Services;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
using GGDeals.Models;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
	public class GameStatusServiceTests
	{
		[Theory]
		[AutoMoqData]
		public void GetStatus_ReturnsNew_WhenTheresNoGGDealsTag(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Tag> tags,
			Game game,
			GameStatusService sut)
		{
			// Arrange
			playniteApiMock.SetupGet(a => a.Database.Tags).Returns(new TestableItemCollection<Tag>(tags));

			// Act
			var result = sut.GetStatus(game);

			// Assert
			Assert.Equal(AddToCollectionResult.New, result);
		}

		[Theory]
		[InlineAutoMoqData("[GGDeals] Synced", AddToCollectionResult.Synced)]
		[InlineAutoMoqData("[GGDeals] NotFound", AddToCollectionResult.NotFound)]
		[InlineAutoMoqData("[GGDeals] Ignored", AddToCollectionResult.Ignored)]
		public void GetStatus_ReturnsStatus_DependingOnTag(
			string tagName,
			AddToCollectionResult expected,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Tag ggDealsTag,
			List<Tag> tags,
			Game game)
		{
			// Arrange
			ggDealsTag.Name = tagName;
			tags.Add(ggDealsTag);
			game.TagIds.Add(ggDealsTag.Id);
			playniteApiMock.Setup(a => a.Database.Tags).Returns(new TestableItemCollection<Tag>(tags));
			var sut = CreateSut(playniteApiMock);

			// Act
			var result = sut.GetStatus(game);

			// Assert
			Assert.Equal(expected, result);
		}

		[Theory]
		[AutoData]
		public void GetStatus_ThrowsException_WhenTagIsNotMapped(
			string tagName,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Tag ggDealsTag,
			List<Tag> tags,
			Game game)
		{
			// Arrange
			tagName = "[GGDeals] " + tagName;
			ggDealsTag.Name = tagName;
			tags.Add(ggDealsTag);
			game.TagIds.Add(ggDealsTag.Id);
			playniteApiMock.Setup(a => a.Database.Tags).Returns(new TestableItemCollection<Tag>(tags));
			var sut = CreateSut(playniteApiMock);

			// Act
			var actual = Record.Exception(() => sut.GetStatus(game));

			// Assert
			Assert.NotNull(actual);
		}

		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Synced, "[GGDeals] Synced")]
		[InlineAutoMoqData(AddToCollectionResult.Synced, "[GGDeals] Synced")]
		[InlineAutoMoqData(AddToCollectionResult.NotFound, "[GGDeals] NotFound")]
		[InlineAutoMoqData(AddToCollectionResult.Ignored, "[GGDeals] Ignored")]
		public void UpdateStatus_AddsTag_DependingOnStatus(
			AddToCollectionResult status,
			string tagName,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Tag ggDealsTag,
			List<Tag> tags,
			List<Game> games,
			Game game)
		{
			// Arrange
			ggDealsTag.Name = tagName;
			tags.Add(ggDealsTag);
			playniteApiMock.Setup(a => a.Database.Tags).Returns(new TestableItemCollection<Tag>(tags));
			var gamesDatabase = new TestableItemCollection<Game>(games);
			playniteApiMock.Setup(a => a.Database.Games).Returns(gamesDatabase);
			var sut = CreateSut(playniteApiMock);

			// Act
			sut.UpdateStatus(game, status);

			// Assert
			Assert.Contains(ggDealsTag.Id, game.TagIds);
			Assert.Equal(1, gamesDatabase.UpdateCount);
		}

		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Synced, "[GGDeals] Synced")]
		public void UpdateStatus_CreatesNewTag_IfOneDoesNotExists(
			AddToCollectionResult status,
			string tagName,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			List<Tag> tags,
			Game game)
		{
			// Arrange
			playniteApiMock.Setup(a => a.Database.Tags).Returns(new TestableItemCollection<Tag>(tags));
			var sut = CreateSut(playniteApiMock);

			// Act
			sut.UpdateStatus(game, status);

			// Assert
			Assert.Contains(tags, t => t.Name == tagName);
		}

		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Synced, "[GGDeals] Synced", "[GGDeals] NotFound")]
		[InlineAutoMoqData(AddToCollectionResult.NotFound, "[GGDeals] NotFound", "[GGDeals] Synced")]
		[InlineAutoMoqData(AddToCollectionResult.Ignored, "[GGDeals] Ignored", "[GGDeals] NotFound")]
		public void UpdateStatus_RetainsOneTag_IfAnotherTagAlreadyExists(
			AddToCollectionResult status,
			string expectedTagName,
			string oldTagName,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Tag expectedTag,
			Tag oldTag,
			List<Tag> tags,
			Game game)
		{
			// Arrange
			expectedTag.Name = expectedTagName;
			oldTag.Name = oldTagName;
			tags.Add(expectedTag);
			tags.Add(oldTag);
			game.TagIds.Add(oldTag.Id);
			playniteApiMock.Setup(a => a.Database.Tags).Returns(new TestableItemCollection<Tag>(tags));
			var sut = CreateSut(playniteApiMock);

			// Act
			sut.UpdateStatus(game, status);

			// Assert
			Assert.Single(game.TagIds, id => tags.SingleOrDefault(t => t.Id == id)?.Name.StartsWith("[GGDeals]") ?? false);
			Assert.Contains(expectedTag.Id, game.TagIds);
		}

		[Theory]
		[InlineAutoMoqData(AddToCollectionResult.Synced, "[GGDeals] Synced")]
		[InlineAutoMoqData(AddToCollectionResult.NotFound, "[GGDeals] NotFound")]
		[InlineAutoMoqData(AddToCollectionResult.Ignored, "[GGDeals] Ignored")]
		public void UpdateStatus_RetainsOneTag_IfSameTagExists(
			AddToCollectionResult status,
			string expectedTagName,
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			Tag expectedTag,
			List<Tag> tags,
			Game game)
		{
			// Arrange
			expectedTag.Name = expectedTagName;
			tags.Add(expectedTag);
			game.TagIds.Add(expectedTag.Id);
			playniteApiMock.Setup(a => a.Database.Tags).Returns(new TestableItemCollection<Tag>(tags));
			var sut = CreateSut(playniteApiMock);

			// Act
			sut.UpdateStatus(game, status);

			// Assert
			Assert.Single(game.TagIds, id => tags.SingleOrDefault(t => t.Id == id)?.Name.StartsWith("[GGDeals]") ?? false);
			Assert.Contains(expectedTag.Id, game.TagIds);
		}

		private GameStatusService CreateSut(Mock<IPlayniteAPI> playniteApiMock)
		{
			return new GameStatusService(playniteApiMock.Object);
		}
	}
}