using System.Threading.Tasks;
using GGDeals.Website;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
    public class AddAGameService
    {
        private readonly IGGWebsite _ggWebsite;
        private readonly IGamePage _gamePage;

        public AddAGameService(IGGWebsite ggWebsite, IGamePage gamePage)
        {
            _ggWebsite = ggWebsite;
            _gamePage = gamePage;
        }

        public async Task<bool> TryAddToCollection(Game game)
        {
            if (!await _ggWebsite.TryNavigateToGamePage(game))
            {
                return false;
            }

            await _gamePage.ClickOwnItButton();
            await _gamePage.ExpandDrmDropDown();
            await _gamePage.ClickDrmPlatformCheckBox(game);
            await _gamePage.ClickSubmitOwnItForm();
            return true;
        }
    }
}