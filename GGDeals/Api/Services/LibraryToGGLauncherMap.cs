using GGDeals.Api.Models;
using System;
using System.Collections.Generic;

namespace GGDeals.Api.Services
{
	public class LibraryToGGLauncherMap : ILibraryToGGLauncherMap
	{
		private readonly Dictionary<Guid, GGLauncher> _libraryIdMapping = new Dictionary<Guid, GGLauncher>()
		{
			{ Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"), GGLauncher.Steam },
			{ Guid.Parse("85DD7072-2F20-4E76-A007-41035E390724"), GGLauncher.EA },
			{ Guid.Parse("C2F038E5-8B92-4877-91F1-DA9094155FC5"), GGLauncher.Ubisoft },
			{ Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E"), GGLauncher.GOG },
			{ Guid.Parse("00000002-DBD1-46C6-B5D0-B1BA559D10E4"), GGLauncher.Epic },
			{ Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287"), GGLauncher.Microsoft },
			{ Guid.Parse("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD"), GGLauncher.BattleNet },
			{ Guid.Parse("88409022-088a-4de8-805a-fdbac291f00a"), GGLauncher.Rockstar },
			{ Guid.Parse("402674cd-4af6-4886-b6ec-0e695bfa0688"), GGLauncher.PrimeGaming },
			{ Guid.Parse("e4ac81cb-1b1a-4ec9-8639-9a9633989a71"), GGLauncher.Playstation },
			{ Guid.Parse("e4ac81cb-1b1a-4ec9-8639-9a9633989a72"), GGLauncher.Nintendo },
			{ Guid.Parse("00000001-EBB2-4EEC-ABCB-7C89937A42BB"), GGLauncher.Itch },
		};

		public GGLauncher GetGGLauncher(Guid pluginId)
		{
			if (_libraryIdMapping.TryGetValue(pluginId, out var ggLauncher))
			{
				return ggLauncher;
			}

			return GGLauncher.Other;
		}
	}
}