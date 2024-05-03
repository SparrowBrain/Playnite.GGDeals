using System;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Events;

namespace GGDeals
{
    public class AwaitableWebView : IDisposable, IAwaitableWebView
    {
        private const double SecondsToPageLoadTimeout = 30;
        private readonly ILogger _logger = LogManager.GetLogger();
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
            await ResetSemaphoreToZero();
            _logger.Trace($"Navigating to {url}");
            _webView.Navigate(url);
            await _semaphore.WaitAsync(TimeSpan.FromSeconds(SecondsToPageLoadTimeout));
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

        public async Task WaitForElement(string jquerySelector)
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
            _logger.Trace($"Executing {script}");
            var result = await _webView.EvaluateScriptAsync(script);
            _logger.Trace($"Result: {{Success: {result.Success}, Result: {result.Result}, Message: {result.Message}}}");
            return result;
        }

        private void OnWebViewLoadingChanged(object sender, WebViewLoadingChangedEventArgs e)
        {
            _logger.Trace($"Loading changed: {{IsLoading: {e.IsLoading}}}");
            if (!e.IsLoading)
            {
                _semaphore.Release();
            }
        }

        private async Task ResetSemaphoreToZero()
        {
            while (_semaphore.CurrentCount > 0)
            {
                _logger.Trace($"Semaphore count: {_semaphore.CurrentCount}");
                await _semaphore.WaitAsync();
            }
            _logger.Trace($"Semaphore count: {_semaphore.CurrentCount}");
        }

        public void Dispose()
        {
            _webView.Dispose();
            _semaphore.Dispose();
        }
    }
}