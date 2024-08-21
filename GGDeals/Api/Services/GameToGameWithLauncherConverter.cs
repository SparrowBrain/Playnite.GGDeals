using GGDeals.Api.Models;
using Playnite.SDK.Models;

namespace GGDeals.Api.Services
{
	public class GameToGameWithLauncherConverter : IGameToGameWithLauncherConverter
	{
		private readonly ILibraryToGGLauncherMap _libraryToGGLauncherMap;

		public GameToGameWithLauncherConverter(ILibraryToGGLauncherMap libraryToGGLauncherMap)
		{
			_libraryToGGLauncherMap = libraryToGGLauncherMap;
		}

		public GameWithLauncher GetGameWithLauncher(Game game)
		{
			var launcher = _libraryToGGLauncherMap.GetGGLauncher(game);
			return GameWithLauncher.FromGame(game, launcher);
		}
	}
}