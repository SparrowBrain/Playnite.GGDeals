using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GGDeals.Services;
using Newtonsoft.Json;

namespace GGDeals.Menu.Failures
{
    public class AddFailuresFileService : IAddFailuresFileService
    {
        private readonly string _failuresFilePath;

        public AddFailuresFileService(string failuresFilePath)
        {
            _failuresFilePath = failuresFilePath;
        }

        public async Task<Dictionary<Guid, AddToCollectionResult>> Load()
        {
            if (!File.Exists(_failuresFilePath))
            {
                return new Dictionary<Guid, AddToCollectionResult>();
            }

            using (var streamReader = new StreamReader(_failuresFilePath))
            {
                var contents = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Dictionary<Guid, AddToCollectionResult>>(contents);
            }
        }

        public async Task Save(Dictionary<Guid, AddToCollectionResult> failures)
        {
            using (var streamWriter = new StreamWriter(_failuresFilePath, false))
            {
                await streamWriter.WriteAsync(JsonConvert.SerializeObject(failures));
            }
        }
    }
}