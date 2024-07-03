using Playnite.SDK.Models;

namespace GGDeals.Api.Models
{
	public class GameWithLauncher : Game
	{
		public GGLauncher GGLauncher { get; set; }
	}
}