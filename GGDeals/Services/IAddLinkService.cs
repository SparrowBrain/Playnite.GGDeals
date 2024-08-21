using Playnite.SDK.Models;

namespace GGDeals.Services
{
	public interface IAddLinkService
	{
		void AddLink(Game game, string url);
	}
}