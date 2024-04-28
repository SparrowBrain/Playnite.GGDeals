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

        public LibraryNameMap(IPlayniteAPI playniteApi)
        {
            _playniteApi = playniteApi;
        }

        public string GetGGLibraryName(Game game)
        {
            var playniteLibraryName = _playniteApi.Addons.Plugins.FirstOrDefault(x => x.Id == game.PluginId) is LibraryPlugin libraryPlugin
                ? libraryPlugin.Name
                : game.Source.Name;

            var libraryMapping = new Dictionary<string, string>()
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

            if (!libraryMapping.TryGetValue(playniteLibraryName, out var ggLibraryName))
            {
                ggLibraryName = "Other";
            }

            return ggLibraryName;
        }
    }
}