using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GGDeals.Menu.Failures.File;
using GGDeals.Services;
using Playnite.SDK;

namespace GGDeals.Menu.Failures
{
    public class AddFailuresManager : IAddFailuresManager, IDisposable
    {
        private const double SemaphoreTimeoutSeconds = 10;
        private readonly ILogger _logger = LogManager.GetLogger();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IAddFailuresFileService _addFailuresFileService;

        private Dictionary<Guid, AddResult> _failures;

        public AddFailuresManager(IAddFailuresFileService addFailuresFileService)
        {
            _addFailuresFileService = addFailuresFileService;
        }

        public async Task AddFailures(IDictionary<Guid, AddResult> failures)
        {
            try
            {
                await _semaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreTimeoutSeconds));
                await EnsureFailuresAreLoaded();

                if (failures.All(f => _failures.ContainsKey(f.Key) && _failures[f.Key] == f.Value))
                {
                    return;
                }

                foreach (var failure in failures)
                {
                    _failures.Add(failure.Key, failure.Value);
                }

                await _addFailuresFileService.Save(_failures);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure while adding failure.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RemoveFailures(IReadOnlyCollection<Guid> gameIds)
        {
            try
            {
                await _semaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreTimeoutSeconds));
                await EnsureFailuresAreLoaded();
                if (gameIds.All(g => !_failures.ContainsKey(g)))
                {
                    return;
                }

                foreach (var gameId in gameIds)
                {
                    _failures.Remove(gameId);
                }

                await _addFailuresFileService.Save(_failures);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure while removing failure.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Dictionary<Guid, AddResult>> GetFailures()
        {
            try
            {
                await _semaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreTimeoutSeconds));
                await EnsureFailuresAreLoaded();

                return new Dictionary<Guid, AddResult>(_failures);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failure while getting failures.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        private async Task EnsureFailuresAreLoaded()
        {
            if (_failures == null)
            {
                _failures = await _addFailuresFileService.Load();
            }
        }
    }
}