using GGDeals.Api.Models;
using Newtonsoft.Json;
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
			var gameWithLauncher = JsonConvert.DeserializeObject<GameWithLauncher>(JsonConvert.SerializeObject(game));
			gameWithLauncher.GGLauncher = launcher;
			return gameWithLauncher;
		}
	}
}