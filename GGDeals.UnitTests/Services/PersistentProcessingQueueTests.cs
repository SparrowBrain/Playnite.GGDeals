using AutoFixture.Xunit2;
using GGDeals.Services;
using Moq;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestTools.Shared;
using Xunit;

namespace GGDeals.UnitTests.Services
{
	public class PersistentProcessingQueueTests
	{
		[Theory]
		[AutoMoqData]
		public async Task Enqueue_WritesToFile_WhenItemsAdded(
			[Frozen] Mock<IQueuePersistence> queuePersistenceMock,
			List<Guid> gameIds,
			PersistentProcessingQueue sut)
		{
			// Act
			await sut.Enqueue(gameIds);

			// Assert
			queuePersistenceMock.Verify(x => x.Save(gameIds));
		}

		[Theory]
		[AutoMoqData]
		public async Task Enqueue_WritesToFileAllGames_WhenItemsAddedAndGamesWereAlreadyInTheQueue(
			[Frozen] Mock<IQueuePersistence> queuePersistenceMock,
			List<Guid> oldGameIds,
			List<Guid> newGameIds,
			PersistentProcessingQueue sut)
		{
			// Arrange
			await sut.Enqueue(oldGameIds);

			// Act
			await sut.Enqueue(newGameIds);

			// Assert
			queuePersistenceMock.Verify(x =>
				x.Save(
					It.Is<IReadOnlyCollection<Guid>>(g => oldGameIds.Concat(newGameIds).All(g.Contains))));
		}

		[Theory]
		[AutoMoqData]
		public async Task Constructor_LoadsFromFile_WhenIsCreated(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IQueuePersistence> queuePersistenceMock,
			List<Guid> oldGameIds,
			List<Guid> newGameIds)
		{
			// Arrange
			queuePersistenceMock.Setup(x => x.Load()).ReturnsAsync(oldGameIds);

			// Act
			var sut = new PersistentProcessingQueue(queuePersistenceMock.Object, x => Task.CompletedTask);
			await sut.Enqueue(newGameIds);

			// Assert
			queuePersistenceMock.Verify(x =>
				x.Save(
					It.Is<IReadOnlyCollection<Guid>>(g => oldGameIds.Concat(newGameIds).All(g.Contains))));
		}

		[Theory]
		[AutoMoqData]
		public async Task Process_ExecutesTheAction(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IQueuePersistence> queuePersistenceMock)
		{
			// Arrange
			var actionCalled = false;
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock.Object, x =>
			{
				actionCalled = true;
				semaphore.Release();
				return Task.CompletedTask;
			});

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
			Assert.True(actionCalled);
		}

		[Theory]
		[AutoMoqData]
		public async Task Process_SavesFile_WhenActionExecutes(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IQueuePersistence> queuePersistenceMock,
			List<Guid> gameIds)
		{
			// Arrange
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock.Object, x =>
			{
				semaphore.Release();
				return Task.CompletedTask;
			});
			await sut.Enqueue(gameIds);

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
			queuePersistenceMock.Verify(x => x.Save(It.Is<IReadOnlyCollection<Guid>>(c => c.Count == 0)));
		}

		[Theory]
		[AutoMoqData]
		public async Task Process_DoesNotSave_WhenActionFails(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IQueuePersistence> queuePersistenceMock,
			List<Guid> gameIds)
		{
			// Arrange
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock.Object, x => throw new Exception());
			await sut.Enqueue(gameIds);

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(1));
			queuePersistenceMock.Verify(x => x.Save(It.Is<IReadOnlyCollection<Guid>>(c => c.Count == 0)), Times.Never);
		}

		[Theory]
		[AutoMoqData]
		public async Task Process_ItemsStayInQueue_WhenActionFails(
			[Frozen] Mock<IPlayniteAPI> playniteApiMock,
			[Frozen] Mock<IQueuePersistence> queuePersistenceMock,
			List<Guid> gameIds)
		{
			// Arrange
			var semaphore = new SemaphoreSlim(0, 1);
			var sut = new PersistentProcessingQueue(queuePersistenceMock.Object, x => throw new Exception());
			await sut.Enqueue(gameIds);
			queuePersistenceMock.Reset();

			// Act
			sut.ProcessInBackground();

			// Assert
			await semaphore.WaitAsync(TimeSpan.FromSeconds(1));
			queuePersistenceMock.Verify(x =>
				x.Save(It.Is<IReadOnlyCollection<Guid>>(g => gameIds.All(g.Contains))));
		}
	}
}