using System.Threading.Tasks;
using Playnite.SDK;

namespace GGDeals
{
    public interface IAwaitableWebView
    {
        Task Navigate(string url);

        Task Click(string jquerySelector);

        Task<JavaScriptEvaluationResult> EvaluateScriptAsync(string script);
    }
}