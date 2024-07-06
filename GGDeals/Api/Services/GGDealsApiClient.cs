using GGDeals.Api.Models;
using GGDeals.Settings;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GGDeals.Api.Services
{
	public class GGDealsApiClient : IDisposable, IGGDealsApiClient
	{
		private static ILogger _logger = LogManager.GetLogger();
		private static string _endpoint = "ACTUAL ENDPOINT GOES HERE?";
		private readonly HttpClient _httpClient;

		public GGDealsApiClient(GGDealsSettings settings)
		{
			_httpClient = new HttpClient();

#if DEBUG
			_endpoint = settings.DevCollectionImportEndpoint;
#endif
		}

		public async Task<ImportResponse> ImportGames(ImportRequest request, CancellationToken ct)
		{
			var content = PrepareContent(request);
			_logger.Debug($"Calling GG.Deals with body: {content}");

			var response = await _httpClient.PostAsync(_endpoint, content, ct);
			var responseString = await response.Content.ReadAsStringAsync();
			_logger.Info($"Response from GG.Deals: Status: {response.StatusCode} Body {responseString}");
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				throw new AuthenticationException("User is not logged in!");
			}

			response.EnsureSuccessStatusCode();

			return ParseResponse(responseString);
		}

		private static StringContent PrepareContent(ImportRequest request)
		{
			var requestJson = JsonConvert.SerializeObject(request);
			var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
			return content;
		}

		private static ImportResponse ParseResponse(string responseContent)
		{
			var importResponse = JsonConvert.DeserializeObject<ImportResponse>(responseContent);
			return importResponse;
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}