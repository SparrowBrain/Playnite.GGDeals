using System.Linq;
using Playnite.SDK.Models;

namespace GGDeals.Website.Url
{
    public class GamePageUrlGuesser : IGamePageUrlGuesser
    {
        private const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789-";
        private readonly IHomePageResolver _homePageResolver;

        public GamePageUrlGuesser(IHomePageResolver homePageResolver)
        {
            _homePageResolver = homePageResolver;
        }

        public string Resolve(Game game)
        {
            var homePage = _homePageResolver.Resolve();
            var gameName = game.Name;

            var gamePage = new string(gameName
                .ToLower()
                .Replace(" - ", "-")
                .Replace(" ", "-")
                .Where(c => AllowedCharacters.Contains(c))
                .ToArray());

            return $"{homePage}/game/{gamePage}/";
        }
    }
}