using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Events;

namespace GGDeals
{
    public partial class GGDealsSettingsView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _api;
        private AwaitableWebView _webView;

        public GGDealsSettingsView(IPlayniteAPI api)
        {
            _api = api;
            InitializeComponent();

            _webView = new AwaitableWebView(_api.WebViews.CreateOffscreenView());
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            logger.Info("Clicked");

            Task.Run(async () =>
            {
                try
                {
                    await _webView.Navigate("https://gg.deals/");

                    // Check we're logged in
                    var aaa = await RunScript<int>(@"$("".login"").children(""a"").length");
                    logger.Info(aaa.ToString());
                    if (aaa != 0)
                    {
                        logger.Error("We're not logged in");
                        return;
                    }

                    //////// Enter into global search
                    //////await RunScript(@"$(""#global-search-input"").text(""Outer Wilds"")");

                    //////// Submit search
                    //////await RunScript(@"$(""#global-search-form"").find("".search-submit"").click()");

                    //////// Get first result title
                    //////var title = await RunScript<string>(@"$(""a.game-info-title"").first().text()");
                    //////if (title != "Outer Wilds")
                    //////{
                    //////    logger.Error("Did not find Outer Wilds");
                    //////    return;
                    //////}

                    //////// Navigate to game page
                    //////await RunScript(@"window.location = $(""a.game-info-title"").first().attr(""href"")");

                    var gameTitle = "Outer Wilds";
                    var gamePage = gameTitle
                        .ToLower()
                        .Replace(":", "")
                        .Replace(" - ", "-")
                        .Replace(" ", "-");

                    // Navigate to game page
                    await _webView.Navigate($"https://gg.deals/game/{gamePage}/");

                    // Check not 404 page
                    var gameNotFound = await RunScript<int>(@"$("".error-404"").length");
                    logger.Info(aaa.ToString());
                    if (gameNotFound != 0)
                    {
                        logger.Error($"Game page {gamePage} not found");
                        return;
                    }

                    // Open Own It modal form
                    await RunScript(@"$("".owned-game.game-action-wrap "").first().find("".activate"").click()");

                    //////// Show DRM dropdown
                    //////await RunScript(@"$(""#drm-collapse"").first().click()");

                    //////// Wait for dropdown
                    //////while (true)
                    //////{
                    //////    var epicFound = await RunScript<int>(@"$("".filter-switch"").filter(""[data-name='Epic Games Launcher']"").length") > 0;
                    //////    if (epicFound)
                    //////    {
                    //////        break;
                    //////    }

                    //////    await Task.Delay(100);
                    //////}

                    //////// Select launchers
                    //////await RunScript(@"$("".filter-switch"").filter(""[data-name='Epic Games Launcher']"").children("".filter-checkbox"").click()");

                    // Submit add to collection form
                    await RunScript(@"$(""button[type='submit']"").filter("".btn"").click()");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error");
                }
            });
        }

        private async Task<T> RunScript<T>(string script)
        {
            var bbb = await _webView.EvaluateScriptAsync(script);
            logger.Info($"{script}: {bbb.Success}");
            return (T)bbb.Result;
        }

        private async Task RunScript(string script)
        {
            var bbb = await _webView.EvaluateScriptAsync(script);
            logger.Info($"{script}: {bbb.Success}");
        }
    }

    public class AwaitableWebView : IDisposable
    {
        private readonly IWebView _webView;
        private readonly SemaphoreSlim _semaphore;

        public AwaitableWebView(IWebView webView)
        {
            _webView = webView;
            _webView.LoadingChanged += OnWebViewLoadingChanged;
            _semaphore = new SemaphoreSlim(0, 1);
        }

        public async Task Navigate(string url)
        {
            _webView.Navigate(url);
            await _semaphore.WaitAsync();
        }

        public async Task<JavaScriptEvaluationResult> EvaluateScriptAsync(string script)
        {
            if (_webView.CanExecuteJavascriptInMainFrame)
            {
                return await _webView.EvaluateScriptAsync(script);
            }

            await _semaphore.WaitAsync();
            return await _webView.EvaluateScriptAsync(script);
        }

        private void OnWebViewLoadingChanged(object sender, WebViewLoadingChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _webView.Dispose();
        }
    }
}