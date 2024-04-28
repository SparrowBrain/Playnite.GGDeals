using System.Threading.Tasks;
using GGDeals.Website;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace GGDeals.Services
{
    public class AddAGameService : IAddAGameService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

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

            Logger.Info($"Added to GG.deals collection: {{ Id: {game.Id}, Name: {game.Name} }}.");
            return true;
        }
    }
}