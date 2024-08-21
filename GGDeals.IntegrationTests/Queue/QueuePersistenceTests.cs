using AutoFixture.Xunit2;
using GGDeals.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace GGDeals.IntegrationTests.Queue
{
	public class QueuePersistenceTests
	{
		private const string FailuresFilePath = "queue.json";

		[Fact]
		public async Task Load_ReturnsEmptyDictionary_WhenFileDoesNotExist()
		{
			// Arrange
			EnsureFileDoesNotExist();
			var sut = CreateSut();

			// Act
			var result = await sut.Load();

			// Assert
			Assert.Empty(result);
		}

		[Theory]
		[AutoData]
		public async Task Load_ReturnsContentsOfFile_WhenFileExists(QueueFile file)
		{
			// Arrange
			File.WriteAllText(FailuresFilePath, JsonConvert.SerializeObject(file));
			var sut = CreateSut();

			// Act
			var result = await sut.Load();

			// Assert
			Assert.Equal(file.GameIds, result);
		}

		[Theory]
		[AutoData]
		public async Task Save_CreatesNewFile_WhenFileDoesNotExist(
			List<Guid> newContents)
		{
			// Arrange
			EnsureFileDoesNotExist();
			var sut = CreateSut();

			// Act
			await sut.Save(newContents);

			// Assert
			var result = ReadFile();
			Assert.Equal(newContents, result);
		}

		[Theory]
		[AutoData]
		public async Task Save_OverwritesFile_WhenFileExists(
			QueueFile originalContents,
			List<Guid> newContents)
		{
			// Arrange
			File.WriteAllText(FailuresFilePath, JsonConvert.SerializeObject(originalContents));
			var sut = CreateSut();

			// Act
			await sut.Save(newContents);

			// Assert
			var result = ReadFile();
			Assert.Equal(newContents, result);
		}

		private QueuePersistence CreateSut()
		{
			return new QueuePersistence(FailuresFilePath);
		}

		private void EnsureFileDoesNotExist()
		{
			if (File.Exists(FailuresFilePath))
			{
				File.Delete(FailuresFilePath);
			}
		}

		private static IReadOnlyCollection<Guid> ReadFile()
		{
			var file = JsonConvert.DeserializeObject<QueueFile>(File.ReadAllText(FailuresFilePath));
			return file.GameIds;
		}
	}
}