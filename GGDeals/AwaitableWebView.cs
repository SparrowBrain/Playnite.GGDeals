using System;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Events;

namespace GGDeals
{
    public class AwaitableWebView : IDisposable
    {
        private ILogger _logger = LogManager.GetLogger();
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
            _logger.Trace($"Navigating to {url}");
            _webView.Navigate(url);
            await _semaphore.WaitAsync();
        }

        public async Task Click(string jquerySelector)
        {
            await WaitForElement(jquerySelector);

            var clickResult = await EvaluateScriptAsync($"{jquerySelector}.click()");
            if (clickResult.Success == false)
            {
                throw new Exception($"Failed to click {jquerySelector}");
            }

            _logger.Trace($"Clicked {jquerySelector}");
        }

        private async Task WaitForElement(string jquerySelector)
        {
            var timeout = DateTime.Now.AddSeconds(5);
            while (true)
            {
                var lengthResult = await EvaluateScriptAsync($"{jquerySelector}.length");
                if (lengthResult.Success == false)
                {
                    throw new Exception($"Failed to evaluate {jquerySelector}.length");
                }

                if ((int)lengthResult.Result > 0)
                {
                    return;
                }

                if (DateTime.Now > timeout)
                {
                    throw new Exception($"Timed out waiting for {jquerySelector}");
                }

                await Task.Delay(100);
            }
        }

        public async Task<JavaScriptEvaluationResult> EvaluateScriptAsync(string script)
        {
            if (_webView.CanExecuteJavascriptInMainFrame)
            {
                _logger.Trace($"Executing {script}");
                return await _webView.EvaluateScriptAsync(script);
            }

            _logger.Trace($"Waiting for page load");
            await _semaphore.WaitAsync();

            _logger.Trace($"Executing {script}");
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