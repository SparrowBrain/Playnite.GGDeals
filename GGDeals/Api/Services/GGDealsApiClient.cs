﻿using GGDeals.Api.Models;
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
	public class GGDealsApiClient : IGGDealsApiClient
	{
		private static readonly ILogger Logger = LogManager.GetLogger();
		private static string _endpoint = "https://api.gg.deals/playnite/collection/import/";
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly HttpClient _httpClient;

		public GGDealsApiClient(GGDealsSettings settings, JsonSerializerSettings jsonSerializerSettings)
		{
			_jsonSerializerSettings = jsonSerializerSettings;
			_httpClient = new HttpClient();
			_httpClient.Timeout = TimeSpan.FromMinutes(5);

#if DEBUG
			if (!string.IsNullOrWhiteSpace(settings.DevCollectionImportEndpoint))
			{
				_endpoint = settings.DevCollectionImportEndpoint;
			}
#endif
		}

		public async Task<ImportResponse> ImportGames(ImportRequest request, CancellationToken ct)
		{
			var content = PrepareContent(request);

			var response = await _httpClient.PostAsync(_endpoint, content, ct);
			var responseString = await response.Content.ReadAsStringAsync();
			Logger.Info($"Response from GG.Deals: Status: {response.StatusCode}; Body {responseString}");
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				throw new AuthenticationException(responseString);
			}

			return ParseResponse(responseString);
		}

		private StringContent PrepareContent(ImportRequest request)
		{
			var requestJson = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
			LogRequest(requestJson);

			var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
			return content;
		}

		private ImportResponse ParseResponse(string responseContent)
		{
			var importResponse = JsonConvert.DeserializeObject<ImportResponse>(responseContent, _jsonSerializerSettings);
			if (!importResponse.Success)
			{
				var failedResponse = JsonConvert.DeserializeObject<FailedImportResponse>(responseContent, _jsonSerializerSettings);
				throw new ApiException(failedResponse.Data.Message);
			}

			return importResponse;
		}

		private static void LogRequest(string requestJson)
		{
			var requestCopy = JsonConvert.DeserializeObject<ImportRequest>(requestJson);
			requestCopy.Token = "***REDACTED***";
			var jsonForLogging = JsonConvert.SerializeObject(requestCopy);
			Logger.Debug($"Calling GG.Deals with body: {jsonForLogging}");
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}