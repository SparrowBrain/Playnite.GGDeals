using GGDeals.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GGDeals.Progress.MVVM;

namespace GGDeals.Services
{
    public interface IAddGamesService
    {
        Task<IDictionary<Guid, AddResult>> TryAddToCollection(IReadOnlyCollection<Game> games,
            Action<float> reportProgress, CancellationToken ct);
    }
}