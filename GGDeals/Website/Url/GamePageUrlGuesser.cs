using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Playnite.SDK.Models;

namespace GGDeals.Website.Url
{
    public class GamePageUrlGuesser : IGamePageUrlGuesser
    {
        private const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789-";

        private readonly IHomePageResolver _homePageResolver;

        private readonly Dictionary<string, string> _anomalousNames = new Dictionary<string, string>()
        {
            { "2064: Read Only Memories", "read-only-memories" }
        };

        public GamePageUrlGuesser(IHomePageResolver homePageResolver)
        {
            _homePageResolver = homePageResolver;
        }

        public string Resolve(Game game)
        {
            var homePage = _homePageResolver.Resolve();
            var gameName = game.Name;

            if (!_anomalousNames.TryGetValue(gameName, out var gamePage))
            {
                gamePage = new string(gameName
                    .ToLower()
                    .Replace(" - ", "-")
                    .Replace(" ", "-")
                    .Where(c => AllowedCharacters.Contains(c))
                    .ToArray());
            }

            var consoleSuffix = GetConsoleSuffix(game);

            return $"{homePage}/game/{gamePage}{consoleSuffix}/";
        }

        private string GetConsoleSuffix(Game game)
        {
            if (game.Platforms == null)
            {
                return null;
            }

            if (game.Platforms.Any(x => x.SpecificationId == "sony_playstation4"))
            {
                return "-ps4";
            }

            if (game.Platforms.Any(x => x.SpecificationId == "sony_playstation5"))
            {
                return "-ps5";
            }

            if (game.Platforms.Any(x => x.SpecificationId == "nintendo_switch"))
            {
                return "-nintendo-switch";
            }

            return null;
        }
    }
}