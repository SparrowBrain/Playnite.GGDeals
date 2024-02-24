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
            var homePage = _homePageResolver.Resolve();

            var gamePage = gameName
                .ToLower()
                .Replace(":", "")
                .Replace(" - ", "-")
                .Replace(" ", "-");

            return $"{homePage}/game/{gamePage}/";
        }
    }
}