using GGDeals.Api.Models;
using GGDeals.Settings;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GGDeals.Api.Services
{
	public class GGDealsApiClient : IDisposable
	{
		private static string _endpoint = "ACTUAL ENDPOINT GOES HERE?";
		private readonly HttpClient _httpClient;

		public GGDealsApiClient(GGDealsSettings settings)
		{
			_httpClient = new HttpClient();

#if DEBUG
			_endpoint = settings.DevCollectionImportEndpoint;
#endif
		}

		public async Task<ImportResponse> ImportGames(ImportRequest request)
		{
			var content = PrepareContent(request);

			var response = await _httpClient.PostAsync(_endpoint, content);
			response.EnsureSuccessStatusCode();

			return await ParseResponse(response);
		}

		private static StringContent PrepareContent(ImportRequest request)
		{
			var requestJson = JsonConvert.SerializeObject(request);
			var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
			return content;
		}

		private static async Task<ImportResponse> ParseResponse(HttpResponseMessage response)
		{
			var responseContent = await response.Content.ReadAsStringAsync();
			var importResponse = JsonConvert.DeserializeObject<ImportResponse>(responseContent);
			return importResponse;
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}