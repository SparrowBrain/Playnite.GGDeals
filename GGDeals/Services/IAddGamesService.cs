using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public interface IAddGamesService
	{
		Task<IDictionary<Guid, AddResult>> TryAddToCollection(IReadOnlyCollection<Game> games, CancellationToken ct);
	}
}