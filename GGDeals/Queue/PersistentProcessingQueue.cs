using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;

namespace GGDeals.Queue
{
	public class PersistentProcessingQueue
	{
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly IQueuePersistence _queuePersistence;
		private readonly Func<IReadOnlyCollection<Guid>, Task> _action;

		private readonly SemaphoreSlim _initSemaphore = new SemaphoreSlim(1, 1);
		private readonly SemaphoreSlim _processingSemaphore = new SemaphoreSlim(1, 1);
		private ConcurrentQueue<Guid> _gameIds;

		public PersistentProcessingQueue(IQueuePersistence queuePersistence, Func<IReadOnlyCollection<Guid>, Task> action)
		{
			_queuePersistence = queuePersistence;
			_action = action;
		}

		public async Task Enqueue(IReadOnlyCollection<Guid> gameIds)
		{
			await EnsureInitialized();
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
				await EnsureInitialized();
				await _processingSemaphore.WaitAsync();

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
					_processingSemaphore.Release();
				}
			});
		}

		private async Task EnsureInitialized()
		{
			if (_gameIds != null)
			{
				return;
			}

			await _initSemaphore.WaitAsync();
			if (_gameIds != null)
			{
				return;
			}

			var gameIds = await _queuePersistence.Load();
			_gameIds = new ConcurrentQueue<Guid>(gameIds);
		}
	}
}