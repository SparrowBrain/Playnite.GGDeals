using GGDeals.Api.Models;
using Playnite.SDK.Models;

namespace GGDeals.Api.Services
{
	public interface IGameToGameWithLauncherConverter
	{
		GameWithLauncher GetGameWithLauncher(Game game);
	}
}