using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public interface IGameStatusService
	{
		AddToCollectionResult GetStatus(Game game);

		void UpdateStatus(Game game, AddToCollectionResult status);
	}
}