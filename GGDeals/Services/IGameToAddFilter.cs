using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public interface IGameToAddFilter
	{
		bool ShouldTryAddGame(Game game, out AddToCollectionResult? status);
	}
}