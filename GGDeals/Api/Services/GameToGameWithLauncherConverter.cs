using GGDeals.Api.Models;
using GGDeals.Settings;
using Playnite.SDK.Models;

namespace GGDeals.Api.Services
{
	public class GameToGameWithLauncherConverter : IGameToGameWithLauncherConverter
	{
		private readonly ILibraryToGGLauncherMap _libraryToGGLauncherMap;
		private readonly GGDealsSettings _settings;

		public GameToGameWithLauncherConverter(ILibraryToGGLauncherMap libraryToGGLauncherMap, GGDealsSettings settings)
		{
			_libraryToGGLauncherMap = libraryToGGLauncherMap;
			_settings = settings;
		}

		public GameWithLauncher GetGameWithLauncher(Game game)
		{
			if (!_settings.LibraryMapOverride.TryGetValue(game.PluginId, out var launcher))
			{
				launcher = _libraryToGGLauncherMap.GetGGLauncher(game.PluginId);
			}

			return GameWithLauncher.FromGame(game, launcher);
		}
	}
}