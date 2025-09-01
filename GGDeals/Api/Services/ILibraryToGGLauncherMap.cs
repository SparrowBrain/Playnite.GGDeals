using System;
using GGDeals.Api.Models;

namespace GGDeals.Api.Services
{
	public interface ILibraryToGGLauncherMap
	{
		GGLauncher GetGGLauncher(Guid pluginId);
	}
}