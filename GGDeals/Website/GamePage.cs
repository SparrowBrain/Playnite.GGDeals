using System;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace GGDeals.Website
{
    public class GamePage : IGamePage
    {
        private readonly ILogger _logger = LogManager.GetLogger();

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

        public async Task<bool> IsDrmPlatformCheckboxActive(Game game)
        {
            var drmCheckboxSelector = GetDrmCheckboxSelector(game, out var ggLibraryName);
            var activeDrmCheckboxLengthSelector = $@"{drmCheckboxSelector}.filter("".active"").length";

            await _awaitableWebView.WaitForElement(drmCheckboxSelector);
            var activeDrmCheckboxResult = await _awaitableWebView.EvaluateScriptAsync(activeDrmCheckboxLengthSelector);
            if (!activeDrmCheckboxResult.Success)
            {
                throw new Exception("Active DRM checkbox check failed.");
            }

            if ((int)activeDrmCheckboxResult.Result > 0)
            {
                _logger.Debug($"Game {{ Id: {game.Id}, Name: {game.Name} }} already checked as owned with library {ggLibraryName}. Skipping.");
                return true;
            }

            return false;
        }

        public async Task ClickDrmPlatformCheckBox(Game game)
        {
            var drmCheckboxSelector = GetDrmCheckboxSelector(game, out _);

            await _awaitableWebView.Click(drmCheckboxSelector);
            await Task.Delay(1000);
        }

        public async Task ClickSubmitOwnItForm()
        {
            await _awaitableWebView.Click(@"$(""button[type='submit']"").filter("".btn"")");
        }

        private string GetDrmCheckboxSelector(Game game, out string ggLibraryName)
        {
            ggLibraryName = _libraryNameMap.GetGGLibraryName(game);
            var drmCheckboxSelector = $@"$(""#drm-collapse"").find("".filter-switch"").filter(""[data-name='{ggLibraryName}']"")";
            return drmCheckboxSelector;
        }
    }
}