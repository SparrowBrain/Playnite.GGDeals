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
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private static ILogger _logger = LogManager.GetLogger();
		private static string _endpoint = "ACTUAL ENDPOINT GOES HERE?";
		private readonly HttpClient _httpClient;

		public GGDealsApiClient(GGDealsSettings settings, JsonSerializerSettings jsonSerializerSettings)
		{
			_jsonSerializerSettings = jsonSerializerSettings;
			_httpClient = new HttpClient();
			_httpClient.Timeout = TimeSpan.FromMinutes(5);

#if DEBUG
			_endpoint = settings.DevCollectionImportEndpoint;
#endif
		}

		public async Task<ImportResponse> ImportGames(ImportRequest request, CancellationToken ct)
		{
			var content = PrepareContent(request);

			var response = await _httpClient.PostAsync(_endpoint, content, ct);
			var responseString = await response.Content.ReadAsStringAsync();
			_logger.Info($"Response from GG.Deals: Status: {response.StatusCode}; Body {responseString}");
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				throw new AuthenticationException(responseString);
			}

			response.EnsureSuccessStatusCode();

			return ParseResponse(responseString);
		}

		private StringContent PrepareContent(ImportRequest request)
		{
			var requestJson = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
			_logger.Debug($"Calling GG.Deals with body: {requestJson}");
			var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
			return content;
		}

		private ImportResponse ParseResponse(string responseContent)
		{
			var importResponse = JsonConvert.DeserializeObject<ImportResponse>(responseContent, _jsonSerializerSettings);
			return importResponse;
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}