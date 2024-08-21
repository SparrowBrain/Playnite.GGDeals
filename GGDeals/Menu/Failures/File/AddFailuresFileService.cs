using GGDeals.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GGDeals.Models;

namespace GGDeals.Menu.Failures.File
{
	public class AddFailuresFileService : IAddFailuresFileService
	{
		private readonly string _failuresFilePath;

		public AddFailuresFileService(string failuresFilePath)
		{
			_failuresFilePath = failuresFilePath;
		}

		public async Task<Dictionary<Guid, AddResult>> Load()
		{
			if (!System.IO.File.Exists(_failuresFilePath))
			{
				return new Dictionary<Guid, AddResult>();
			}

			using (var streamReader = new StreamReader(_failuresFilePath))
			{
				var contents = await streamReader.ReadToEndAsync();

				var file = JsonConvert.DeserializeObject<FailuresFile>(contents);
				if (file.Failures.Count == 0)
				{
					var versionedFile = JsonConvert.DeserializeObject<VersionedFailuresFile>(contents);
					if (versionedFile.Version == 0)
					{
						var fileV0 = JsonConvert.DeserializeObject<Dictionary<Guid, AddToCollectionResult>>(contents);
						return fileV0.ToDictionary(x => x.Key, x => new AddResult() { Result = x.Value });
					}
				}

				return file.Failures;
			}
		}

		public async Task Save(Dictionary<Guid, AddResult> failures)
		{
			var file = new FailuresFile() { Failures = failures };
			using (var streamWriter = new StreamWriter(_failuresFilePath, false))
			{
				await streamWriter.WriteAsync(JsonConvert.SerializeObject(file));
			}
		}
	}
}