using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GGDeals.Services
{
	public class PersistentProcessingQueue
	{
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly IQueuePersistence _queuePersistence;
		private readonly Func<IReadOnlyCollection<Guid>, Task> _action;

		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		private readonly ConcurrentQueue<Guid> _gameIds;

		public PersistentProcessingQueue(IQueuePersistence queuePersistence, Func<IReadOnlyCollection<Guid>, Task> action)
		{
			_queuePersistence = queuePersistence;
			_action = action;

			_gameIds = new ConcurrentQueue<Guid>(_queuePersistence.Load().Result);
		}

		public async Task Enqueue(IReadOnlyCollection<Guid> gameIds)
		{
			foreach (var gameId in gameIds)
			{
				_gameIds.Enqueue(gameId);
			}

			await _queuePersistence.Save(_gameIds.ToArray());
		}

		public void ProcessInBackground()
		{
			Task.Run(async () =>
			{
				await _semaphore.WaitAsync();

				var gameIds = new List<Guid>();
				try
				{
					while (_gameIds.TryDequeue(out var gameId))
					{
						gameIds.Add(gameId);
					}

					await _action(gameIds);
					await _queuePersistence.Save(_gameIds);
				}
				catch (Exception e)
				{
					_logger.Error(e, "Failed to process queue.");
					await Enqueue(gameIds);
				}
				finally
				{
					_semaphore.Release();
				}
			});
		}
	}
}