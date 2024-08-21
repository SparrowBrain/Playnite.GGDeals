using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGDeals.Models;
using GGDeals.Services;

namespace GGDeals.Menu.Failures
{
    public interface IAddFailuresManager
    {
        Task AddFailures(IDictionary<Guid, AddResult> failures);
        Task RemoveFailures(IReadOnlyCollection<Guid> gameIds);
        Task<Dictionary<Guid, AddResult>> GetFailures();
    }
}