using System.Linq;

namespace GGDeals.Website.Url
{
    public class GamePageUrlGuesser : IGamePageUrlGuesser
    {
        private readonly IHomePageResolver _homePageResolver;

        public GamePageUrlGuesser(IHomePageResolver homePageResolver)
        {
            _homePageResolver = homePageResolver;
        }

        public string Resolve(string gameName)
        {
            var allowedCharacters = "abcdefghijklmnopqrstuvwxyz0123456789-";
            var homePage = _homePageResolver.Resolve();

            var gamePage = new string(gameName
                .ToLower()
                .Replace(" - ", "-")
                .Replace(" ", "-")
                .Where(c => allowedCharacters.Contains(c))
                .ToArray());

            return $"{homePage}/game/{gamePage}/";
        }
    }
}