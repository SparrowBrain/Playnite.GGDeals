using System.Threading.Tasks;
using GGDeals.Website;
using Playnite.SDK;

namespace GGDeals.Services
{
    public class AuthenticationStatusService
    {
        private readonly IGGWebsite _ggWebsite;
        private readonly IHomePage _homePage;

        public AuthenticationStatusService(
            IGGWebsite ggWebsite,
            IHomePage homePage)
        {
            _ggWebsite = ggWebsite;
            _homePage = homePage;
        }

        public async Task<string> GetAuthenticationStatus()
        {
            await _ggWebsite.NavigateToHomePage();
            if (!await _homePage.IsUserLoggedIn())
            {
                return ResourceProvider.GetString("LOC_GGDeals_SettingsNotAuthenticated");
            }

            var userName = await _homePage.GetUserName();
            return string.Format(ResourceProvider.GetString("LOC_GGDeals_SettingsAuthenticatedAs_Format"), userName);
        }
    }
}