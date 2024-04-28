using System;
using System.Threading.Tasks;
using GGDeals.Website.Url;
using Playnite.SDK.Models;

namespace GGDeals.Website
{
    public class GGWebsite : IGGWebsite
    {
        private readonly IHomePageResolver _homePageResolver;
        private readonly IGamePageUrlGuesser _gamePageUrlGuesser;
        private readonly IAwaitableWebView _awaitableWebView;

        public GGWebsite(
            IHomePageResolver homePageResolver,
            IGamePageUrlGuesser gamePageUrlGuesser,
            IAwaitableWebView awaitableWebView)
        {
            _homePageResolver = homePageResolver;
            _gamePageUrlGuesser = gamePageUrlGuesser;
            _awaitableWebView = awaitableWebView;
        }

        public async Task CheckLoggedIn()
        {
            var homePage = _homePageResolver.Resolve();
            await _awaitableWebView.Navigate(homePage);
            var loginCheck = await _awaitableWebView.EvaluateScriptAsync(@"$("".login"").children(""a"").length");
            if (!loginCheck.Success)
            {
                throw new Exception("Failed to check for login button.");
            }

            if ((int)loginCheck.Result > 0)
            {
                throw new Exception("User not logged in!");
            }
        }

        public async Task<bool> TryNavigateToGamePage(Game game)
        {
            var url = _gamePageUrlGuesser.Resolve(game);
            await _awaitableWebView.Navigate(url);
            var error404Check = await _awaitableWebView.EvaluateScriptAsync(@"$("".error-404"").length");
            if (!error404Check.Success)
            {
                throw new Exception("Failed to check for 404 error page.");
            }

            return (int)error404Check.Result == 0;
        }
    }
}