using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGDeals.Services;

namespace GGDeals.Menu.Failures
{
    public interface IAddFailuresManager
    {
        Task AddFailures(IDictionary<Guid, AddToCollectionResult> failures);
        Task RemoveFailures(IReadOnlyCollection<Guid> gameIds);
        Task<Dictionary<Guid, AddToCollectionResult>> GetFailures();
    }
}