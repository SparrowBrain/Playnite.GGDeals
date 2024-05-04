using System;
using System.Threading.Tasks;

namespace GGDeals.Website
{
    public class HomePage : IHomePage
    {
        private readonly IAwaitableWebView _awaitableWebView;

        public HomePage(IAwaitableWebView awaitableWebView)
        {
            _awaitableWebView = awaitableWebView;
        }

        public async Task<bool> IsUserLoggedIn()
        {
            var loginCheck = await _awaitableWebView.EvaluateScriptAsync(@"$("".login"").length");
            if (!loginCheck.Success)
            {
                throw new Exception("Failed to check for login button.");
            }

            return (int)loginCheck.Result == 0;
        }

        public async Task<string> GetUserName()
        {
            var userNameSelect = await _awaitableWebView.EvaluateScriptAsync(@"$("".menu-profile-label"").contents().not($("".menu-profile-label"").children()).text()");
            if (!userNameSelect.Success)
            {
                throw new Exception("Failed to get the user name.");
            }

            return (string)userNameSelect.Result;
        }
    }
}