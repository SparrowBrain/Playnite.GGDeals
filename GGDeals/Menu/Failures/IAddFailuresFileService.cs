using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGDeals.Services;

namespace GGDeals.Menu.Failures
{
    public interface IAddFailuresFileService
    {
        Task<Dictionary<Guid, AddToCollectionResult>> Load();

        Task Save(Dictionary<Guid, AddToCollectionResult> failures);
    }
}