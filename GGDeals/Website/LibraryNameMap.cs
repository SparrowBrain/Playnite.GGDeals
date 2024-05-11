using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace GGDeals.Website
{
    public class LibraryNameMap : ILibraryNameMap
    {
        private readonly IPlayniteAPI _playniteApi;

        private readonly Dictionary<Guid, string> _libraryIdMapping = new Dictionary<Guid, string>()
        {
            { Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"), "Steam" },
            { Guid.Parse("85DD7072-2F20-4E76-A007-41035E390724"), "EA App" },
            { Guid.Parse("C2F038E5-8B92-4877-91F1-DA9094155FC5"), "Ubisoft Connect" },
            { Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E"), "GOG" },
            { Guid.Parse("00000002-DBD1-46C6-B5D0-B1BA559D10E4"), "Epic Games Launcher" },
            { Guid.Parse("7e4fbb5e-2ae3-48d4-8ba0-6b30e7a4e287"), "Microsoft Store" },
            { Guid.Parse("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD"), "Battle.net" },
            { Guid.Parse("88409022-088a-4de8-805a-fdbac291f00a"), "Rockstar Games Launcher" },
            { Guid.Parse("402674cd-4af6-4886-b6ec-0e695bfa0688"), "Prime Gaming" },
            { Guid.Parse("e4ac81cb-1b1a-4ec9-8639-9a9633989a71"), "PlayStation Network" },
            { Guid.Parse("e4ac81cb-1b1a-4ec9-8639-9a9633989a72"), "Nintendo eShop" },
            { Guid.Parse("00000001-EBB2-4EEC-ABCB-7C89937A42BB"), "Itch.io" },
        };

        private readonly Dictionary<string, string> _libraryNameMapping = new Dictionary<string, string>()
        {
            { "Steam", "Steam" },
            { "EA app", "EA App" },
            { "Ubisoft Connect", "Ubisoft Connect" },
            { "GOG", "GOG" },
            { "Epic", "Epic Games Launcher" },
            { "Xbox", "Microsoft Store" },
            { "Battle.net", "Battle.net" },
            { "Rockstar Games", "Rockstar Games Launcher" },
            { "Amazon Games", "Prime Gaming"},
            { "PlayStation", "PlayStation Network"},
            { "Nintendo", "Nintendo eShop"},
            { "itch.io", "Itch.io"},
        };

        public LibraryNameMap(IPlayniteAPI playniteApi)
        {
            _playniteApi = playniteApi;
        }

        public string GetGGLibraryName(Game game)
        {
            var playniteLibraryId = game.PluginId;
            if (_libraryIdMapping.TryGetValue(playniteLibraryId, out var ggLibraryName))
            {
                return ggLibraryName;
            }

            var playniteLibraryName = _playniteApi.Addons.Plugins.FirstOrDefault(x => x.Id == game.PluginId) is LibraryPlugin libraryPlugin
                ? libraryPlugin.Name
                : game.Source?.Name;

            if (playniteLibraryName == null || !_libraryNameMapping.TryGetValue(playniteLibraryName, out ggLibraryName))
            {
                ggLibraryName = "Other";
            }

            return ggLibraryName;
        }
    }
}