using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GGDeals.Queue
{
	public class QueuePersistence : IQueuePersistence
	{
		private readonly string _queueFilePath;

		public QueuePersistence(string queueFilePath)
		{
			_queueFilePath = queueFilePath;
		}

		public async Task Save(IReadOnlyCollection<Guid> gameIds)
		{
			using (var streamWriter = new StreamWriter(_queueFilePath, false))
			{
				var file = new QueueFile() { GameIds = gameIds };
				await streamWriter.WriteAsync(JsonConvert.SerializeObject(file));
			}
		}

		public async Task<IReadOnlyCollection<Guid>> Load()
		{
			if (!File.Exists(_queueFilePath))
			{
				return new List<Guid>();
			}

			using (var streamReader = new StreamReader(_queueFilePath))
			{
				var contents = await streamReader.ReadToEndAsync();
				var file = JsonConvert.DeserializeObject<QueueFile>(contents);
				return file.GameIds;
			}
		}
	}
}