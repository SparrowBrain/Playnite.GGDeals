using GGDeals.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGDeals.Api.Services
{
    public interface IGGDealsApiClient : IDisposable
    {
        Task<ImportResponse> ImportGames(ImportRequest request, CancellationToken ct);
    }
}