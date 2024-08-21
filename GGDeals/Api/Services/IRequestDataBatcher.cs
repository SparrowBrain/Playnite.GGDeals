using System.Collections.Generic;
using GGDeals.Api.Models;

namespace GGDeals.Api.Services
{
	public interface IRequestDataBatcher
	{
		IEnumerable<string> CreateDataJsons(IReadOnlyCollection<GameWithLauncher> games);
	}
}