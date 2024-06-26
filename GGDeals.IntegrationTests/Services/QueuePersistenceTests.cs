using AutoFixture.Xunit2;
using GGDeals.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace GGDeals.IntegrationTests.Services
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
		public async Task Load_ReturnsContentsOfFile_WhenFileExists(List<Guid> contents)
		{
			// Arrange
			File.WriteAllText(FailuresFilePath, JsonConvert.SerializeObject(contents));
			var sut = CreateSut();

			// Act
			var result = await sut.Load();

			// Assert
			Assert.Equal(contents, result);
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
			List<Guid> originalContents,
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

		private static List<Guid> ReadFile()
		{
			return JsonConvert.DeserializeObject<List<Guid>>(File.ReadAllText(FailuresFilePath));
		}
	}
}