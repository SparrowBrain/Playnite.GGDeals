using System.Threading;
using System.Threading.Tasks;
using GGDeals.Api.Models;

namespace GGDeals.Api.Services
{
	public interface IGGDealsApiClient
	{
		Task<ImportResponse> ImportGames(ImportRequest request, CancellationToken ct);
	}
}