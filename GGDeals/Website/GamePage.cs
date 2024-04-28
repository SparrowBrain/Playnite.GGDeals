using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace GGDeals.Website
{
    public class GamePage : IGamePage
    {
        private readonly IAwaitableWebView _awaitableWebView;
        private readonly ILibraryNameMap _libraryNameMap;

        public GamePage(IAwaitableWebView awaitableWebView, ILibraryNameMap libraryNameMap)
        {
            _awaitableWebView = awaitableWebView;
            _libraryNameMap = libraryNameMap;
        }

        public async Task ClickOwnItButton()
        {
            await _awaitableWebView.Click(@"$("".owned-game.game-action-wrap "").first().find("".activate"")");
        }

        public async Task ExpandDrmDropDown()
        {
            await _awaitableWebView.Click(@"$(""#drm-collapse"").find(""a"").first()");
        }

        public async Task ClickDrmPlatformCheckBox(Game game)
        {
            var ggLibraryName = _libraryNameMap.GetGGLibraryName(game);
            await _awaitableWebView.Click($@"$(""#drm-collapse"").find("".filter-switch"").filter(""[data-name='{ggLibraryName}']"")");
            await Task.Delay(1000);
        }

        public async Task ClickSubmitOwnItForm()
        {
            await _awaitableWebView.Click(@"$(""button[type='submit']"").filter("".btn"")");
        }
    }
}