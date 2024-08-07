using GGDeals.Models;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public interface IGameToAddFilter
	{
		bool ShouldTryAddGame(Game game, out AddResult status);
	}
}