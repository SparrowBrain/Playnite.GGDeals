using System.Threading.Tasks;
using GGDeals.Website;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
    public class AddAGameService
    {
        private readonly IGGWebsite _ggWebsite;

        public AddAGameService(IGGWebsite ggWebsite)
        {
            _ggWebsite = ggWebsite;
        }

        public async Task<bool> TryAddToCollection(Game game)
        {
            if (!await _ggWebsite.TryNavigateToGamePage(game.Name, out var gamePage))
            {
                return false;
            }

            await gamePage.ClickOwnItButton();
            await gamePage.ExpandDrmDropDown();
            await gamePage.ClickDrmPlatformCheckBox(game);
            await gamePage.ClickSubmitOwnItForm();
            return true;
        }
    }
}