using System;
using GGDeals.Api.Models;
using Playnite.SDK.Models;

namespace GGDeals.Api.Services
{
	public interface ILibraryToGGLauncherMap
	{
		GGLauncher GetGGLauncher(Game game);

		GGLauncher GetGGLauncher(Guid pluginId);
	}
}